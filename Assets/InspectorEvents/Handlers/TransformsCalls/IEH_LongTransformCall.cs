using System;
using InspectorEvents.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InspectorEvents.Handlers;

[Serializable]
public sealed class IEH_LongTransformCall : IInspectorEventHandler<long> {
#if UNITY_EDITOR
    [ShowInInspector, ReadOnly, PropertyOrder(-1), LabelText("Summary")]
    string Summary => GetSummary();
#endif

    [SerializeField]
    NumericTransformMode mode = NumericTransformMode.Add;

    [SerializeField, ShowIf(nameof(IsSetMode)), LabelText("Value")]
    long setValue;
    [SerializeField, ShowIf(nameof(IsAddMode)), LabelText("Operand")]
    long addValue;
    [SerializeField, ShowIf(nameof(IsMultiplyMode)), LabelText("Factor")]
    float multiplyFactor = 1f;
    [SerializeField, ShowIf(nameof(IsClampRangeMode)), LabelText("Min"), OnValueChanged(nameof(NormalizeClampBounds))]
    long clampMin;
    [SerializeField, ShowIf(nameof(IsClampRangeMode)), LabelText("Max"), OnValueChanged(nameof(NormalizeClampBounds))]
    long clampMax;

    [PropertySpace]
    [SerializeReference, LabelText("Handlers")]
    // ReSharper disable once UseArrayEmptyMethod disabled due to SerializeReferece actually serializing the empty array instance
    IInspectorEventHandler<long>[] handlers = new IInspectorEventHandler<long>[0];

    bool IsSetMode => mode == NumericTransformMode.Set;
    bool IsAddMode => mode == NumericTransformMode.Add;
    bool IsMultiplyMode => mode == NumericTransformMode.Multiply;
    bool IsClampRangeMode => mode == NumericTransformMode.ClampRange;

    public void Handle(in long value) {
        var transformed = Transform(value);
        foreach (var handler in handlers) {
            handler.Handle(transformed);
        }
    }

#if UNITY_EDITOR
    public string GetSummary() {
        return NumericTransformUtility.BuildIntegerSummary(
            "Long Transform",
            mode,
            setValue,
            addValue,
            multiplyFactor,
            clampMin,
            clampMax
        );
    }
#endif

    long Transform(long value) {
        return mode switch {
            NumericTransformMode.Set => setValue,
            NumericTransformMode.Add => unchecked(value + addValue),
            NumericTransformMode.Multiply => NumericTransformUtility.MultiplyAndRound(value, multiplyFactor),
            NumericTransformMode.ClampRange => NumericTransformUtility.Clamp(value, clampMin, clampMax),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    void NormalizeClampBounds() {
        if (clampMin > clampMax) {
            (clampMin, clampMax) = (clampMax, clampMin);
        }
    }
}
