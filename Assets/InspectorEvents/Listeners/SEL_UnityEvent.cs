using System;
using UnityEngine;
using UnityEngine.Events;

namespace InspectorEvents.Listeners;

[Serializable]
public sealed class SEL_UnityEvent<TMessage> : ISerializedEventListener<TMessage> {
    [SerializeField] UnityEvent<TMessage> onEvent = new();
    public void OnEvent(in TMessage e) => onEvent.Invoke(e);
}