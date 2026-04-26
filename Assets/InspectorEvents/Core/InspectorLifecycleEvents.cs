using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InspectorEvents.Core {
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

    public sealed class InspectorLifecycleEvents : MonoBehaviour {
        [SerializeField] InspectorGameObjectLifecycleCallbacks callbacks = InspectorGameObjectLifecycleCallbacks.None;

        [SerializeField, ShowIf(nameof(UsesAwake))]
        InspectorEvent onAwake = new();

        [SerializeField, ShowIf(nameof(UsesStart))]
        InspectorEvent onStart = new();

        [SerializeField, ShowIf(nameof(UsesOnEnable))]
        InspectorEvent onEnable = new();

        [SerializeField, ShowIf(nameof(UsesOnDisable))]
        InspectorEvent onDisable = new();

        [SerializeField, ShowIf(nameof(UsesOnDestroy))]
        InspectorEvent onDestroy = new();

        bool UsesAwake => Has(InspectorGameObjectLifecycleCallbacks.Awake);
        bool UsesStart => Has(InspectorGameObjectLifecycleCallbacks.Start);
        bool UsesOnEnable => Has(InspectorGameObjectLifecycleCallbacks.OnEnable);
        bool UsesOnDisable => Has(InspectorGameObjectLifecycleCallbacks.OnDisable);
        bool UsesOnDestroy => Has(InspectorGameObjectLifecycleCallbacks.OnDestroy);

        bool Has(InspectorGameObjectLifecycleCallbacks callback) => (callbacks & callback) != 0;

        void Awake() {
            if (Has(InspectorGameObjectLifecycleCallbacks.Awake)) {
                onAwake.Invoke();
            }
        }

        void Start() {
            if (Has(InspectorGameObjectLifecycleCallbacks.Start)) {
                onStart.Invoke();
            }
        }

        void OnEnable() {
            if (Has(InspectorGameObjectLifecycleCallbacks.OnEnable)) {
                onEnable.Invoke();
            }
        }

        void OnDisable() {
            if (Has(InspectorGameObjectLifecycleCallbacks.OnDisable)) {
                onDisable.Invoke();
            }
        }

        void OnDestroy() {
            if (Has(InspectorGameObjectLifecycleCallbacks.OnDestroy)) {
                onDestroy.Invoke();
            }
        }
    }
}
