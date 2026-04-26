using System;
using InspectorEvents.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InspectorEvents.Physics3D {
    [Flags]
    public enum InspectorPhysics3DCallbacks {
        None = 0,
        OnCollisionEnter = 1 << 0,
        OnCollisionStay = 1 << 1,
        OnCollisionExit = 1 << 2,
        OnTriggerEnter = 1 << 3,
        OnTriggerStay = 1 << 4,
        OnTriggerExit = 1 << 5,
        OnControllerColliderHit = 1 << 6,
        OnJointBreak = 1 << 7,
        All = OnCollisionEnter
              | OnCollisionStay
              | OnCollisionExit
              | OnTriggerEnter
              | OnTriggerStay
              | OnTriggerExit
              | OnControllerColliderHit
              | OnJointBreak
    }

    public sealed class InspectorPhysics3DEvents : MonoBehaviour {
        [SerializeField] InspectorPhysics3DCallbacks callbacks = InspectorPhysics3DCallbacks.None;

        [SerializeField, ShowIf(nameof(UsesOnCollisionEnter))]
        InspectorEvent<Collision> onCollisionEnter = new();

        [SerializeField, ShowIf(nameof(UsesOnCollisionStay))]
        InspectorEvent<Collision> onCollisionStay = new();

        [SerializeField, ShowIf(nameof(UsesOnCollisionExit))]
        InspectorEvent<Collision> onCollisionExit = new();

        [SerializeField, ShowIf(nameof(UsesOnTriggerEnter))]
        InspectorEvent<Collider> onTriggerEnter = new();

        [SerializeField, ShowIf(nameof(UsesOnTriggerStay))]
        InspectorEvent<Collider> onTriggerStay = new();

        [SerializeField, ShowIf(nameof(UsesOnTriggerExit))]
        InspectorEvent<Collider> onTriggerExit = new();

        [SerializeField, ShowIf(nameof(UsesOnControllerColliderHit))]
        InspectorEvent<ControllerColliderHit> onControllerColliderHit = new();

        [SerializeField, ShowIf(nameof(UsesOnJointBreak))]
        InspectorEvent<float> onJointBreak = new();

        bool UsesOnCollisionEnter => Has(InspectorPhysics3DCallbacks.OnCollisionEnter);
        bool UsesOnCollisionStay => Has(InspectorPhysics3DCallbacks.OnCollisionStay);
        bool UsesOnCollisionExit => Has(InspectorPhysics3DCallbacks.OnCollisionExit);
        bool UsesOnTriggerEnter => Has(InspectorPhysics3DCallbacks.OnTriggerEnter);
        bool UsesOnTriggerStay => Has(InspectorPhysics3DCallbacks.OnTriggerStay);
        bool UsesOnTriggerExit => Has(InspectorPhysics3DCallbacks.OnTriggerExit);
        bool UsesOnControllerColliderHit => Has(InspectorPhysics3DCallbacks.OnControllerColliderHit);
        bool UsesOnJointBreak => Has(InspectorPhysics3DCallbacks.OnJointBreak);

        bool Has(InspectorPhysics3DCallbacks callback) => (callbacks & callback) != 0;

        void OnCollisionEnter(Collision collision) {
            if (Has(InspectorPhysics3DCallbacks.OnCollisionEnter)) {
                onCollisionEnter.Invoke(collision);
            }
        }

        void OnCollisionStay(Collision collision) {
            if (Has(InspectorPhysics3DCallbacks.OnCollisionStay)) {
                onCollisionStay.Invoke(collision);
            }
        }

        void OnCollisionExit(Collision collision) {
            if (Has(InspectorPhysics3DCallbacks.OnCollisionExit)) {
                onCollisionExit.Invoke(collision);
            }
        }

        void OnTriggerEnter(Collider other) {
            if (Has(InspectorPhysics3DCallbacks.OnTriggerEnter)) {
                onTriggerEnter.Invoke(other);
            }
        }

        void OnTriggerStay(Collider other) {
            if (Has(InspectorPhysics3DCallbacks.OnTriggerStay)) {
                onTriggerStay.Invoke(other);
            }
        }

        void OnTriggerExit(Collider other) {
            if (Has(InspectorPhysics3DCallbacks.OnTriggerExit)) {
                onTriggerExit.Invoke(other);
            }
        }

        void OnControllerColliderHit(ControllerColliderHit hit) {
            if (Has(InspectorPhysics3DCallbacks.OnControllerColliderHit)) {
                onControllerColliderHit.Invoke(hit);
            }
        }

        void OnJointBreak(float breakForce) {
            if (Has(InspectorPhysics3DCallbacks.OnJointBreak)) {
                onJointBreak.Invoke(breakForce);
            }
        }
    }
}
