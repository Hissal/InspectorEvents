using System;
using InspectorEvents.Core;
using InspectorEvents.Listeners;
using UnityEngine;

namespace InspectorEvents.Internal;

internal static class SerializedEventFactory {
    public static ISerializedEventListener CreateDefaultListenerPackage(Type eventType) {
        var defaultListener = CreateDefaultListener(eventType);
        var wrappedListener = CreateWrappedListener(eventType, defaultListener);
        return wrappedListener;
    }

    public static ISerializedEventFilter CreateDefaultFilterPackage(Type eventType) {
        var wrapperType = typeof(EventFilter<>).MakeGenericType(eventType);
        return (ISerializedEventFilter)Activator.CreateInstance(wrapperType, (ISerializedEventFilter?)null)!;
    }

    static ISerializedEventListener CreateDefaultListener(Type eventType) {
        var listenerType = typeof(SEL_UnityEvent<>).MakeGenericType(eventType);
        return (ISerializedEventListener)Activator.CreateInstance(listenerType)!;
    }

    static ISerializedEventListener CreateWrappedListener(Type eventType, ISerializedEventListener? listener) {
        var wrapperType = typeof(EventListener<>).MakeGenericType(eventType);
        return (ISerializedEventListener)Activator.CreateInstance(wrapperType, listener)!;
    }

    [Serializable]
    sealed class EventListener<TEvent> : ISerializedEventListener<TEvent> {
        [SerializeReference] ISerializedEventListener<TEvent>? listener;
        public void OnEvent(in TEvent e) => listener?.OnEvent(e);
        public EventListener(ISerializedEventListener<TEvent>? listener) => this.listener = listener;
    }

    [Serializable]
    sealed class EventFilter<TEvent> : ISerializedEventFilter<TEvent> {
        [SerializeReference] ISerializedEventFilter<TEvent>? filter;
        public bool Filter(in TEvent e) => filter == null || filter.Filter(e);
        public EventFilter(ISerializedEventFilter<TEvent>? filter) => this.filter = filter;
    }
}