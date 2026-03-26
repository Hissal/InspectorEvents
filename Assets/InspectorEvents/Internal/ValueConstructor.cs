using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InspectorEvents.Internal;

internal interface IValueConstructor {
    Type ValueType { get; }
    object? ConstructValueBoxed();
    void Rebuild();
    string? LastError { get; }
    
    public static IValueConstructor Create(Type type) {
        var constructorType = typeof(ValueConstructor<>).MakeGenericType(type);
        return (IValueConstructor)Activator.CreateInstance(constructorType, nonPublic: true)!;
    }
}

[Serializable]
internal sealed class ValueConstructor<T> : IValueConstructor {
    [SerializeField, HideInInspector] string valueTypeName = typeof(T).AssemblyQualifiedName ?? typeof(T).FullName ?? typeof(T).Name;

    [SerializeField, BoxGroup("Value"), ShowIf(nameof(UseDirectValue)), HideLabel]
    SupportedValue directValue = new();

    [SerializeField, BoxGroup("Constructor"), HideLabel, HideIf(nameof(UseDirectValue))]
    [ValueDropdown(nameof(GetConstructorOptions))]
    int selectedConstructorIndex;

    [SerializeField, ListDrawerSettings(
        ShowFoldout =  true,
        DraggableItems = false,
        HideAddButton = true,
        HideRemoveButton = true
    ), HideIf(nameof(UseDirectValue))]
    List<ParameterValue> constructorArguments = new();

    [SerializeField, ListDrawerSettings(
         ShowFoldout =  true,
         DraggableItems = false,
         HideAddButton = true,
         HideRemoveButton = true
     ), HideIf(nameof(UseDirectValue))]
    List<MemberValue> memberAssignments = new();

    [SerializeField, TextArea, ReadOnly]
    string? lastError;

    public Type ValueType => typeof(T);
    public string? LastError => string.IsNullOrWhiteSpace(lastError) ? null : lastError;

    bool UseDirectValue => SupportedValue.IsSimpleSupported(typeof(T));

    internal ValueConstructor() {
        Rebuild();
    }

    public void Rebuild() {
        valueTypeName = typeof(T).AssemblyQualifiedName ?? typeof(T).FullName ?? typeof(T).Name;

        if (UseDirectValue) {
            directValue.InitializeFor(typeof(T));
            selectedConstructorIndex = -1;
            constructorArguments.Clear();
            memberAssignments.Clear();
            return;
        }

        var constructorPlans = TypePlanCache.ConstructorPlans;
        if (constructorPlans.Count == 0) {
            selectedConstructorIndex = -1;
            constructorArguments.Clear();
            EnsureMemberSlots();
            return;
        }

        if (selectedConstructorIndex < 0 || selectedConstructorIndex >= constructorPlans.Count) {
            selectedConstructorIndex = 0;
        }

        EnsureConstructorSlots(constructorPlans[selectedConstructorIndex]);
        EnsureMemberSlots();
    }

    public T ConstructValue() {
        var boxed = ConstructValueBoxed();
        return boxed is T typed ? typed : default!;
    }

    public object? ConstructValueBoxed() {
        lastError = null;

        if (UseDirectValue) {
            directValue.InitializeFor(typeof(T));
            if (!directValue.TryResolve(typeof(T), typeof(T).Name, out var directResult, out var error)) {
                lastError = error;
                return null;
            }

            return directResult;
        }

        var constructors = TypePlanCache.ConstructorPlans;
        var members = TypePlanCache.MemberPlans;

        object? value;
        if (constructors.Count == 0) {
            value = GetDefaultValue(typeof(T));
        }
        else {
            var ctor = constructors[Mathf.Clamp(selectedConstructorIndex, 0, constructors.Count - 1)];
            var args = new object?[ctor.Parameters.Length];

            for (var i = 0; i < ctor.Parameters.Length; i++) {
                if (i >= constructorArguments.Count) {
                    lastError = $"Missing value for ctor argument '{ctor.Parameters[i].Name}'.";
                    return null;
                }

                var slot = constructorArguments[i];
                if (!slot.TryResolve(ctor.Parameters[i].ParameterType, out var arg, out var error)) {
                    lastError = error;
                    return null;
                }

                args[i] = arg;
            }

            try {
                value = ctor.Constructor.Invoke(args);
            }
            catch (Exception e) {
                lastError = $"Failed to invoke constructor for '{typeof(T).Name}'. {e.Message}";
                return null;
            }
        }

        if (value == null) {
            return null;
        }

