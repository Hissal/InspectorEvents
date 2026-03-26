using System;
using InspectorEvents.Core;
using UnityEngine;
using UnityEngine.Events;

namespace InspectorEvents.Handlers;

[Serializable]
public sealed class IEH_UnityEvent<TMessage> : IInspectorEventHandler<TMessage> {
    [SerializeField] UnityEvent<TMessage> onEvent = new();
    public void Handle(in TMessage e) => onEvent.Invoke(e);
}