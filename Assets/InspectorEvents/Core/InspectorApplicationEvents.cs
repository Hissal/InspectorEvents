using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InspectorEvents.Core;

[Flags]
public enum InspectorApplicationCallbacks {
    None = 0,
    OnApplicationFocus = 1 << 0,
    OnApplicationPause = 1 << 1,
    OnApplicationQuit = 1 << 2,
    All = OnApplicationFocus | OnApplicationPause | OnApplicationQuit
}

public sealed class InspectorApplicationEvents : MonoBehaviour {
    [SerializeField] InspectorApplicationCallbacks callbacks = InspectorApplicationCallbacks.None;

    [SerializeField, ShowIf(nameof(UsesOnApplicationFocus)), HideLabel, InlineProperty]
    InspectorEvent<bool> onApplicationFocus = new();

    [SerializeField, ShowIf(nameof(UsesOnApplicationPause)), HideLabel, InlineProperty]
    InspectorEvent<bool> onApplicationPause = new();

    [SerializeField, ShowIf(nameof(UsesOnApplicationQuit)), HideLabel, InlineProperty]
    InspectorEvent onApplicationQuit = new();

    bool UsesOnApplicationFocus => Has(InspectorApplicationCallbacks.OnApplicationFocus);
    bool UsesOnApplicationPause => Has(InspectorApplicationCallbacks.OnApplicationPause);
    bool UsesOnApplicationQuit => Has(InspectorApplicationCallbacks.OnApplicationQuit);

    bool Has(InspectorApplicationCallbacks callback) => (callbacks & callback) != 0;

    void OnApplicationFocus(bool hasFocus) => InvokeFocus(hasFocus);

    void OnApplicationPause(bool paused) => InvokePause(paused);

    void OnApplicationQuit() => InvokeQuit();

    internal void InvokeFocus(bool hasFocus) {
        if (Has(InspectorApplicationCallbacks.OnApplicationFocus)) {
            onApplicationFocus.Invoke(hasFocus);
        }
    }

    internal void InvokePause(bool paused) {
        if (Has(InspectorApplicationCallbacks.OnApplicationPause)) {
            onApplicationPause.Invoke(paused);
        }
    }

    internal void InvokeQuit() {
        if (Has(InspectorApplicationCallbacks.OnApplicationQuit)) {
            onApplicationQuit.Invoke();
        }
    }
}
