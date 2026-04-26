using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using InspectorEvents.Core;
using InspectorEvents.Physics2D;
using NUnit.Framework;
using UnityEngine;

namespace InspectorEvents.Tests.Editor.Physics2D {
    public sealed class InspectorPhysics2DEventsTests {
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
            AssertValidFlags(InspectorPhysics2DCallbacks.None, InspectorPhysics2DCallbacks.All);
        }

        [Test]
        public void Collision_RoutesEnabledPayload() {
            var capture = new CaptureHandler<Collision2D>();
            var listener = CreateListener(InspectorPhysics2DCallbacks.OnCollisionEnter2D);
            SetInspectorEventHandlers(GetField<InspectorEvent<Collision2D>>(listener, "onCollisionEnter2D"), capture);

            listener.InvokeCollision(InspectorPhysics2DCallbacks.OnCollisionEnter2D, null!);

            Assert.That(capture.InvocationCount, Is.EqualTo(1));
            Assert.That(capture.LastValue, Is.Null);
        }

        [Test]
        public void Trigger_DoesNotInvokeDisabledPayloadEvent() {
            var capture = new CaptureHandler<Collider2D>();
            var listener = CreateListener(InspectorPhysics2DCallbacks.None);
            SetInspectorEventHandlers(GetField<InspectorEvent<Collider2D>>(listener, "onTriggerEnter2D"), capture);

            listener.InvokeTrigger(InspectorPhysics2DCallbacks.OnTriggerEnter2D, null!);

            Assert.That(capture.InvocationCount, Is.EqualTo(0));
        }

        [Test]
        public void JointBreak_RoutesJointPayload() {
            var capture = new CaptureHandler<Joint2D>();
            var listener = CreateListener(InspectorPhysics2DCallbacks.OnJointBreak2D);
            SetInspectorEventHandlers(GetField<InspectorEvent<Joint2D>>(listener, "onJointBreak2D"), capture);

            listener.InvokeJointBreak(null!);

            Assert.That(capture.InvocationCount, Is.EqualTo(1));
            Assert.That(capture.LastValue, Is.Null);
        }

        InspectorPhysics2DEvents CreateListener(InspectorPhysics2DCallbacks callbacks) {
            _gameObject = new GameObject(nameof(InspectorPhysics2DEventsTests));
            var listener = _gameObject.AddComponent<InspectorPhysics2DEvents>();
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
