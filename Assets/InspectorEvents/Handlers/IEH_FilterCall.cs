using System;
using InspectorEvents.Core;
using UnityEngine;

namespace InspectorEvents.Handlers;

[Serializable]
public sealed class IEH_FilterCall : IInspectorEventHandler {
    [SerializeReference] 
    IInspectorEventFilter[] filters = new IInspectorEventFilter[0];
    [SerializeReference] 
    IInspectorEventHandler[] handlers = new IInspectorEventHandler[0];
    
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
    IInspectorEventFilter<TEvent>[] filters = new IInspectorEventFilter<TEvent>[0];
    [SerializeReference] 
    IInspectorEventHandler<TEvent>[] handlers = new IInspectorEventHandler<TEvent>[0];
    
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
