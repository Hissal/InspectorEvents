using System;
using InspectorEvents.Core;
using UnityEngine;

namespace InspectorEvents.Handlers;

[Serializable]
public sealed class IEH_ToNonGenericAdapter<TEvent> : IInspectorEventHandler<TEvent> {
    [SerializeReference] IInspectorEventHandler nonGenericHandler;
    public void Handle(in TEvent e) => nonGenericHandler.Handle();
}