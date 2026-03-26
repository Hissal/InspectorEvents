using System;
using InspectorEvents.Core;
using UnityEngine;

namespace InspectorEvents.Internal;

internal interface IEventHandlerSentinel {
    public static IEventHandlerSentinel Create(Type eventType) {
        var sentinelType = typeof(EventHandlerSentinel<>).MakeGenericType(eventType);
        return (IEventHandlerSentinel)Activator.CreateInstance(sentinelType)!;
    }
}

[Serializable]
internal class EventHandlerSentinel<TEvent> : IEventHandlerSentinel {
    [SerializeField] 
    InspectorEvent<TEvent> @event = new();
    
    public void Handle(in TEvent e) {
        @event.Invoke(e);    
    }
}