        foreach (var memberPlan in members) {
            var assignment = memberAssignments.FirstOrDefault(x => x.MemberName == memberPlan.Name);
            if (assignment == null || !assignment.Enabled) {
                continue;
            }

            if (!assignment.TryResolve(memberPlan.MemberType, out var memberValue, out var error)) {
                lastError = error;
                return null;
            }

            try {
                memberPlan.Assign(ref value, memberValue);
            }
            catch (Exception e) {
                lastError = $"Failed assigning member '{memberPlan.Name}' on '{typeof(T).Name}'. {e.Message}";
                return null;
            }
        }

        return value;
    }

    IEnumerable<ValueDropdownItem<int>> GetConstructorOptions() {
        for (var i = 0; i < TypePlanCache.ConstructorPlans.Count; i++) {
            yield return new ValueDropdownItem<int>(TypePlanCache.ConstructorPlans[i].DisplayName, i);
        }
    }

    void EnsureConstructorSlots(ConstructorPlan plan) {
        var previous = constructorArguments.ToDictionary(x => x.ParameterName, x => x);
        constructorArguments.Clear();

        foreach (var parameter in plan.Parameters) {
            if (previous.TryGetValue(parameter.Name, out var existing)) {
                existing.Reinitialize(parameter.Name, parameter.ParameterType);
                constructorArguments.Add(existing);
            }
            else {
                constructorArguments.Add(new ParameterValue(parameter.Name, parameter.ParameterType));
            }
        }
    }

    void EnsureMemberSlots() {
        var previous = memberAssignments.ToDictionary(x => x.MemberName, x => x);
        memberAssignments.Clear();

        foreach (var member in TypePlanCache.MemberPlans) {
            if (previous.TryGetValue(member.Name, out var existing)) {
                existing.Reinitialize(member.Name, member.MemberType);
                memberAssignments.Add(existing);
            }
            else {
                memberAssignments.Add(new MemberValue(member.Name, member.MemberType));
            }
        }
    }

    static object? GetDefaultValue(Type type) {
        if (type.IsValueType) {
            return Activator.CreateInstance(type);
        }

        return null;
    }

    static class TypePlanCache {
        public static readonly List<ConstructorPlan> ConstructorPlans = BuildConstructors();
        public static readonly List<MemberPlan> MemberPlans = BuildMembers();

        static List<ConstructorPlan> BuildConstructors() {
            var type = typeof(T);
            var ctors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .Select(c => new ConstructorPlan(c))
                .Where(c => c.Parameters.All(p => SupportedValue.IsSupported(p.ParameterType)))
                .OrderByDescending(c => c.Parameters.Length)
                .ToList();

            if (ctors.Count > 0) {
                return ctors;
            }

            var hasPublicParameterless = type.GetConstructor(Type.EmptyTypes) != null;
            if (hasPublicParameterless || type.IsValueType) {
                var fallbackCtor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(c => c.GetParameters().Length == 0);

                if (fallbackCtor != null) {
                    return new List<ConstructorPlan> { new ConstructorPlan(fallbackCtor) };
                }
            }

            return new List<ConstructorPlan>();
        }

        static List<MemberPlan> BuildMembers() {
            var type = typeof(T);
            var plans = new List<MemberPlan>();

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public)) {
                if (field.IsStatic || field.IsInitOnly || field.IsLiteral || !SupportedValue.IsSupported(field.FieldType)) {
                    continue;
                }

                plans.Add(MemberPlan.ForField(field));
            }

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                if (property.GetIndexParameters().Length != 0) {
                    continue;
                }

                var setter = property.SetMethod;
                if (setter == null || setter.IsStatic || !setter.IsPublic || !SupportedValue.IsSupported(property.PropertyType)) {
                    continue;
                }

