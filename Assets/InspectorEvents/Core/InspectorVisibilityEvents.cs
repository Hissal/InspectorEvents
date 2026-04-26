using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InspectorEvents.Core {
    [Flags]
    public enum InspectorVisibilityCallbacks {
        None = 0,
        OnBecameVisible = 1 << 0,
        OnBecameInvisible = 1 << 1,
        All = OnBecameVisible | OnBecameInvisible
    }

    public sealed class InspectorVisibilityEvents : MonoBehaviour {
        [SerializeField] InspectorVisibilityCallbacks callbacks = InspectorVisibilityCallbacks.None;

        [SerializeField, ShowIf(nameof(UsesOnBecameVisible))]
        InspectorEvent onBecameVisible = new();

        [SerializeField, ShowIf(nameof(UsesOnBecameInvisible))]
        InspectorEvent onBecameInvisible = new();

        bool UsesOnBecameVisible => Has(InspectorVisibilityCallbacks.OnBecameVisible);
        bool UsesOnBecameInvisible => Has(InspectorVisibilityCallbacks.OnBecameInvisible);

        bool Has(InspectorVisibilityCallbacks callback) => (callbacks & callback) != 0;

        void OnBecameVisible() {
            if (Has(InspectorVisibilityCallbacks.OnBecameVisible)) {
                onBecameVisible.Invoke();
            }
        }

        void OnBecameInvisible() {
            if (Has(InspectorVisibilityCallbacks.OnBecameInvisible)) {
                onBecameInvisible.Invoke();
            }
        }
    }
}
