using System;
using UnityEngine;

namespace InspectorEvents.Listeners;

[Serializable]
public sealed class SEL_DebugBreak<TEvent> : ISerializedEventListener<TEvent> {
    public void OnEvent(in TEvent e) => Debug.Break();
}