using System;
using System.Reflection;
using InspectorEvents.Core;
using NUnit.Framework;
using UnityEngine;

namespace InspectorEvents.Tests.Editor {
    public sealed class EventListenerBehaviorBaseTests {
        const BindingFlags c_instanceFieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        [Test]
        public void Destroy_Unsubscribes_WhenListenerIsStillSubscribed() {
            var gameObject = new GameObject(nameof(Destroy_Unsubscribes_WhenListenerIsStillSubscribed));

            try {
                var listener = gameObject.AddComponent<TestEventListener>();
                SetField(listener, "_isSubscribed", true);
                SetField(listener, "unsubscribeOn", LifecycleMessage.Disable);

                listener.Trigger(LifecycleMessage.Destroy);

                Assert.That(listener.UnsubscribeCallCount, Is.EqualTo(1));
                Assert.That(GetField<bool>(listener, "_isSubscribed"), Is.False);
            }
            finally {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        static void SetField(object target, string fieldName, object value) {
            GetFieldInfo(target.GetType(), fieldName).SetValue(target, value);
        }

        static T GetField<T>(object target, string fieldName) {
            return (T)GetFieldInfo(target.GetType(), fieldName).GetValue(target)!;
        }

        static FieldInfo GetFieldInfo(Type type, string fieldName) {
            return type.GetField(fieldName, c_instanceFieldFlags)
                   ?? throw new InvalidOperationException($"Field '{fieldName}' was not found on '{type.Name}'.");
        }

        sealed class TestEventListener : EventListenerBehaviorBase<object> {
            public int UnsubscribeCallCount { get; private set; }

            protected override void OnSubscribe<TEvent>(Action<TEvent> call) { }

            public override void Unsubscribe() {
                UnsubscribeCallCount++;
            }

            public void Trigger(LifecycleMessage message) {
                HandleLifecycleMessage(message);
            }
        }
    }
}
