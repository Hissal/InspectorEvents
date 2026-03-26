using System;
using System.Collections.Generic;
using System.Reflection;
using Hissal.UnityTypeSerializer;
using InspectorEvents.Internal;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InspectorEvents.Core {
    public abstract class EventListenerBehaviorBase<TEventBase> : EventListenerLifecycleHandler where TEventBase : class {
        [Title("Subscription Lifecycle")]
        [SerializeField] LifecycleMessage subscribeOn = LifecycleMessage.Enable;
        [SerializeField] LifecycleMessage unsubscribeOn = LifecycleMessage.Disable;

        [Title("Event Configuration")]
        [SerializeField]
#if UNITY_EDITOR
        [SerializedTypeOptions(
            OnTypeChanged = nameof(OnEventTypeChanged)
        )]
#endif
        SerializedType<TEventBase> eventType = null!;

        [PropertySpace]
        [SerializeReference]
        [HideReferenceObjectPicker, InlineProperty, HideLabel]
        [ShowIf("@eventType.IsValid")]
        ISerializedEventFilter? filter;

        [SerializeReference]
        [HideReferenceObjectPicker, InlineProperty, HideLabel]
        [ShowIf("@eventType.IsValid")]
        ISerializedEventListener? listener;

#if UNITY_EDITOR

#endif

        Action<EventListenerBehaviorBase<TEventBase>>? _subscriber;
        Type? _subscriberType;

        Delegate? _eventCall;
        Type? _eventCallType;
        ISerializedEventListener? _eventCallListener;
        ISerializedEventFilter? _eventCallFilter;

        bool _isSubscribed;

        protected abstract void OnSubscribe<TEvent>(Action<TEvent> call) where TEvent : TEventBase;

#if UNITY_EDITOR
        bool EnableSubscribeButton => eventType.IsValid && !_isSubscribed && Application.isPlaying;
    
        [TitleGroup("Controls")]
        [ButtonGroup("Controls/Sub", -1)]
        [ShowIf("@eventType.IsValid")]
        [EnableIf(nameof(EnableSubscribeButton))]
#endif
        public void Subscribe() {
            if (listener == null) {
                return;
            }

            var type = eventType.Type;
            if (type == null) {
                return;
            }

            if (_subscriber == null || _subscriberType != type) {
                _subscriber = SubscribeInvokerCache.Get(type);
                _subscriberType = type;
            }

            _subscriber(this);
        }

#if UNITY_EDITOR
        bool EnableUnsubscribeButton => eventType.IsValid && _isSubscribed && Application.isPlaying;
    
        [ButtonGroup("Controls/Sub", -1)]
        [ShowIf("@eventType.IsValid")]
        [EnableIf(nameof(EnableUnsubscribeButton))]
#endif
        public abstract void Unsubscribe();

        void SubscribeTyped<TEvent>() where TEvent : TEventBase {
            var call = GetOrCreateEventCall<TEvent>();

            try {
                OnSubscribe(call);
                _isSubscribed = true;
            }
            catch (Exception e) {
                Debug.LogError($"Unhandled exception thrown while subscribing to event '{typeof(TEvent).FullName}'. {e}", this);
            }
        }

        Action<TEvent> GetOrCreateEventCall<TEvent>() where TEvent : TEventBase {
            if (_eventCallType != typeof(TEvent)
                || !ReferenceEquals(_eventCallListener, listener)
                || !ReferenceEquals(_eventCallFilter, filter)
                || _eventCall is not Action<TEvent>)
            {
                _eventCall = DynamicEventInvoker.CreateCaller<TEvent>(listener, filter);
                _eventCallType = typeof(TEvent);
                _eventCallListener = listener;
                _eventCallFilter = filter;
            }

            return (Action<TEvent>)_eventCall;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Invoked when the 
        /// </summary>
        protected virtual void OnInvokeTestEvent<TEvent>(in TEvent message) where TEvent : TEventBase {
            // Default: run through local listener/filter for editor validation.
            GetOrCreateEventCall<TEvent>().Invoke(message);
        }

        void InvokeTestTyped<TEvent>(object value) where TEvent : TEventBase {
            OnInvokeTestEvent((TEvent)value);
        }

        [FoldoutGroup("Controls/Test Invoke", Expanded = false)]
        [Button("Invoke")]
        [ShowIf("@eventType.IsValid")]
        public void InvokeTestEvent() {
            var type = eventType.Type;
            if (type == null) {
                Debug.LogWarning("Cannot invoke test event: no event type selected.", this);
                return;
            }

            if (testValueConstructor == null) {
                Debug.LogWarning("Cannot invoke test event: missing value constructor. Change event type to regenerate it.", this);
                return;
            }

            if (testValueConstructor.ValueType != type) {
                testValueConstructor = IValueConstructor.Create(type);
            }

            var value = testValueConstructor.ConstructValueBoxed();
            if (value == null) {
                Debug.LogWarning($"Cannot invoke test event for '{type.Name}': {testValueConstructor.LastError ?? "no value produced"}.", this);
                return;
            }

            if (_testEventInvoker == null || _testEventInvokerType != type) {
                _testEventInvoker = TestEventInvokerCache.Get(type);
                _testEventInvokerType = type;
            }

            try {
                _testEventInvoker(this, value);
            }
            catch (Exception e) {
                Debug.LogError($"Unhandled exception while invoking test event '{type.FullName}'. {e}", this);
            }
        }
    
        [SerializeReference]
        [FoldoutGroup("Controls/Test Invoke/Value", 1)]
        [HideReferenceObjectPicker, InlineProperty, HideLabel]
        [ShowIf("@eventType.IsValid")]
        IValueConstructor? testValueConstructor;
    
        Action<EventListenerBehaviorBase<TEventBase>, object>? _testEventInvoker;
        Type? _testEventInvokerType;
#endif

        protected override void HandleLifecycleMessage(LifecycleMessage message) {
            switch (_isSubscribed) {
                case false when message == subscribeOn:
                    Subscribe();
                    break;
                case true when message == unsubscribeOn:
                    try {
                        Unsubscribe();
                        _isSubscribed = false;
                    }
                    catch (Exception e) {
                        Debug.LogError($"Unhandled exception thrown while unsubscribing from event. {e}", this);
                    }

                    break;
            }
        }

#if UNITY_EDITOR
        internal void OnEventTypeChanged() {
            _subscriber = null;
            _subscriberType = null;
            _eventCall = null;
            _eventCallType = null;
            _eventCallListener = null;
            _eventCallFilter = null;
            _testEventInvoker = null;
            _testEventInvokerType = null;

            var type = eventType.Type;
            if (type == null) {
                listener = null;
                filter = null;
                testValueConstructor = null;
                return;
            }

            if (!typeof(TEventBase).IsAssignableFrom(type)) {
                Debug.LogError($"Selected event type '{type.FullName}' is not assignable to '{typeof(TEventBase).FullName}'.");
                listener = null;
                filter = null;
                testValueConstructor = null;
                return;
            }

            try {
                listener = SerializedEventFactory.CreateDefaultListenerPackage(type);
                filter = SerializedEventFactory.CreateDefaultFilterPackage(type);
            
                try {
                    testValueConstructor = IValueConstructor.Create(type);
                }
                catch (Exception e) {
                    Debug.LogWarning($"Failed to create value constructor for event type '{type.FullName}'. Test event invocation will not work. {e}");
                    testValueConstructor = null;
                }
            }
            catch (Exception ex) {
                Debug.LogError($"Failed to create default listener/filter for event type '{type.FullName}'. {ex}");
                listener = null;
                filter = null;
                testValueConstructor = null;
            }
        }
#endif

        static class SubscribeInvokerCache {
            // ReSharper disable StaticMemberInGenericType
            static readonly object syncRoot = new();
            static readonly Dictionary<Type, Action<EventListenerBehaviorBase<TEventBase>>> cache = new();
            static readonly MethodInfo subscribeTypedMethod = typeof(EventListenerBehaviorBase<TEventBase>)
                .GetMethod(nameof(SubscribeTyped), BindingFlags.Instance | BindingFlags.NonPublic)!;
            // ReSharper restore StaticMemberInGenericType

            public static Action<EventListenerBehaviorBase<TEventBase>> Get(Type eventType) {
                lock (syncRoot) {
                    if (cache.TryGetValue(eventType, out var invoker)) {
                        return invoker;
                    }

                    var typedMethod = subscribeTypedMethod.MakeGenericMethod(eventType);
                    invoker = (Action<EventListenerBehaviorBase<TEventBase>>)Delegate.CreateDelegate(
                        typeof(Action<EventListenerBehaviorBase<TEventBase>>),
                        null,
                        typedMethod
                    );
                    cache.Add(eventType, invoker);
                    return invoker;
                }
            }
        }

#if UNITY_EDITOR
        static class TestEventInvokerCache {
            static readonly object syncRoot = new();
            static readonly Dictionary<Type, Action<EventListenerBehaviorBase<TEventBase>, object>> cache = new();
            static readonly MethodInfo invokeTypedMethod = typeof(EventListenerBehaviorBase<TEventBase>)
                .GetMethod(nameof(InvokeTestTyped), BindingFlags.Instance | BindingFlags.NonPublic)!;

            public static Action<EventListenerBehaviorBase<TEventBase>, object> Get(Type eventType) {
                lock (syncRoot) {
                    if (cache.TryGetValue(eventType, out var invoker)) {
                        return invoker;
                    }

                    var typedMethod = invokeTypedMethod.MakeGenericMethod(eventType);
                    invoker = (Action<EventListenerBehaviorBase<TEventBase>, object>)Delegate.CreateDelegate(
                        typeof(Action<EventListenerBehaviorBase<TEventBase>, object>),
                        null,
                        typedMethod
                    );
                    cache.Add(eventType, invoker);
                    return invoker;
                }
            }
        }
#endif
    }
}
