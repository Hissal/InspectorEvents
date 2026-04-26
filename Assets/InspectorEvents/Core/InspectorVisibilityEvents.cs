using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InspectorEvents.Core;

[Flags]
public enum InspectorVisibilityCallbacks {
    None = 0,
    OnBecameVisible = 1 << 0,
    OnBecameInvisible = 1 << 1,
    All = OnBecameVisible | OnBecameInvisible
}

public sealed class InspectorVisibilityEvents : MonoBehaviour {
    [SerializeField] InspectorVisibilityCallbacks callbacks = InspectorVisibilityCallbacks.None;

    [SerializeField, ShowIf(nameof(UsesOnBecameVisible)), HideLabel, InlineProperty]
    InspectorEvent onBecameVisible = new();

    [SerializeField, ShowIf(nameof(UsesOnBecameInvisible)), HideLabel, InlineProperty]
    InspectorEvent onBecameInvisible = new();

    bool UsesOnBecameVisible => Has(InspectorVisibilityCallbacks.OnBecameVisible);
    bool UsesOnBecameInvisible => Has(InspectorVisibilityCallbacks.OnBecameInvisible);

    bool Has(InspectorVisibilityCallbacks callback) => (callbacks & callback) != 0;

    void OnBecameVisible() => Invoke(InspectorVisibilityCallbacks.OnBecameVisible);

    void OnBecameInvisible() => Invoke(InspectorVisibilityCallbacks.OnBecameInvisible);

    internal void Invoke(InspectorVisibilityCallbacks callback) {
        if (!Has(callback)) {
            return;
        }

        switch (callback) {
            case InspectorVisibilityCallbacks.OnBecameVisible:
                onBecameVisible.Invoke();
                break;
            case InspectorVisibilityCallbacks.OnBecameInvisible:
                onBecameInvisible.Invoke();
                break;
        }
    }
}
