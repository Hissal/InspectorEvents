using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using InspectorEvents.Core;
using NUnit.Framework;
using UnityEngine;

namespace InspectorEvents.Tests.Editor {
    public sealed class InspectorSemanticEventComponentTests {
        const BindingFlags c_instanceFieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        readonly List<GameObject> _createdObjects = new();

        [TearDown]
        public void TearDown() {
            foreach (var gameObject in _createdObjects) {
                if (gameObject != null) {
                    UnityEngine.Object.DestroyImmediate(gameObject);
                }
            }

            _createdObjects.Clear();
        }

        [Test]
        public void CoreCallbackEnums_UseValidFlags() {
            AssertValidFlags(InspectorGameObjectLifecycleCallbacks.None, InspectorGameObjectLifecycleCallbacks.All);
            AssertValidFlags(InspectorApplicationCallbacks.None, InspectorApplicationCallbacks.All);
            AssertValidFlags(InspectorTickCallbacks.None, InspectorTickCallbacks.All);
            AssertValidFlags(InspectorTransformCallbacks.None, InspectorTransformCallbacks.All);
            AssertValidFlags(InspectorVisibilityCallbacks.None, InspectorVisibilityCallbacks.All);
            AssertValidFlags(InspectorObjectMouseCallbacks.None, InspectorObjectMouseCallbacks.All);
        }

        [Test]
        public void GameObjectLifecycle_InvokesEnabledCallbackEvent() {
            var capture = new CaptureHandler();
            var listener = CreateComponent<InspectorGameObjectLifecycleEvents>();
            SetField(listener, "callbacks", InspectorGameObjectLifecycleCallbacks.OnEnable);
            SetInspectorEventHandlers(GetField<InspectorEvent>(listener, "onEnable"), capture);

            listener.Invoke(InspectorGameObjectLifecycleCallbacks.OnEnable);

            Assert.That(capture.Calls, Is.EqualTo(new[] { "OnEnable" }));
        }

        [Test]
        public void GameObjectLifecycle_DoesNotInvokeDisabledCallbackEvent() {
            var capture = new CaptureHandler();
            var listener = CreateComponent<InspectorGameObjectLifecycleEvents>();
            SetField(listener, "callbacks", InspectorGameObjectLifecycleCallbacks.None);
            SetInspectorEventHandlers(GetField<InspectorEvent>(listener, "onEnable"), capture);

            listener.Invoke(InspectorGameObjectLifecycleCallbacks.OnEnable);

            Assert.That(capture.Calls, Is.Empty);
        }

        [Test]
        public void GameObjectLifecycle_RunsMultipleHandlersInOneInspectorEvent_InOrder() {
            var calls = new List<string>();
            var listener = CreateComponent<InspectorGameObjectLifecycleEvents>();
            SetField(listener, "callbacks", InspectorGameObjectLifecycleCallbacks.OnEnable);
            SetInspectorEventHandlers(
                GetField<InspectorEvent>(listener, "onEnable"),
                new CaptureHandler(calls, "First"),
                new CaptureHandler(calls, "Second")
            );

            listener.Invoke(InspectorGameObjectLifecycleCallbacks.OnEnable);

            Assert.That(calls, Is.EqualTo(new[] { "First", "Second" }));
        }

        [Test]
        public void Application_RoutesBoolPayload() {
            var capture = new CaptureHandler<bool>();
            var listener = CreateComponent<InspectorApplicationEvents>();
            SetField(listener, "callbacks", InspectorApplicationCallbacks.OnApplicationFocus);
            SetInspectorEventHandlers(GetField<InspectorEvent<bool>>(listener, "onApplicationFocus"), capture);

            listener.InvokeFocus(true);

            Assert.That(capture.InvocationCount, Is.EqualTo(1));
            Assert.That(capture.LastValue, Is.True);
        }

        [Test]
        public void Tick_InvokesEnabledCallbackEvent() {
            var capture = new CaptureHandler();
            var listener = CreateComponent<InspectorTickEvents>();
            SetField(listener, "callbacks", InspectorTickCallbacks.FixedUpdate);
            SetInspectorEventHandlers(GetField<InspectorEvent>(listener, "onFixedUpdate"), capture);

            listener.Invoke(InspectorTickCallbacks.FixedUpdate);

            Assert.That(capture.Calls, Is.EqualTo(new[] { "OnEnable" }));
        }

