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

        public void OnPointerMove(PointerEventData eventData) => InvokePointer(InspectorPointerCallbacks.OnPointerMove, eventData);

        public void OnPointerEnter(PointerEventData eventData) => InvokePointer(InspectorPointerCallbacks.OnPointerEnter, eventData);

        public void OnPointerExit(PointerEventData eventData) => InvokePointer(InspectorPointerCallbacks.OnPointerExit, eventData);

        public void OnPointerDown(PointerEventData eventData) => InvokePointer(InspectorPointerCallbacks.OnPointerDown, eventData);

        public void OnPointerUp(PointerEventData eventData) => InvokePointer(InspectorPointerCallbacks.OnPointerUp, eventData);

        public void OnPointerClick(PointerEventData eventData) => InvokePointer(InspectorPointerCallbacks.OnPointerClick, eventData);

        public void OnInitializePotentialDrag(PointerEventData eventData) => InvokePointer(InspectorPointerCallbacks.OnInitializePotentialDrag, eventData);

        public void OnBeginDrag(PointerEventData eventData) => InvokePointer(InspectorPointerCallbacks.OnBeginDrag, eventData);

        public void OnDrag(PointerEventData eventData) => InvokePointer(InspectorPointerCallbacks.OnDrag, eventData);

        public void OnEndDrag(PointerEventData eventData) => InvokePointer(InspectorPointerCallbacks.OnEndDrag, eventData);

        public void OnDrop(PointerEventData eventData) => InvokePointer(InspectorPointerCallbacks.OnDrop, eventData);

        public void OnScroll(PointerEventData eventData) => InvokePointer(InspectorPointerCallbacks.OnScroll, eventData);

        public void OnUpdateSelected(BaseEventData eventData) => InvokeBase(InspectorPointerCallbacks.OnUpdateSelected, eventData);

        public void OnSelect(BaseEventData eventData) => InvokeBase(InspectorPointerCallbacks.OnSelect, eventData);

        public void OnDeselect(BaseEventData eventData) => InvokeBase(InspectorPointerCallbacks.OnDeselect, eventData);

        public void OnMove(AxisEventData eventData) => InvokeMove(eventData);

        public void OnSubmit(BaseEventData eventData) => InvokeBase(InspectorPointerCallbacks.OnSubmit, eventData);

        public void OnCancel(BaseEventData eventData) => InvokeBase(InspectorPointerCallbacks.OnCancel, eventData);

        internal void InvokePointer(InspectorPointerCallbacks callback, PointerEventData eventData) {
            if (!Has(callback)) {
                return;
            }

            switch (callback) {
                case InspectorPointerCallbacks.OnPointerMove:
                    onPointerMove.Invoke(eventData);
                    break;
                case InspectorPointerCallbacks.OnPointerEnter:
                    onPointerEnter.Invoke(eventData);
                    break;
                case InspectorPointerCallbacks.OnPointerExit:
                    onPointerExit.Invoke(eventData);
                    break;
                case InspectorPointerCallbacks.OnPointerDown:
                    onPointerDown.Invoke(eventData);
                    break;
                case InspectorPointerCallbacks.OnPointerUp:
                    onPointerUp.Invoke(eventData);
                    break;
                case InspectorPointerCallbacks.OnPointerClick:
                    onPointerClick.Invoke(eventData);
                    break;
                case InspectorPointerCallbacks.OnInitializePotentialDrag:
                    onInitializePotentialDrag.Invoke(eventData);
                    break;
                case InspectorPointerCallbacks.OnBeginDrag:
                    onBeginDrag.Invoke(eventData);
                    break;
                case InspectorPointerCallbacks.OnDrag:
                    onDrag.Invoke(eventData);
                    break;
                case InspectorPointerCallbacks.OnEndDrag:
                    onEndDrag.Invoke(eventData);
                    break;
                case InspectorPointerCallbacks.OnDrop:
                    onDrop.Invoke(eventData);
                    break;
                case InspectorPointerCallbacks.OnScroll:
                    onScroll.Invoke(eventData);
                    break;
            }
        }

        internal void InvokeBase(InspectorPointerCallbacks callback, BaseEventData eventData) {
            if (!Has(callback)) {
                return;
            }

            switch (callback) {
                case InspectorPointerCallbacks.OnUpdateSelected:
                    onUpdateSelected.Invoke(eventData);
                    break;
                case InspectorPointerCallbacks.OnSelect:
                    onSelect.Invoke(eventData);
                    break;
                case InspectorPointerCallbacks.OnDeselect:
                    onDeselect.Invoke(eventData);
                    break;
                case InspectorPointerCallbacks.OnSubmit:
                    onSubmit.Invoke(eventData);
                    break;
                case InspectorPointerCallbacks.OnCancel:
                    onCancel.Invoke(eventData);
                    break;
            }
        }

        internal void InvokeMove(AxisEventData eventData) {
            if (Has(InspectorPointerCallbacks.OnMove)) {
                onMove.Invoke(eventData);
            }
        }
    }
}
