using UnityEngine;

namespace InspectorEvents {
    public enum LifecycleMessage {
        None,

        Start,
        Destroy,
        Enable,
        Disable,

        TriggerEnter,
        TriggerExit,
        CollisionEnter,
        CollisionExit,

        // ObjectMouseEnter,
        // ObjectMouseExit,
        // ObjectMouseDown,
        // ObjectMouseUp,
        //
        // UIPointerEnter,
        // UIPointerExit,
        // UIPointerDown,
        // UIPointerUp
    }

    public abstract class EventListenerLifecycleHandler : MonoBehaviour
// , IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        // Lifecycle events
        void Start() {
            HandleLifecycleMessage(LifecycleMessage.Start);
        }

        void OnDestroy() {
            HandleLifecycleMessage(LifecycleMessage.Destroy);
        }

        void OnEnable() {
            HandleLifecycleMessage(LifecycleMessage.Enable);
        }

        void OnDisable() {
            HandleLifecycleMessage(LifecycleMessage.Disable);
        }

        // Trigger events
        void OnTriggerEnter(Collider other) {
            HandleLifecycleMessage(LifecycleMessage.TriggerEnter);
        }

        void OnTriggerExit(Collider other) {
            HandleLifecycleMessage(LifecycleMessage.TriggerExit);
        }

        // Collision events
        void OnCollisionEnter(Collision collision) {
            HandleLifecycleMessage(LifecycleMessage.CollisionEnter);
        }

        void OnCollisionExit(Collision collision) {
            HandleLifecycleMessage(LifecycleMessage.CollisionExit);
        }

        // // Object Mouse events
        // void OnMouseEnter() {
        //    HandleObjectMessage(GameObjectMessage.ObjectMouseEnter);
        // }
        //
        // void OnMouseExit() {
        //    HandleObjectMessage(GameObjectMessage.ObjectMouseExit);
        // }
        //
        // void OnMouseDown() {
        //    HandleObjectMessage(GameObjectMessage.ObjectMouseDown);
        // }
        //
        // void OnMouseUp() {
        //    HandleObjectMessage(GameObjectMessage.ObjectMouseUp);
        // }
        //
        // // UI mouse events
        // void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        //    HandleObjectMessage(GameObjectMessage.UIPointerEnter);
        // }
        //
        // void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        //    HandleObjectMessage(GameObjectMessage.UIPointerExit);
        // }
        //
        // void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
        //    HandleObjectMessage(GameObjectMessage.UIPointerDown);
        // }
        //
        // void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
        //    HandleObjectMessage(GameObjectMessage.UIPointerUp);
        // }

        protected abstract void HandleLifecycleMessage(LifecycleMessage message);
    }
}