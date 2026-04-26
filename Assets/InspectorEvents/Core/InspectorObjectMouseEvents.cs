using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InspectorEvents.Core {
    [Flags]
    public enum InspectorObjectMouseCallbacks {
        None = 0,
        OnMouseEnter = 1 << 0,
        OnMouseExit = 1 << 1,
        OnMouseDown = 1 << 2,
        OnMouseUp = 1 << 3,
        OnMouseUpAsButton = 1 << 4,
        OnMouseDrag = 1 << 5,
        OnMouseOver = 1 << 6,
        All = OnMouseEnter | OnMouseExit | OnMouseDown | OnMouseUp | OnMouseUpAsButton | OnMouseDrag | OnMouseOver
    }

    public sealed class InspectorObjectMouseEvents : MonoBehaviour {
        [SerializeField] InspectorObjectMouseCallbacks callbacks = InspectorObjectMouseCallbacks.None;

        [SerializeField, ShowIf(nameof(UsesOnMouseEnter))]
        InspectorEvent onMouseEnter = new();

        [SerializeField, ShowIf(nameof(UsesOnMouseExit))]
        InspectorEvent onMouseExit = new();

        [SerializeField, ShowIf(nameof(UsesOnMouseDown))]
        InspectorEvent onMouseDown = new();

        [SerializeField, ShowIf(nameof(UsesOnMouseUp))]
        InspectorEvent onMouseUp = new();

        [SerializeField, ShowIf(nameof(UsesOnMouseUpAsButton))]
        InspectorEvent onMouseUpAsButton = new();

        [SerializeField, ShowIf(nameof(UsesOnMouseDrag))]
        InspectorEvent onMouseDrag = new();

        [SerializeField, ShowIf(nameof(UsesOnMouseOver))]
        InspectorEvent onMouseOver = new();

        bool UsesOnMouseEnter => Has(InspectorObjectMouseCallbacks.OnMouseEnter);
        bool UsesOnMouseExit => Has(InspectorObjectMouseCallbacks.OnMouseExit);
        bool UsesOnMouseDown => Has(InspectorObjectMouseCallbacks.OnMouseDown);
        bool UsesOnMouseUp => Has(InspectorObjectMouseCallbacks.OnMouseUp);
        bool UsesOnMouseUpAsButton => Has(InspectorObjectMouseCallbacks.OnMouseUpAsButton);
        bool UsesOnMouseDrag => Has(InspectorObjectMouseCallbacks.OnMouseDrag);
        bool UsesOnMouseOver => Has(InspectorObjectMouseCallbacks.OnMouseOver);

        bool Has(InspectorObjectMouseCallbacks callback) => (callbacks & callback) != 0;

        void OnMouseEnter() => Invoke(InspectorObjectMouseCallbacks.OnMouseEnter);

        void OnMouseExit() => Invoke(InspectorObjectMouseCallbacks.OnMouseExit);

        void OnMouseDown() => Invoke(InspectorObjectMouseCallbacks.OnMouseDown);

        void OnMouseUp() => Invoke(InspectorObjectMouseCallbacks.OnMouseUp);

        void OnMouseUpAsButton() => Invoke(InspectorObjectMouseCallbacks.OnMouseUpAsButton);

        void OnMouseDrag() => Invoke(InspectorObjectMouseCallbacks.OnMouseDrag);

        void OnMouseOver() => Invoke(InspectorObjectMouseCallbacks.OnMouseOver);

        internal void Invoke(InspectorObjectMouseCallbacks callback) {
            if (!Has(callback)) {
                return;
            }

            switch (callback) {
                case InspectorObjectMouseCallbacks.OnMouseEnter:
                    onMouseEnter.Invoke();
                    break;
                case InspectorObjectMouseCallbacks.OnMouseExit:
                    onMouseExit.Invoke();
                    break;
                case InspectorObjectMouseCallbacks.OnMouseDown:
                    onMouseDown.Invoke();
                    break;
                case InspectorObjectMouseCallbacks.OnMouseUp:
                    onMouseUp.Invoke();
                    break;
                case InspectorObjectMouseCallbacks.OnMouseUpAsButton:
                    onMouseUpAsButton.Invoke();
                    break;
                case InspectorObjectMouseCallbacks.OnMouseDrag:
                    onMouseDrag.Invoke();
                    break;
                case InspectorObjectMouseCallbacks.OnMouseOver:
                    onMouseOver.Invoke();
                    break;
            }
        }
    }
}
