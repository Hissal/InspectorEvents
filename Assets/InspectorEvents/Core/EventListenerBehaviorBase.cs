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
        IEventHandlerSentinel? handlerSentinel;
        
        Action<EventListenerBehaviorBase<TEventBase>>? _subscriber;
        Type? _subscriberType;

        Delegate? _eventCall;
        Type? _eventCallType;
        IEventHandlerSentinel? _eventCallSentinel;

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
            if (handlerSentinel == null) {
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
                || !ReferenceEquals(_eventCallSentinel, handlerSentinel)
                || _eventCall is not Action<TEvent>)
            {
                _eventCall = handlerSentinel?.CreateCaller<TEvent>() ?? (static _ => { });
                _eventCallType = typeof(TEvent);
                _eventCallSentinel = handlerSentinel;
            }

            return (Action<TEvent>)_eventCall;
        }

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
            _eventCallSentinel = null;

            var type = eventType.Type;
            if (type == null) {
                handlerSentinel = null;
                return;
            }

            if (!typeof(TEventBase).IsAssignableFrom(type)) {
                Debug.LogError($"Selected event type '{type.FullName}' is not assignable to '{typeof(TEventBase).FullName}'.");
                handlerSentinel = null;
                return;
            }

            try {
                handlerSentinel = IEventHandlerSentinel.Create(type);
            }
            catch (Exception ex) {
                Debug.LogError($"Failed to create event handler sentinel for event type '{type.FullName}'. {ex}");
                handlerSentinel = null;
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
    }
}
