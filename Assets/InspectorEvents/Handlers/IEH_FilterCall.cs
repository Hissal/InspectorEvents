using System;
using InspectorEvents.Core;
using UnityEngine;

namespace InspectorEvents.Handlers;

[Serializable]
public sealed class IEH_FilterCall : IInspectorEventHandler {
    [SerializeReference] 
    IInspectorEventFilter[] filters = Array.Empty<IInspectorEventFilter>();
    [SerializeReference] 
    IInspectorEventHandler[] handlers = Array.Empty<IInspectorEventHandler>();
    
    public void Handle() {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var filter in filters) {
            if (!filter.Evaluate()) {
                return;
            }
        }

        foreach (var handler in handlers) {
            handler.Handle();
        }
    }
}

[Serializable]
public sealed class IEH_FilterCall<TEvent> : IInspectorEventHandler<TEvent> {
    [SerializeReference] 
    IInspectorEventFilter<TEvent>[] filters = Array.Empty<IInspectorEventFilter<TEvent>>();
    [SerializeReference] 
    IInspectorEventHandler<TEvent>[] handlers = Array.Empty<IInspectorEventHandler<TEvent>>();
    
    public void Handle(in TEvent evt) {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var filter in filters) {
            if (!filter.Evaluate(evt)) {
                return;
            }
        }

        foreach (var handler in handlers) {
            handler.Handle(evt);
        }
    }
}