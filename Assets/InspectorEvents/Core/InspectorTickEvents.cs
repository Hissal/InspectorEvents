using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InspectorEvents.Core {
    [Flags]
    public enum InspectorTickCallbacks {
        None = 0,
        Update = 1 << 0,
        FixedUpdate = 1 << 1,
        LateUpdate = 1 << 2,
        All = Update | FixedUpdate | LateUpdate
    }

    public sealed class InspectorTickEvents : MonoBehaviour {
        [SerializeField] InspectorTickCallbacks callbacks = InspectorTickCallbacks.None;

        [SerializeField, ShowIf(nameof(UsesUpdate))]
        InspectorEvent onUpdate = new();

        [SerializeField, ShowIf(nameof(UsesFixedUpdate))]
        InspectorEvent onFixedUpdate = new();

        [SerializeField, ShowIf(nameof(UsesLateUpdate))]
        InspectorEvent onLateUpdate = new();

        bool UsesUpdate => Has(InspectorTickCallbacks.Update);
        bool UsesFixedUpdate => Has(InspectorTickCallbacks.FixedUpdate);
        bool UsesLateUpdate => Has(InspectorTickCallbacks.LateUpdate);

        bool Has(InspectorTickCallbacks callback) => (callbacks & callback) != 0;

        void Update() => Invoke(InspectorTickCallbacks.Update);

        void FixedUpdate() => Invoke(InspectorTickCallbacks.FixedUpdate);

        void LateUpdate() => Invoke(InspectorTickCallbacks.LateUpdate);

        internal void Invoke(InspectorTickCallbacks callback) {
            if (!Has(callback)) {
                return;
            }

            switch (callback) {
                case InspectorTickCallbacks.Update:
                    onUpdate.Invoke();
                    break;
                case InspectorTickCallbacks.FixedUpdate:
                    onFixedUpdate.Invoke();
                    break;
                case InspectorTickCallbacks.LateUpdate:
                    onLateUpdate.Invoke();
                    break;
            }
        }
    }
}
