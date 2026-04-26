using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using InspectorEvents.Core;
using InspectorEvents.UGUI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InspectorEvents.Tests.Editor.UGUI {
    public sealed class InspectorPointerEventsTests {
        const BindingFlags c_instanceFieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        GameObject? _gameObject;

        [TearDown]
        public void TearDown() {
            if (_gameObject != null) {
                UnityEngine.Object.DestroyImmediate(_gameObject);
            }
        }

        [Test]
        public void Callbacks_UseValidFlags() {
            AssertValidFlags(InspectorPointerCallbacks.None, InspectorPointerCallbacks.All);
        }

        [Test]
        public void Pointer_RoutesEnabledPayload() {
            var capture = new CaptureHandler<PointerEventData>();
            var listener = CreateListener(InspectorPointerCallbacks.OnPointerClick);
            SetInspectorEventHandlers(GetField<InspectorEvent<PointerEventData>>(listener, "onPointerClick"), capture);

            listener.InvokePointer(InspectorPointerCallbacks.OnPointerClick, null!);

            Assert.That(capture.InvocationCount, Is.EqualTo(1));
            Assert.That(capture.LastValue, Is.Null);
        }

        [Test]
        public void Pointer_DoesNotInvokeDisabledPayloadEvent() {
            var capture = new CaptureHandler<PointerEventData>();
            var listener = CreateListener(InspectorPointerCallbacks.None);
            SetInspectorEventHandlers(GetField<InspectorEvent<PointerEventData>>(listener, "onPointerDown"), capture);

            listener.InvokePointer(InspectorPointerCallbacks.OnPointerDown, null!);

            Assert.That(capture.InvocationCount, Is.EqualTo(0));
        }

        [Test]
        public void Base_RoutesEnabledPayload() {
            var capture = new CaptureHandler<BaseEventData>();
            var listener = CreateListener(InspectorPointerCallbacks.OnSubmit);
            SetInspectorEventHandlers(GetField<InspectorEvent<BaseEventData>>(listener, "onSubmit"), capture);

            listener.InvokeBase(InspectorPointerCallbacks.OnSubmit, null!);

            Assert.That(capture.InvocationCount, Is.EqualTo(1));
            Assert.That(capture.LastValue, Is.Null);
        }

        [Test]
        public void Move_RoutesAxisPayload() {
            var capture = new CaptureHandler<AxisEventData>();
            var listener = CreateListener(InspectorPointerCallbacks.OnMove);
            SetInspectorEventHandlers(GetField<InspectorEvent<AxisEventData>>(listener, "onMove"), capture);

            listener.InvokeMove(null!);

            Assert.That(capture.InvocationCount, Is.EqualTo(1));
            Assert.That(capture.LastValue, Is.Null);
        }

        InspectorPointerEvents CreateListener(InspectorPointerCallbacks callbacks) {
            _gameObject = new GameObject(nameof(InspectorPointerEventsTests));
            var listener = _gameObject.AddComponent<InspectorPointerEvents>();
            SetField(listener, "callbacks", callbacks);
            return listener;
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

            Assert.That(Convert.ToInt32(all), Is.EqualTo(values.Aggregate(0, (current, value) => current | Convert.ToInt32(value))));
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
