using System;
using InspectorEvents.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InspectorEvents.Filters;

[Serializable]
public sealed class IEF_ToNonGenericAdapter<TEvent> : IInspectorEventFilter<TEvent> {
    [SerializeReference, HideLabel, InlineProperty] IInspectorEventFilter filter;
    public bool Evaluate(in TEvent e) => filter.Evaluate();
}

// TODO: maybe add a non-generic to generic adapter as well, but it seems less useful since most filters will likely need the event data to be useful
// [Serializable]
// public class IEF_ToGenericAdapter : IInspectorEventFilter {
//     [SerializeReference, HideLabel, InlineProperty] IInspectorEventFilter filter;
//     public bool Evaluate() => filter.Evaluate();
// }