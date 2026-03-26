using System;

namespace InspectorEvents;

internal static class DynamicEventInvoker {
    public static Action<TEvent> CreateCaller<TEvent>(ISerializedEventListener? listener, ISerializedEventFilter? filter) {
        var typedFilter = filter as ISerializedEventFilter<TEvent>;
         
        return listener is not ISerializedEventListener<TEvent> typedListener 
            ? EmptyActionCache<TEvent>.Value 
            : e => InvokeListener(typedListener, typedFilter, e);
    }

    static void InvokeListener<TEvent>(ISerializedEventListener<TEvent> listener, ISerializedEventFilter<TEvent>? filter, in TEvent e) {
        if (filter != null && !filter.Filter(e)) {
            return;
        }

        listener.OnEvent(e);
    }

    static class EmptyActionCache<TEvent> {
        public static readonly Action<TEvent> Value = static _ => { };
    }
}