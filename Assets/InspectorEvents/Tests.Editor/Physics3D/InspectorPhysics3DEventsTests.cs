using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using InspectorEvents.Core;
using InspectorEvents.Physics3D;
using NUnit.Framework;
using UnityEngine;

namespace InspectorEvents.Tests.Editor.Physics3D {
    public sealed class InspectorPhysics3DEventsTests {
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
            AssertValidFlags(InspectorPhysics3DCallbacks.None, InspectorPhysics3DCallbacks.All);
        }

        [Test]
        public void Collision_RoutesEnabledPayload() {
            var capture = new CaptureHandler<Collision>();
            var listener = CreateListener(InspectorPhysics3DCallbacks.OnCollisionEnter);
            SetInspectorEventHandlers(GetField<InspectorEvent<Collision>>(listener, "onCollisionEnter"), capture);

            listener.InvokeCollision(InspectorPhysics3DCallbacks.OnCollisionEnter, null!);

            Assert.That(capture.InvocationCount, Is.EqualTo(1));
            Assert.That(capture.LastValue, Is.Null);
        }

        [Test]
        public void Trigger_DoesNotInvokeDisabledPayloadEvent() {
            var capture = new CaptureHandler<Collider>();
            var listener = CreateListener(InspectorPhysics3DCallbacks.None);
            SetInspectorEventHandlers(GetField<InspectorEvent<Collider>>(listener, "onTriggerEnter"), capture);

            listener.InvokeTrigger(InspectorPhysics3DCallbacks.OnTriggerEnter, null!);

            Assert.That(capture.InvocationCount, Is.EqualTo(0));
        }

        [Test]
        public void JointBreak_RoutesFloatPayload() {
            var capture = new CaptureHandler<float>();
            var listener = CreateListener(InspectorPhysics3DCallbacks.OnJointBreak);
            SetInspectorEventHandlers(GetField<InspectorEvent<float>>(listener, "onJointBreak"), capture);

            listener.InvokeJointBreak(12.5f);

            Assert.That(capture.InvocationCount, Is.EqualTo(1));
            Assert.That(capture.LastValue, Is.EqualTo(12.5f));
        }

        InspectorPhysics3DEvents CreateListener(InspectorPhysics3DCallbacks callbacks) {
            _gameObject = new GameObject(nameof(InspectorPhysics3DEventsTests));
            var listener = _gameObject.AddComponent<InspectorPhysics3DEvents>();
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
