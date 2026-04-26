using System;
using InspectorEvents.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InspectorEvents.Physics2D;

[Flags]
public enum InspectorPhysics2DCallbacks {
    None = 0,
    OnCollisionEnter2D = 1 << 0,
    OnCollisionStay2D = 1 << 1,
    OnCollisionExit2D = 1 << 2,
    OnTriggerEnter2D = 1 << 3,
    OnTriggerStay2D = 1 << 4,
    OnTriggerExit2D = 1 << 5,
    OnJointBreak2D = 1 << 6,
    All = OnCollisionEnter2D
          | OnCollisionStay2D
          | OnCollisionExit2D
          | OnTriggerEnter2D
          | OnTriggerStay2D
          | OnTriggerExit2D
          | OnJointBreak2D
}

public sealed class InspectorPhysics2DEvents : MonoBehaviour {
    [SerializeField] InspectorPhysics2DCallbacks callbacks = InspectorPhysics2DCallbacks.None;

    [SerializeField, ShowIf(nameof(UsesOnCollisionEnter2D)), HideLabel, InlineProperty]
    InspectorEvent<Collision2D> onCollisionEnter2D = new();

    [SerializeField, ShowIf(nameof(UsesOnCollisionStay2D)), HideLabel, InlineProperty]
    InspectorEvent<Collision2D> onCollisionStay2D = new();

    [SerializeField, ShowIf(nameof(UsesOnCollisionExit2D)), HideLabel, InlineProperty]
    InspectorEvent<Collision2D> onCollisionExit2D = new();

    [SerializeField, ShowIf(nameof(UsesOnTriggerEnter2D)), HideLabel, InlineProperty]
    InspectorEvent<Collider2D> onTriggerEnter2D = new();

    [SerializeField, ShowIf(nameof(UsesOnTriggerStay2D)), HideLabel, InlineProperty]
    InspectorEvent<Collider2D> onTriggerStay2D = new();

    [SerializeField, ShowIf(nameof(UsesOnTriggerExit2D)), HideLabel, InlineProperty]
    InspectorEvent<Collider2D> onTriggerExit2D = new();

    [SerializeField, ShowIf(nameof(UsesOnJointBreak2D)), HideLabel, InlineProperty]
    InspectorEvent<Joint2D> onJointBreak2D = new();

    bool UsesOnCollisionEnter2D => Has(InspectorPhysics2DCallbacks.OnCollisionEnter2D);
    bool UsesOnCollisionStay2D => Has(InspectorPhysics2DCallbacks.OnCollisionStay2D);
    bool UsesOnCollisionExit2D => Has(InspectorPhysics2DCallbacks.OnCollisionExit2D);
    bool UsesOnTriggerEnter2D => Has(InspectorPhysics2DCallbacks.OnTriggerEnter2D);
    bool UsesOnTriggerStay2D => Has(InspectorPhysics2DCallbacks.OnTriggerStay2D);
    bool UsesOnTriggerExit2D => Has(InspectorPhysics2DCallbacks.OnTriggerExit2D);
    bool UsesOnJointBreak2D => Has(InspectorPhysics2DCallbacks.OnJointBreak2D);

    bool Has(InspectorPhysics2DCallbacks callback) => (callbacks & callback) != 0;

    void OnCollisionEnter2D(Collision2D collision) => InvokeCollision(InspectorPhysics2DCallbacks.OnCollisionEnter2D, collision);

    void OnCollisionStay2D(Collision2D collision) => InvokeCollision(InspectorPhysics2DCallbacks.OnCollisionStay2D, collision);

    void OnCollisionExit2D(Collision2D collision) => InvokeCollision(InspectorPhysics2DCallbacks.OnCollisionExit2D, collision);

    void OnTriggerEnter2D(Collider2D other) => InvokeTrigger(InspectorPhysics2DCallbacks.OnTriggerEnter2D, other);

    void OnTriggerStay2D(Collider2D other) => InvokeTrigger(InspectorPhysics2DCallbacks.OnTriggerStay2D, other);

    void OnTriggerExit2D(Collider2D other) => InvokeTrigger(InspectorPhysics2DCallbacks.OnTriggerExit2D, other);

    void OnJointBreak2D(Joint2D brokenJoint) => InvokeJointBreak(brokenJoint);

    internal void InvokeCollision(InspectorPhysics2DCallbacks callback, Collision2D collision) {
        if (!Has(callback)) {
            return;
        }

        switch (callback) {
            case InspectorPhysics2DCallbacks.OnCollisionEnter2D:
                onCollisionEnter2D.Invoke(collision);
                break;
            case InspectorPhysics2DCallbacks.OnCollisionStay2D:
                onCollisionStay2D.Invoke(collision);
                break;
            case InspectorPhysics2DCallbacks.OnCollisionExit2D:
                onCollisionExit2D.Invoke(collision);
                break;
        }
    }

    internal void InvokeTrigger(InspectorPhysics2DCallbacks callback, Collider2D other) {
        if (!Has(callback)) {
            return;
        }

        switch (callback) {
            case InspectorPhysics2DCallbacks.OnTriggerEnter2D:
                onTriggerEnter2D.Invoke(other);
                break;
            case InspectorPhysics2DCallbacks.OnTriggerStay2D:
                onTriggerStay2D.Invoke(other);
                break;
            case InspectorPhysics2DCallbacks.OnTriggerExit2D:
                onTriggerExit2D.Invoke(other);
                break;
        }
    }

    internal void InvokeJointBreak(Joint2D brokenJoint) {
        if (Has(InspectorPhysics2DCallbacks.OnJointBreak2D)) {
            onJointBreak2D.Invoke(brokenJoint);
        }
    }
}
