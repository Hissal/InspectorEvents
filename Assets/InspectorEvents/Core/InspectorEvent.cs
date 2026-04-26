using System;
using InspectorEvents.Internal;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InspectorEvents.Core;

[Serializable]
public sealed class InspectorEvent {
    [SerializeReference] 
    IInspectorEventFilter[] filters = new IInspectorEventFilter[0];
    
    [PropertySpace]
    [SerializeReference] 
    IInspectorEventHandler[] handlers = new IInspectorEventHandler[0];

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
    // ReSharper disable UseArrayEmptyMethod
    [SerializeReference] 
    IInspectorEventFilter<TEvent>[] filters = new IInspectorEventFilter<TEvent>[0];
    
    [PropertySpace]
    [SerializeReference] 
    IInspectorEventHandler<TEvent>[] handlers = new IInspectorEventHandler<TEvent>[0];
    // ReSharper restore UseArrayEmptyMethod

#if UNITY_EDITOR
    [SerializeReference, HideInInspector]
    IValueConstructor? valueConstructor;

    IValueConstructor EnsureValueConstructor() {
        if (valueConstructor == null || valueConstructor.ValueType != typeof(TEvent)) {
            valueConstructor = IValueConstructor.Create(typeof(TEvent));
        }

        return valueConstructor;
    }

    public object GetOrCreateValueConstructorObject() {
        return EnsureValueConstructor();
    }

    public bool TryInvokeConfigured(out string error) {
        var constructor = EnsureValueConstructor();
        var boxed = constructor.ConstructValueBoxed();

        if (boxed is TEvent typedValue) {
            Invoke(typedValue);
            error = string.Empty;
            return true;
        }

        if (boxed == null && !typeof(TEvent).IsValueType) {
            Invoke(default!);
            error = string.Empty;
            return true;
        }

        error = constructor.LastError ?? $"Configured value type mismatch. Expected '{typeof(TEvent).FullName}', got '{boxed.GetType().FullName}'.";
        return false;
    }
#endif

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
