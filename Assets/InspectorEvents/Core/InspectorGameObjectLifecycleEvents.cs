using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InspectorEvents.Core;

[Flags]
public enum InspectorGameObjectLifecycleCallbacks {
    None = 0,
    Awake = 1 << 0,
    Start = 1 << 1,
    OnEnable = 1 << 2,
    OnDisable = 1 << 3,
    OnDestroy = 1 << 4,
    All = Awake | Start | OnEnable | OnDisable | OnDestroy
}

public sealed class InspectorGameObjectLifecycleEvents : MonoBehaviour {
    [SerializeField] InspectorGameObjectLifecycleCallbacks callbacks = InspectorGameObjectLifecycleCallbacks.None;

    [SerializeField, ShowIf(nameof(UsesAwake)), HideLabel, InlineProperty]
    InspectorEvent onAwake = new();

    [SerializeField, ShowIf(nameof(UsesStart)), HideLabel, InlineProperty]
    InspectorEvent onStart = new();

    [SerializeField, ShowIf(nameof(UsesOnEnable)), HideLabel, InlineProperty]
    InspectorEvent onEnable = new();

    [SerializeField, ShowIf(nameof(UsesOnDisable)), HideLabel, InlineProperty]
    InspectorEvent onDisable = new();

    [SerializeField, ShowIf(nameof(UsesOnDestroy)), HideLabel, InlineProperty]
    InspectorEvent onDestroy = new();

    bool UsesAwake => Has(InspectorGameObjectLifecycleCallbacks.Awake);
    bool UsesStart => Has(InspectorGameObjectLifecycleCallbacks.Start);
    bool UsesOnEnable => Has(InspectorGameObjectLifecycleCallbacks.OnEnable);
    bool UsesOnDisable => Has(InspectorGameObjectLifecycleCallbacks.OnDisable);
    bool UsesOnDestroy => Has(InspectorGameObjectLifecycleCallbacks.OnDestroy);

    bool Has(InspectorGameObjectLifecycleCallbacks callback) => (callbacks & callback) != 0;

    void Awake() => Invoke(InspectorGameObjectLifecycleCallbacks.Awake);

    void Start() => Invoke(InspectorGameObjectLifecycleCallbacks.Start);

    void OnEnable() => Invoke(InspectorGameObjectLifecycleCallbacks.OnEnable);

    void OnDisable() => Invoke(InspectorGameObjectLifecycleCallbacks.OnDisable);

    void OnDestroy() => Invoke(InspectorGameObjectLifecycleCallbacks.OnDestroy);

    internal void Invoke(InspectorGameObjectLifecycleCallbacks callback) {
        if (!Has(callback)) {
            return;
        }

        switch (callback) {
            case InspectorGameObjectLifecycleCallbacks.Awake:
                onAwake.Invoke();
                break;
            case InspectorGameObjectLifecycleCallbacks.Start:
                onStart.Invoke();
                break;
            case InspectorGameObjectLifecycleCallbacks.OnEnable:
                onEnable.Invoke();
                break;
            case InspectorGameObjectLifecycleCallbacks.OnDisable:
                onDisable.Invoke();
                break;
            case InspectorGameObjectLifecycleCallbacks.OnDestroy:
                onDestroy.Invoke();
                break;
        }
    }
}
