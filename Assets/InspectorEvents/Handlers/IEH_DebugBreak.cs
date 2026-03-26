using System;
using InspectorEvents.Core;
using UnityEngine;

namespace InspectorEvents.Listeners;

[Serializable]
public sealed class IEH_DebugBreak : IInspectorEventHandler {
    public void Handle() => Debug.Break();
}

[Serializable]
public sealed class IEH_DebugBreak<TEvent> : IInspectorEventHandler<TEvent> {
    public void Handle(in TEvent e) => Debug.Break();
}