using System;
using InspectorEvents.Core;
using UnityEngine;

namespace InspectorEvents.Internal;

internal interface IEventHandlerSentinel {
    Action<TEvent> CreateCaller<TEvent>() => EmptyActionCache<TEvent>.Value;

    public static IEventHandlerSentinel Create(Type eventType) {
        var sentinelType = typeof(EventHandlerSentinel<>).MakeGenericType(eventType);
        return (IEventHandlerSentinel)Activator.CreateInstance(sentinelType)!;
    }

    static class EmptyActionCache<TEvent> {
        public static readonly Action<TEvent> Value = static _ => { };
    }
}

[Serializable]
internal class EventHandlerSentinel<TEvent> : IEventHandlerSentinel {
    [SerializeField] 
    InspectorEvent<TEvent> @event = new();

    public Action<TRequestedEvent> CreateCaller<TRequestedEvent>() {
        return this is not EventHandlerSentinel<TRequestedEvent> typedSentinel
            ? IEventHandlerSentinel.EmptyActionCache<TRequestedEvent>.Value
            : e => typedSentinel.Handle(e);
    }
    
    public void Handle(in TEvent e) {
        @event.Invoke(e);    
    }
}
