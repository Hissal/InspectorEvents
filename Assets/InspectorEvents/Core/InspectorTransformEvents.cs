using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InspectorEvents.Core {
    [Flags]
    public enum InspectorTransformCallbacks {
        None = 0,
        OnTransformChildrenChanged = 1 << 0,
        OnTransformParentChanged = 1 << 1,
        All = OnTransformChildrenChanged | OnTransformParentChanged
    }

    public sealed class InspectorTransformEvents : MonoBehaviour {
        [SerializeField] InspectorTransformCallbacks callbacks = InspectorTransformCallbacks.None;

        [SerializeField, ShowIf(nameof(UsesOnTransformChildrenChanged))]
        InspectorEvent onTransformChildrenChanged = new();

        [SerializeField, ShowIf(nameof(UsesOnTransformParentChanged))]
        InspectorEvent onTransformParentChanged = new();

        bool UsesOnTransformChildrenChanged => Has(InspectorTransformCallbacks.OnTransformChildrenChanged);
        bool UsesOnTransformParentChanged => Has(InspectorTransformCallbacks.OnTransformParentChanged);

        bool Has(InspectorTransformCallbacks callback) => (callbacks & callback) != 0;

        void OnTransformChildrenChanged() {
            if (Has(InspectorTransformCallbacks.OnTransformChildrenChanged)) {
                onTransformChildrenChanged.Invoke();
            }
        }

        void OnTransformParentChanged() {
            if (Has(InspectorTransformCallbacks.OnTransformParentChanged)) {
                onTransformParentChanged.Invoke();
            }
        }
    }
}
