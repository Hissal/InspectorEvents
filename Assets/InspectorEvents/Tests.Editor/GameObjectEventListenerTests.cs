using System;
using System.Collections.Generic;
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
        public void InvokeNoPayload_InvokesMatchingListenerOnce() {
            var listener = CreateListener(
                CreateNoPayloadEntry(GameObjectEventMessage.OnEnable, new CaptureHandler())
            );

            listener.InvokeNoPayload(GameObjectEventMessage.OnEnable);

            var capture = GetNoPayloadCapture(listener, 0);
            Assert.That(capture.Calls, Is.EqualTo(new[] { "OnEnable" }));
        }

        [Test]
        public void InvokeNoPayload_DoesNotInvokeNonMatchingListener() {
            var listener = CreateListener(
                CreateNoPayloadEntry(GameObjectEventMessage.OnDisable, new CaptureHandler())
            );

            listener.InvokeNoPayload(GameObjectEventMessage.OnEnable);

            var capture = GetNoPayloadCapture(listener, 0);
            Assert.That(capture.Calls, Is.Empty);
        }

        [Test]
        public void InvokeNoPayload_AllowsDuplicateMessages_InSerializedOrder() {
            var calls = new List<string>();
            var listener = CreateListener(
                CreateNoPayloadEntry(GameObjectEventMessage.Start, new CaptureHandler(calls, "First")),
                CreateNoPayloadEntry(GameObjectEventMessage.OnDisable, new CaptureHandler(calls, "Wrong")),
                CreateNoPayloadEntry(GameObjectEventMessage.Start, new CaptureHandler(calls, "Second"))
            );

            listener.InvokeNoPayload(GameObjectEventMessage.Start);

            Assert.That(calls, Is.EqualTo(new[] { "First", "Second" }));
        }

        [Test]
        public void InvokeCollision3D_RoutesToTypedCollisionEvent() {
            var capture = new CaptureHandler<Collision>();
            var listener = CreateListener(
                CreateTypedEntry(GameObjectEventMessage.OnCollisionEnter, "collision3DEvent", capture)
            );

            listener.InvokeCollision3D(GameObjectEventMessage.OnCollisionEnter, null!);

            Assert.That(capture.InvocationCount, Is.EqualTo(1));
            Assert.That(capture.LastValue, Is.Null);
        }

        [Test]
        public void InvokeTrigger3D_RoutesToTypedColliderEvent() {
            var capture = new CaptureHandler<Collider>();
            var listener = CreateListener(
                CreateTypedEntry(GameObjectEventMessage.OnTriggerEnter, "trigger3DEvent", capture)
            );

            listener.InvokeTrigger3D(GameObjectEventMessage.OnTriggerEnter, null!);

            Assert.That(capture.InvocationCount, Is.EqualTo(1));
            Assert.That(capture.LastValue, Is.Null);
        }

        [Test]
        public void InvokeCollision2D_RoutesToTypedCollision2DEvent() {
            var capture = new CaptureHandler<Collision2D>();
            var listener = CreateListener(
                CreateTypedEntry(GameObjectEventMessage.OnCollisionEnter2D, "collision2DEvent", capture)
            );

            listener.InvokeCollision2D(GameObjectEventMessage.OnCollisionEnter2D, null!);

            Assert.That(capture.InvocationCount, Is.EqualTo(1));
            Assert.That(capture.LastValue, Is.Null);
        }

        [Test]
        public void InvokeTrigger2D_RoutesToTypedCollider2DEvent() {
            var capture = new CaptureHandler<Collider2D>();
            var listener = CreateListener(
                CreateTypedEntry(GameObjectEventMessage.OnTriggerEnter2D, "trigger2DEvent", capture)
            );

            listener.InvokeTrigger2D(GameObjectEventMessage.OnTriggerEnter2D, null!);

            Assert.That(capture.InvocationCount, Is.EqualTo(1));
            Assert.That(capture.LastValue, Is.Null);
        }

        GameObjectEventListener CreateListener(params GameObjectEventListenerEntry[] entries) {
            var gameObject = new GameObject(nameof(GameObjectEventListenerTests));
            _createdObjects.Add(gameObject);
            var listener = gameObject.AddComponent<GameObjectEventListener>();
            SetField(listener, "listeners", new List<GameObjectEventListenerEntry>(entries));
            return listener;
        }

        static GameObjectEventListenerEntry CreateNoPayloadEntry(GameObjectEventMessage message, CaptureHandler handler) {
            var entry = new GameObjectEventListenerEntry();
            SetField(entry, "message", message);
            SetInspectorEventHandlers(GetField<InspectorEvent>(entry, "noPayloadEvent"), handler);
            return entry;
        }

        static GameObjectEventListenerEntry CreateTypedEntry<TEvent>(
            GameObjectEventMessage message,
            string eventFieldName,
            CaptureHandler<TEvent> handler
        ) {
            var entry = new GameObjectEventListenerEntry();
            SetField(entry, "message", message);
            SetInspectorEventHandlers(GetField<InspectorEvent<TEvent>>(entry, eventFieldName), handler);
            return entry;
        }

        static CaptureHandler GetNoPayloadCapture(GameObjectEventListener listener, int entryIndex) {
            var entries = GetField<List<GameObjectEventListenerEntry>>(listener, "listeners");
            var inspectorEvent = GetField<InspectorEvent>(entries[entryIndex], "noPayloadEvent");
            return (CaptureHandler)GetField<IInspectorEventHandler[]>(inspectorEvent, "handlers")[0];
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
