using System;
using InspectorEvents.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InspectorEvents.Handlers;

[Serializable]
public sealed class IEH_DelayCall : IInspectorEventHandler {
    [SerializeField] bool randomize;
    [SerializeField, HideIf(nameof(randomize))] float delaySeconds;
    [SerializeField, ShowIf(nameof(randomize))] Vector2 delaySecondsRange;
    [PropertySpace]
    [SerializeReference] 
    IInspectorEventHandler[] handlers = Array.Empty<IInspectorEventHandler>();
    
    public void Handle() {
        var delay = randomize ? UnityEngine.Random.Range(delaySecondsRange.x, delaySecondsRange.y) : delaySeconds;
        HandleDelayed(delay);
    }

    async void HandleDelayed(float delay) {
        try {
            await Awaitable.WaitForSecondsAsync(delay);
            foreach (var handler in handlers) {
                handler.Handle();
            }
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }
}

[Serializable]
public sealed class IEH_DelayCall<TEvent> : IInspectorEventHandler<TEvent> {
    [SerializeField] bool randomize;
    [SerializeField, HideIf(nameof(randomize))] float delaySeconds;
    [SerializeField, ShowIf(nameof(randomize))] Vector2 delaySecondsRange;
    [PropertySpace]
    [SerializeReference] 
    IInspectorEventHandler<TEvent>[] handlers = Array.Empty<IInspectorEventHandler<TEvent>>();
    
    public void Handle(in TEvent evt) {
        var delay = randomize ? UnityEngine.Random.Range(delaySecondsRange.x, delaySecondsRange.y) : delaySeconds;
        HandleDelayed(delay, evt);
    }

    async void HandleDelayed(float delay, TEvent evt) {
        try {
            await Awaitable.WaitForSecondsAsync(delay);
            foreach (var handler in handlers) {
                handler.Handle(evt);
            }
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }
}