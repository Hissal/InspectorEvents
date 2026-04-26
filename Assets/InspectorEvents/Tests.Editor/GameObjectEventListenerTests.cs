using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using InspectorEvents.Core;
using NUnit.Framework;
using UnityEngine;

namespace InspectorEvents.Tests.Editor {
    public sealed class GameObjectEventListenerTests {
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
        public void EventCallbacks_UseValidFlags() {
            var values = Enum.GetValues(typeof(GameObjectEventCallbacks))
                .Cast<GameObjectEventCallbacks>()
                .ToArray();

            Assert.That(GameObjectEventCallbacks.None, Is.EqualTo((GameObjectEventCallbacks)0));

            var singleBitValues = values
                .Where(value => value is not GameObjectEventCallbacks.None and not GameObjectEventCallbacks.All)
                .ToArray();

            foreach (var callback in singleBitValues) {
                var intValue = (int)callback;
                Assert.That(intValue > 0 && (intValue & (intValue - 1)) == 0, Is.True, $"{callback} must be a single bit.");
            }

            var allFromSingles = singleBitValues.Aggregate(GameObjectEventCallbacks.None, (current, value) => current | value);
            Assert.That(GameObjectEventCallbacks.All, Is.EqualTo(allFromSingles));
        }

        [Test]
        public void InvokeNoPayload_InvokesEnabledCallbackEvent() {
            var capture = new CaptureHandler();
            var listener = CreateListener(GameObjectEventCallbacks.OnEnable);
            SetInspectorEventHandlers(GetField<InspectorEvent>(listener, "onEnable"), capture);

            listener.InvokeNoPayload(GameObjectEventCallbacks.OnEnable);

            Assert.That(capture.Calls, Is.EqualTo(new[] { "OnEnable" }));
        }

        [Test]
        public void InvokeNoPayload_DoesNotInvokeDisabledCallbackEvent() {
            var capture = new CaptureHandler();
            var listener = CreateListener(GameObjectEventCallbacks.None);
            SetInspectorEventHandlers(GetField<InspectorEvent>(listener, "onEnable"), capture);

            listener.InvokeNoPayload(GameObjectEventCallbacks.OnEnable);

            Assert.That(capture.Calls, Is.Empty);
        }

        [Test]
        public void InvokeNoPayload_RunsMultipleHandlersInOneInspectorEvent_InOrder() {
            var calls = new List<string>();
            var listener = CreateListener(GameObjectEventCallbacks.OnEnable);
            SetInspectorEventHandlers(
                GetField<InspectorEvent>(listener, "onEnable"),
                new CaptureHandler(calls, "First"),
                new CaptureHandler(calls, "Second")
            );

            listener.InvokeNoPayload(GameObjectEventCallbacks.OnEnable);

            Assert.That(calls, Is.EqualTo(new[] { "First", "Second" }));
        }

        [Test]
        public void InvokeCollision3D_RoutesToEnabledTypedCollisionEvent() {
            var capture = new CaptureHandler<Collision>();
            var listener = CreateListener(GameObjectEventCallbacks.OnCollisionEnter);
            SetInspectorEventHandlers(GetField<InspectorEvent<Collision>>(listener, "onCollisionEnter"), capture);

            listener.InvokeCollision3D(GameObjectEventCallbacks.OnCollisionEnter, null!);

            Assert.That(capture.InvocationCount, Is.EqualTo(1));
            Assert.That(capture.LastValue, Is.Null);
        }

        [Test]
        public void InvokeTrigger3D_RoutesToEnabledTypedColliderEvent() {
            var capture = new CaptureHandler<Collider>();
            var listener = CreateListener(GameObjectEventCallbacks.OnTriggerEnter);
            SetInspectorEventHandlers(GetField<InspectorEvent<Collider>>(listener, "onTriggerEnter"), capture);

            listener.InvokeTrigger3D(GameObjectEventCallbacks.OnTriggerEnter, null!);

            Assert.That(capture.InvocationCount, Is.EqualTo(1));
            Assert.That(capture.LastValue, Is.Null);
        }

        [Test]
        public void InvokeCollision2D_RoutesToEnabledTypedCollision2DEvent() {
            var capture = new CaptureHandler<Collision2D>();
            var listener = CreateListener(GameObjectEventCallbacks.OnCollisionEnter2D);
            SetInspectorEventHandlers(GetField<InspectorEvent<Collision2D>>(listener, "onCollisionEnter2D"), capture);

            listener.InvokeCollision2D(GameObjectEventCallbacks.OnCollisionEnter2D, null!);

            Assert.That(capture.InvocationCount, Is.EqualTo(1));
            Assert.That(capture.LastValue, Is.Null);
        }

        [Test]
        public void InvokeTrigger2D_RoutesToEnabledTypedCollider2DEvent() {
            var capture = new CaptureHandler<Collider2D>();
            var listener = CreateListener(GameObjectEventCallbacks.OnTriggerEnter2D);
            SetInspectorEventHandlers(GetField<InspectorEvent<Collider2D>>(listener, "onTriggerEnter2D"), capture);

            listener.InvokeTrigger2D(GameObjectEventCallbacks.OnTriggerEnter2D, null!);

            Assert.That(capture.InvocationCount, Is.EqualTo(1));
            Assert.That(capture.LastValue, Is.Null);
        }

        GameObjectEventListener CreateListener(GameObjectEventCallbacks callbacks) {
            var gameObject = new GameObject(nameof(GameObjectEventListenerTests));
            _createdObjects.Add(gameObject);
            var listener = gameObject.AddComponent<GameObjectEventListener>();
            SetField(listener, "callbacks", callbacks);
            return listener;
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