        [Test]
        public void Transform_InvokesEnabledCallbackEvent() {
            var capture = new CaptureHandler();
            var listener = CreateComponent<InspectorTransformEvents>();
            SetField(listener, "callbacks", InspectorTransformCallbacks.OnTransformParentChanged);
            SetInspectorEventHandlers(GetField<InspectorEvent>(listener, "onTransformParentChanged"), capture);

            listener.Invoke(InspectorTransformCallbacks.OnTransformParentChanged);

            Assert.That(capture.Calls, Is.EqualTo(new[] { "OnEnable" }));
        }

        [Test]
        public void Visibility_InvokesEnabledCallbackEvent() {
            var capture = new CaptureHandler();
            var listener = CreateComponent<InspectorVisibilityEvents>();
            SetField(listener, "callbacks", InspectorVisibilityCallbacks.OnBecameVisible);
            SetInspectorEventHandlers(GetField<InspectorEvent>(listener, "onBecameVisible"), capture);

            listener.Invoke(InspectorVisibilityCallbacks.OnBecameVisible);

            Assert.That(capture.Calls, Is.EqualTo(new[] { "OnEnable" }));
        }

        [Test]
        public void ObjectMouse_InvokesEnabledCallbackEvent() {
            var capture = new CaptureHandler();
            var listener = CreateComponent<InspectorObjectMouseEvents>();
            SetField(listener, "callbacks", InspectorObjectMouseCallbacks.OnMouseDown);
            SetInspectorEventHandlers(GetField<InspectorEvent>(listener, "onMouseDown"), capture);

            listener.Invoke(InspectorObjectMouseCallbacks.OnMouseDown);

            Assert.That(capture.Calls, Is.EqualTo(new[] { "OnEnable" }));
        }

        T CreateComponent<T>() where T : Component {
            var gameObject = new GameObject(nameof(InspectorSemanticEventComponentTests));
            _createdObjects.Add(gameObject);
            return gameObject.AddComponent<T>();
        }

        static void AssertValidFlags<TEnum>(TEnum none, TEnum all) where TEnum : struct, Enum {
            Assert.That(Convert.ToInt32(none), Is.EqualTo(0));

            var values = Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Where(value => !EqualityComparer<TEnum>.Default.Equals(value, none) && !EqualityComparer<TEnum>.Default.Equals(value, all))
                .ToArray();

            foreach (var callback in values) {
                var intValue = Convert.ToInt32(callback);
                Assert.That(intValue > 0 && (intValue & (intValue - 1)) == 0, Is.True, $"{callback} must be a single bit.");
            }

            var allFromSingles = values.Aggregate(0, (current, value) => current | Convert.ToInt32(value));
            Assert.That(Convert.ToInt32(all), Is.EqualTo(allFromSingles));
        }

        static void SetInspectorEventHandlers(InspectorEvent inspectorEvent, params IInspectorEventHandler[] handlers) {
            SetField(inspectorEvent, "handlers", handlers);
        }

        static void SetInspectorEventHandlers<TEvent>(InspectorEvent<TEvent> inspectorEvent, params IInspectorEventHandler<TEvent>[] handlers) {
            SetField(inspectorEvent, "handlers", handlers);
        }

        static void SetField(object target, string fieldName, object value) {
            GetFieldInfo(target.GetType(), fieldName).SetValue(target, value);
        }

        static T GetField<T>(object target, string fieldName) {
            return (T)GetFieldInfo(target.GetType(), fieldName).GetValue(target)!;
        }

        static FieldInfo GetFieldInfo(Type type, string fieldName) {
            for (var currentType = type; currentType != null; currentType = currentType.BaseType) {
                var field = currentType.GetField(fieldName, c_instanceFieldFlags);
                if (field != null) {
                    return field;
                }
            }

            throw new InvalidOperationException($"Field '{fieldName}' was not found on '{type.Name}'.");
        }

        sealed class CaptureHandler : IInspectorEventHandler {
            readonly List<string> _calls;
            readonly string _label;

            public IReadOnlyList<string> Calls => _calls;

            public CaptureHandler() : this(new List<string>(), "OnEnable") { }

            public CaptureHandler(List<string> calls, string label) {
                _calls = calls;
                _label = label;
            }

            public void Handle() {
                _calls.Add(_label);
            }
        }

        sealed class CaptureHandler<TEvent> : IInspectorEventHandler<TEvent> {
            public int InvocationCount { get; private set; }
            public TEvent LastValue { get; private set; } = default!;

            public void Handle(in TEvent evt) {
                InvocationCount++;
                LastValue = evt;
            }
        }
    }
}
