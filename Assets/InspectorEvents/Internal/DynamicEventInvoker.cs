using System;

namespace InspectorEvents.Internal;

internal static class DynamicEventInvoker {
    public static Action<TEvent> CreateCaller<TEvent>(IEventHandlerSentinel? sentinel) {
        return sentinel is not EventHandlerSentinel<TEvent> typedSentinel
            ? EmptyActionCache<TEvent>.Value
            : e => typedSentinel.Handle(e);
    }

    static class EmptyActionCache<TEvent> {
        public static readonly Action<TEvent> Value = static _ => { };
    }
}