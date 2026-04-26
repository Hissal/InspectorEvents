using System;
using InspectorEvents.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InspectorEvents.UGUI {
    [Flags]
    public enum InspectorPointerCallbacks {
        None = 0,
        OnPointerMove = 1 << 0,
        OnPointerEnter = 1 << 1,
        OnPointerExit = 1 << 2,
        OnPointerDown = 1 << 3,
        OnPointerUp = 1 << 4,
        OnPointerClick = 1 << 5,
        OnInitializePotentialDrag = 1 << 6,
        OnBeginDrag = 1 << 7,
        OnDrag = 1 << 8,
        OnEndDrag = 1 << 9,
        OnDrop = 1 << 10,
        OnScroll = 1 << 11,
        OnUpdateSelected = 1 << 12,
        OnSelect = 1 << 13,
        OnDeselect = 1 << 14,
        OnMove = 1 << 15,
        OnSubmit = 1 << 16,
        OnCancel = 1 << 17,
        All = OnPointerMove
              | OnPointerEnter
              | OnPointerExit
              | OnPointerDown
              | OnPointerUp
              | OnPointerClick
              | OnInitializePotentialDrag
              | OnBeginDrag
              | OnDrag
              | OnEndDrag
              | OnDrop
              | OnScroll
              | OnUpdateSelected
              | OnSelect
              | OnDeselect
              | OnMove
              | OnSubmit
              | OnCancel
    }

    public sealed class InspectorPointerEvents :
        MonoBehaviour,
        IPointerMoveHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler,
        IInitializePotentialDragHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler,
        IScrollHandler,
        IUpdateSelectedHandler,
        ISelectHandler,
        IDeselectHandler,
        IMoveHandler,
        ISubmitHandler,
        ICancelHandler
    {
        [SerializeField] InspectorPointerCallbacks callbacks = InspectorPointerCallbacks.None;

        [SerializeField, ShowIf(nameof(UsesOnPointerMove))]
        InspectorEvent<PointerEventData> onPointerMove = new();

        [SerializeField, ShowIf(nameof(UsesOnPointerEnter))]
        InspectorEvent<PointerEventData> onPointerEnter = new();

        [SerializeField, ShowIf(nameof(UsesOnPointerExit))]
        InspectorEvent<PointerEventData> onPointerExit = new();

        [SerializeField, ShowIf(nameof(UsesOnPointerDown))]
        InspectorEvent<PointerEventData> onPointerDown = new();

        [SerializeField, ShowIf(nameof(UsesOnPointerUp))]
        InspectorEvent<PointerEventData> onPointerUp = new();

        [SerializeField, ShowIf(nameof(UsesOnPointerClick))]
        InspectorEvent<PointerEventData> onPointerClick = new();

        [SerializeField, ShowIf(nameof(UsesOnInitializePotentialDrag))]
        InspectorEvent<PointerEventData> onInitializePotentialDrag = new();

        [SerializeField, ShowIf(nameof(UsesOnBeginDrag))]
        InspectorEvent<PointerEventData> onBeginDrag = new();

        [SerializeField, ShowIf(nameof(UsesOnDrag))]
        InspectorEvent<PointerEventData> onDrag = new();

        [SerializeField, ShowIf(nameof(UsesOnEndDrag))]
        InspectorEvent<PointerEventData> onEndDrag = new();

        [SerializeField, ShowIf(nameof(UsesOnDrop))]
        InspectorEvent<PointerEventData> onDrop = new();

        [SerializeField, ShowIf(nameof(UsesOnScroll))]
        InspectorEvent<PointerEventData> onScroll = new();

        [SerializeField, ShowIf(nameof(UsesOnUpdateSelected))]
        InspectorEvent<BaseEventData> onUpdateSelected = new();

        [SerializeField, ShowIf(nameof(UsesOnSelect))]
        InspectorEvent<BaseEventData> onSelect = new();

        [SerializeField, ShowIf(nameof(UsesOnDeselect))]
        InspectorEvent<BaseEventData> onDeselect = new();

        [SerializeField, ShowIf(nameof(UsesOnMove))]
        InspectorEvent<AxisEventData> onMove = new();

        [SerializeField, ShowIf(nameof(UsesOnSubmit))]
        InspectorEvent<BaseEventData> onSubmit = new();

        [SerializeField, ShowIf(nameof(UsesOnCancel))]
        InspectorEvent<BaseEventData> onCancel = new();

        bool UsesOnPointerMove => Has(InspectorPointerCallbacks.OnPointerMove);
        bool UsesOnPointerEnter => Has(InspectorPointerCallbacks.OnPointerEnter);
        bool UsesOnPointerExit => Has(InspectorPointerCallbacks.OnPointerExit);
        bool UsesOnPointerDown => Has(InspectorPointerCallbacks.OnPointerDown);
        bool UsesOnPointerUp => Has(InspectorPointerCallbacks.OnPointerUp);
        bool UsesOnPointerClick => Has(InspectorPointerCallbacks.OnPointerClick);
        bool UsesOnInitializePotentialDrag => Has(InspectorPointerCallbacks.OnInitializePotentialDrag);
        bool UsesOnBeginDrag => Has(InspectorPointerCallbacks.OnBeginDrag);
        bool UsesOnDrag => Has(InspectorPointerCallbacks.OnDrag);
        bool UsesOnEndDrag => Has(InspectorPointerCallbacks.OnEndDrag);
        bool UsesOnDrop => Has(InspectorPointerCallbacks.OnDrop);
        bool UsesOnScroll => Has(InspectorPointerCallbacks.OnScroll);
        bool UsesOnUpdateSelected => Has(InspectorPointerCallbacks.OnUpdateSelected);
        bool UsesOnSelect => Has(InspectorPointerCallbacks.OnSelect);
        bool UsesOnDeselect => Has(InspectorPointerCallbacks.OnDeselect);
        bool UsesOnMove => Has(InspectorPointerCallbacks.OnMove);
        bool UsesOnSubmit => Has(InspectorPointerCallbacks.OnSubmit);
        bool UsesOnCancel => Has(InspectorPointerCallbacks.OnCancel);

        bool Has(InspectorPointerCallbacks callback) => (callbacks & callback) != 0;

        public void OnPointerMove(PointerEventData eventData) {
            if (Has(InspectorPointerCallbacks.OnPointerMove)) {
                onPointerMove.Invoke(eventData);
            }
        }

        public void OnPointerEnter(PointerEventData eventData) {
            if (Has(InspectorPointerCallbacks.OnPointerEnter)) {
                onPointerEnter.Invoke(eventData);
            }
        }

        public void OnPointerExit(PointerEventData eventData) {
            if (Has(InspectorPointerCallbacks.OnPointerExit)) {
                onPointerExit.Invoke(eventData);
            }
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (Has(InspectorPointerCallbacks.OnPointerDown)) {
                onPointerDown.Invoke(eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData) {
            if (Has(InspectorPointerCallbacks.OnPointerUp)) {
                onPointerUp.Invoke(eventData);
            }
        }

        public void OnPointerClick(PointerEventData eventData) {
            if (Has(InspectorPointerCallbacks.OnPointerClick)) {
                onPointerClick.Invoke(eventData);
            }
        }

        public void OnInitializePotentialDrag(PointerEventData eventData) {
            if (Has(InspectorPointerCallbacks.OnInitializePotentialDrag)) {
                onInitializePotentialDrag.Invoke(eventData);
            }
        }

        public void OnBeginDrag(PointerEventData eventData) {
            if (Has(InspectorPointerCallbacks.OnBeginDrag)) {
                onBeginDrag.Invoke(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData) {
            if (Has(InspectorPointerCallbacks.OnDrag)) {
                onDrag.Invoke(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData) {
            if (Has(InspectorPointerCallbacks.OnEndDrag)) {
                onEndDrag.Invoke(eventData);
            }
        }

        public void OnDrop(PointerEventData eventData) {
            if (Has(InspectorPointerCallbacks.OnDrop)) {
                onDrop.Invoke(eventData);
            }
        }

        public void OnScroll(PointerEventData eventData) {
            if (Has(InspectorPointerCallbacks.OnScroll)) {
                onScroll.Invoke(eventData);
            }
        }

        public void OnUpdateSelected(BaseEventData eventData) {
            if (Has(InspectorPointerCallbacks.OnUpdateSelected)) {
                onUpdateSelected.Invoke(eventData);
            }
        }

        public void OnSelect(BaseEventData eventData) {
            if (Has(InspectorPointerCallbacks.OnSelect)) {
                onSelect.Invoke(eventData);
            }
        }

        public void OnDeselect(BaseEventData eventData) {
            if (Has(InspectorPointerCallbacks.OnDeselect)) {
                onDeselect.Invoke(eventData);
            }
        }

        public void OnMove(AxisEventData eventData) {
            if (Has(InspectorPointerCallbacks.OnMove)) {
                onMove.Invoke(eventData);
            }
        }

        public void OnSubmit(BaseEventData eventData) {
            if (Has(InspectorPointerCallbacks.OnSubmit)) {
                onSubmit.Invoke(eventData);
            }
        }

        public void OnCancel(BaseEventData eventData) {
            if (Has(InspectorPointerCallbacks.OnCancel)) {
                onCancel.Invoke(eventData);
            }
        }
    }
}