                plans.Add(MemberPlan.ForProperty(property));
            }

            return plans;
        }
    }

    [Serializable]
    sealed class ParameterValue {
        [SerializeField, HideInInspector] string parameterName;
        [SerializeField, DisplayAsString(false), HideLabel] string parameterLabel = string.Empty;
        [SerializeField, InlineProperty, HideLabel] SupportedValue value = new();

        public string ParameterName => parameterName;

        public ParameterValue(string parameterName, Type parameterType) {
            this.parameterName = parameterName;
            parameterLabel = BuildLabel(parameterName, parameterType);
            value.InitializeFor(parameterType);
        }

        public void Reinitialize(string name, Type parameterType) {
            parameterName = name;
            parameterLabel = BuildLabel(name, parameterType);
            value.InitializeFor(parameterType);
        }

        public bool TryResolve(Type targetType, out object? result, out string? error) {
            return value.TryResolve(targetType, parameterName, out result, out error);
        }

        static string BuildLabel(string name, Type type) {
            var t = Nullable.GetUnderlyingType(type) ?? type;
            return $"{name} ({t.Name})";
        }
    }

    [Serializable]
    sealed class MemberValue {
        [SerializeField, HorizontalGroup("Label", Width = 20f), HideLabel] bool enabled;
        [SerializeField, HideInInspector] string memberName;
        [SerializeField, DisplayAsString(false), HorizontalGroup("Label"), HideLabel, EnableIf(nameof(enabled))] string memberLabel = string.Empty;
        [SerializeField, InlineProperty, HideLabel, EnableIf(nameof(enabled))] SupportedValue value = new();

        public bool Enabled => enabled;
        public string MemberName => memberName;

        public MemberValue(string memberName, Type memberType) {
            enabled = false;
            this.memberName = memberName;
            memberLabel = BuildLabel(memberName, memberType);
            value.InitializeFor(memberType);
        }

        public void Reinitialize(string name, Type memberType) {
            memberName = name;
            memberLabel = BuildLabel(name, memberType);
            value.InitializeFor(memberType);
        }

        public bool TryResolve(Type targetType, out object? result, out string? error) {
            return value.TryResolve(targetType, memberName, out result, out error);
        }

        static string BuildLabel(string name, Type type) {
            var t = Nullable.GetUnderlyingType(type) ?? type;
            return $"{name} ({t.Name})";
        }
    }

    readonly struct ConstructorPlan {
        public readonly ConstructorInfo Constructor;
        public readonly ParameterInfo[] Parameters;
        public readonly string DisplayName;

        public ConstructorPlan(ConstructorInfo constructor) {
            Constructor = constructor;
            Parameters = constructor.GetParameters();
            DisplayName = $"{typeof(T).Name}({string.Join(", ", Parameters.Select(p => p.ParameterType.Name + " " + p.Name))})";
        }
    }

    sealed class MemberPlan {
        readonly FieldInfo? _field;
        readonly PropertyInfo? _property;

        public string Name { get; }
        public Type MemberType { get; }

        MemberPlan(FieldInfo field) {
            _field = field;
            Name = field.Name;
            MemberType = field.FieldType;
        }

        MemberPlan(PropertyInfo property) {
            _property = property;
            Name = property.Name;
            MemberType = property.PropertyType;
        }

        public static MemberPlan ForField(FieldInfo field) => new(field);
        public static MemberPlan ForProperty(PropertyInfo property) => new(property);

        public void Assign(ref object target, object? value) {
            if (_field != null) {
                _field.SetValue(target, value);
                return;
            }

            _property!.SetValue(target, value);
        }
    }

    [Serializable]
    sealed class SupportedValue {
        enum SupportedKind {
            Unsupported,
            Bool,
            Int,
            Float,
            String,
            Long,
            Double,
            UnityObject,
            Vector2,
            Vector3,
            Vector4,
            Color,
            Enum,
            SerializedClass,
            NestedStruct,
            NestedClass
        }

        [SerializeField, HideInInspector] string targetTypeName = string.Empty;

        [SerializeField, ShowIf(nameof(IsBool)), HideLabel] bool boolValue;
        [SerializeField, ShowIf(nameof(IsInt)), HideLabel] int intValue;
        [SerializeField, ShowIf(nameof(IsFloat)), HideLabel] float floatValue;
        [SerializeField, ShowIf(nameof(IsString)), HideLabel] string stringValue = string.Empty;
        [SerializeField, ShowIf(nameof(IsLong)), HideLabel] long longValue;
        [SerializeField, ShowIf(nameof(IsDouble)), HideLabel] double doubleValue;
        [SerializeField, ShowIf(nameof(IsUnityObject)), HideLabel] UnityEngine.Object? objectReference;
        [SerializeField, ShowIf(nameof(IsVector2)), HideLabel] Vector2 vector2Value;
        [SerializeField, ShowIf(nameof(IsVector3)), HideLabel] Vector3 vector3Value;
        [SerializeField, ShowIf(nameof(IsVector4)), HideLabel] Vector4 vector4Value;
        [SerializeField, ShowIf(nameof(IsColor)), HideLabel] Color colorValue = Color.white;
        [SerializeField, ShowIf(nameof(IsEnum)), HideLabel]
        [ValueDropdown(nameof(GetEnumOptions))]
        string enumName = string.Empty;
        [SerializeReference, ShowIf(nameof(IsSerializedClass)), HideLabel]
        object? serializableObject;
        [SerializeReference, ShowIf(nameof(UseNestedConstructor)), HideLabel]
        IValueConstructor? nestedValueConstructor;

        Type? TargetType {
            get {
                if (string.IsNullOrEmpty(targetTypeName)) {
                    return null;
                }

                return Type.GetType(targetTypeName, throwOnError: false);
            }
        }

        SupportedKind Kind {
            get {
                var type = Nullable.GetUnderlyingType(TargetType ?? typeof(void)) ?? TargetType;
                if (type == null) return SupportedKind.Unsupported;
                if (type.IsEnum) return SupportedKind.Enum;
                if (typeof(UnityEngine.Object).IsAssignableFrom(type)) return SupportedKind.UnityObject;
                if (type == typeof(bool)) return SupportedKind.Bool;
                if (type == typeof(int)) return SupportedKind.Int;
                if (type == typeof(float)) return SupportedKind.Float;
                if (type == typeof(string)) return SupportedKind.String;
                if (type == typeof(long)) return SupportedKind.Long;
                if (type == typeof(double)) return SupportedKind.Double;
                if (type == typeof(Vector2)) return SupportedKind.Vector2;
                if (type == typeof(Vector3)) return SupportedKind.Vector3;
                if (type == typeof(Vector4)) return SupportedKind.Vector4;
                if (type == typeof(Color)) return SupportedKind.Color;
                if (IsStructType(type)) return SupportedKind.NestedStruct;
                if (IsSerializableClassType(type)) return SupportedKind.SerializedClass;
                if (IsClassType(type)) return SupportedKind.NestedClass;
                return SupportedKind.Unsupported;
            }
        }

        bool IsBool => Kind == SupportedKind.Bool;
        bool IsInt => Kind == SupportedKind.Int;
        bool IsFloat => Kind == SupportedKind.Float;
        bool IsString => Kind == SupportedKind.String;
        bool IsLong => Kind == SupportedKind.Long;
        bool IsDouble => Kind == SupportedKind.Double;
        bool IsUnityObject => Kind == SupportedKind.UnityObject;
        bool IsVector2 => Kind == SupportedKind.Vector2;
        bool IsVector3 => Kind == SupportedKind.Vector3;
        bool IsVector4 => Kind == SupportedKind.Vector4;
        bool IsColor => Kind == SupportedKind.Color;
        bool IsEnum => Kind == SupportedKind.Enum;
        bool IsSerializedClass => Kind == SupportedKind.SerializedClass;
        bool UseNestedConstructor => Kind == SupportedKind.NestedStruct || Kind == SupportedKind.NestedClass;

        IEnumerable<ValueDropdownItem<string>> GetEnumOptions() {
            var enumType = Nullable.GetUnderlyingType(TargetType ?? typeof(void)) ?? TargetType;
            if (enumType == null || !enumType.IsEnum) {
                yield break;
            }

            foreach (var name in Enum.GetNames(enumType)) {
                yield return new ValueDropdownItem<string>(name, name);
            }
        }

        public static bool IsSimpleSupported(Type type) {
            var t = Nullable.GetUnderlyingType(type) ?? type;
            if (t.IsEnum) return true;
            if (typeof(UnityEngine.Object).IsAssignableFrom(t)) return true;

            return t == typeof(bool)
                   || t == typeof(int)
                   || t == typeof(float)
                   || t == typeof(string)
                   || t == typeof(long)
                   || t == typeof(double)
                   || t == typeof(Vector2)
                   || t == typeof(Vector3)
                   || t == typeof(Vector4)
                   || t == typeof(Color);
        }

        public static bool IsSupported(Type type) {
            var t = Nullable.GetUnderlyingType(type) ?? type;
            if (t == typeof(void) || t.IsPointer || t.IsByRef || t.ContainsGenericParameters) {
                return false;
            }

            return IsSimpleSupported(t) || IsStructType(t) || IsClassType(t);
        }

        static bool IsClassType(Type type) {
            return type.IsClass && type != typeof(string);
        }

        static bool IsSerializableClassType(Type type) {
            return IsClassType(type) && type.IsSerializable;
        }

        static bool IsStructType(Type type) {
            return type.IsValueType && !type.IsPrimitive && !type.IsEnum;
        }

        public void InitializeFor(Type type) {
            targetTypeName = type.AssemblyQualifiedName ?? type.FullName ?? type.Name;
            var resolvedType = Nullable.GetUnderlyingType(type) ?? type;

            if (resolvedType.IsEnum && string.IsNullOrEmpty(enumName)) {
                enumName = Enum.GetNames(resolvedType).FirstOrDefault() ?? string.Empty;
            }

            var shouldUseNestedConstructor = IsStructType(resolvedType) || (IsClassType(resolvedType) && !IsSerializableClassType(resolvedType));
            if (shouldUseNestedConstructor) {
                if (nestedValueConstructor == null || nestedValueConstructor.ValueType != resolvedType) {
                    nestedValueConstructor = IValueConstructor.Create(resolvedType);
                }
            }
            else {
                nestedValueConstructor = null;
            }

            if (IsSerializableClassType(resolvedType)) {
                if (serializableObject == null || !resolvedType.IsInstanceOfType(serializableObject)) {
                    try {
                        serializableObject = Activator.CreateInstance(resolvedType, nonPublic: true);
                    }
                    catch {
                        serializableObject = null;
                    }
                }
            }
            else {
                serializableObject = null;
            }
        }

        public bool TryResolve(Type targetType, string slotName, out object? value, out string? error) {
            var t = Nullable.GetUnderlyingType(targetType) ?? targetType;

            try {
                if (t.IsEnum) {
                    value = Enum.Parse(t, enumName, true);
                    error = null;
                    return true;
                }

                if (typeof(UnityEngine.Object).IsAssignableFrom(t)) {
                    if (objectReference == null && targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null) {
                        value = null;
                        error = $"Slot '{slotName}' does not allow null.";
                        return false;
                    }

                    if (objectReference != null && !t.IsInstanceOfType(objectReference)) {
                        value = null;
                        error = $"Slot '{slotName}' expects '{t.Name}', got '{objectReference.GetType().Name}'.";
                        return false;
                    }

                    value = objectReference;
                    error = null;
                    return true;
                }

                if (t == typeof(bool)) {
                    value = boolValue;
                    error = null;
                    return true;
                }

                if (t == typeof(int)) {
                    value = intValue;
                    error = null;
                    return true;
                }

                if (t == typeof(float)) {
                    value = floatValue;
                    error = null;
                    return true;
                }

                if (t == typeof(string)) {
                    value = stringValue;
                    error = null;
                    return true;
                }

                if (t == typeof(long)) {
                    value = longValue;
                    error = null;
                    return true;
                }

                if (t == typeof(double)) {
                    value = doubleValue;
                    error = null;
                    return true;
                }

                if (t == typeof(Vector2)) {
                    value = vector2Value;
                    error = null;
                    return true;
                }

                if (t == typeof(Vector3)) {
                    value = vector3Value;
                    error = null;
                    return true;
                }

                if (t == typeof(Vector4)) {
                    value = vector4Value;
                    error = null;
                    return true;
                }

                if (t == typeof(Color)) {
                    value = colorValue;
                    error = null;
                    return true;
                }

                if (IsSerializableClassType(t)) {
                    if (serializableObject != null && !t.IsInstanceOfType(serializableObject)) {
                        value = null;
                        error = $"Slot '{slotName}' expects '{t.Name}', got '{serializableObject.GetType().Name}'.";
                        return false;
                    }

                    value = serializableObject;
                    error = null;
                    return true;
                }

                if (IsStructType(t) || (IsClassType(t) && !IsSerializableClassType(t))) {
                    if (nestedValueConstructor == null || nestedValueConstructor.ValueType != t) {
                        nestedValueConstructor = IValueConstructor.Create(t);
                    }

                    value = nestedValueConstructor.ConstructValueBoxed();
                    if (value == null) {
                        if (!t.IsValueType) {
                            error = null;
                            return true;
                        }

                        error = nestedValueConstructor.LastError ?? $"Slot '{slotName}' could not construct '{t.Name}'.";
                        return false;
                    }

                    if (!t.IsInstanceOfType(value) && !(t.IsValueType && value.GetType() == t)) {
                        error = $"Slot '{slotName}' expected '{t.Name}', got '{value.GetType().Name}'.";
                        value = null;
                        return false;
                    }

                    error = null;
                    return true;
                }
            }
            catch (Exception e) {
                value = null;
                error = $"Slot '{slotName}' could not be converted to '{targetType.Name}'. {e.Message}";
                return false;
            }

            value = null;
            error = $"Slot '{slotName}' type '{targetType.Name}' is not supported by ValueConstructor.";
            return false;
        }
    }
}