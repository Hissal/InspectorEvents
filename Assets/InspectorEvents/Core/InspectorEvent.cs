using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InspectorEvents.Core;

[Serializable]
public sealed class TestFileter : IInspectorEventFilter {
    public bool Evaluate() => Application.isPlaying;
}

[Serializable]
public sealed class TestHandler : IInspectorEventHandler {
    public void Handle() => Debug.Log("Handled!");
}

[Serializable]
public sealed class InspectorEvent {
    [SerializeReference] 
    IInspectorEventFilter[] filters = Array.Empty<IInspectorEventFilter>();
    
    [PropertySpace]
    [SerializeReference] 
    IInspectorEventHandler[] handlers = Array.Empty<IInspectorEventHandler>();

    public void Invoke() {
        if (!EvaluateFilters()) 
            return;

        foreach (var handler in handlers) {
            handler.Handle();
        }
    }
    
    bool EvaluateFilters() {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var filter in filters) {
            if (!filter.Evaluate()) 
                return false;
        }

        return true;
    }
}

[Serializable]
public sealed class InspectorEvent<TEvent> {
    [SerializeReference] 
    IInspectorEventFilter<TEvent>[] filters = Array.Empty<IInspectorEventFilter<TEvent>>();
    
    [PropertySpace]
    [SerializeReference] 
    IInspectorEventHandler<TEvent>[] handlers = Array.Empty<IInspectorEventHandler<TEvent>>();

    public void Invoke(in TEvent e) {
        if (!EvaluateFilters(e)) 
            return;

        foreach (var handler in handlers) {
            handler.Handle(e);
        }
    }
    
    bool EvaluateFilters(in TEvent e) {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var filter in filters) {
            if (!filter.Evaluate(e)) 
                return false;
        }

        return true;
    }
}