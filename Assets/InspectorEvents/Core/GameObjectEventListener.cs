using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InspectorEvents.Core;

[Flags]
public enum GameObjectEventCallbacks {
    None = 0,

    Awake = 1 << 0,
    Start = 1 << 1,
    OnEnable = 1 << 2,
    OnDisable = 1 << 3,
    OnDestroy = 1 << 4,

    OnCollisionEnter = 1 << 5,
    OnCollisionStay = 1 << 6,
    OnCollisionExit = 1 << 7,
    OnTriggerEnter = 1 << 8,
    OnTriggerStay = 1 << 9,
    OnTriggerExit = 1 << 10,

    OnCollisionEnter2D = 1 << 11,
    OnCollisionStay2D = 1 << 12,
    OnCollisionExit2D = 1 << 13,
    OnTriggerEnter2D = 1 << 14,
    OnTriggerStay2D = 1 << 15,
    OnTriggerExit2D = 1 << 16,

    All = Awake
          | Start
          | OnEnable
          | OnDisable
          | OnDestroy
          | OnCollisionEnter
          | OnCollisionStay
          | OnCollisionExit
          | OnTriggerEnter
          | OnTriggerStay
          | OnTriggerExit
          | OnCollisionEnter2D
          | OnCollisionStay2D
          | OnCollisionExit2D
          | OnTriggerEnter2D
          | OnTriggerStay2D
          | OnTriggerExit2D
}

public sealed class GameObjectEventListener : MonoBehaviour {
    [SerializeField] GameObjectEventCallbacks callbacks = GameObjectEventCallbacks.None;

    [SerializeField, ShowIf(nameof(UsesAwake)), HideLabel, InlineProperty]
    InspectorEvent onAwake = new();

    [SerializeField, ShowIf(nameof(UsesStart)), HideLabel, InlineProperty]
    InspectorEvent onStart = new();

    [SerializeField, ShowIf(nameof(UsesOnEnable)), HideLabel, InlineProperty]
    InspectorEvent onEnable = new();

    [SerializeField, ShowIf(nameof(UsesOnDisable)), HideLabel, InlineProperty]
    InspectorEvent onDisable = new();

    [SerializeField, ShowIf(nameof(UsesOnDestroy)), HideLabel, InlineProperty]
    InspectorEvent onDestroy = new();

    [SerializeField, ShowIf(nameof(UsesOnCollisionEnter)), HideLabel, InlineProperty]
    InspectorEvent<Collision> onCollisionEnter = new();

    [SerializeField, ShowIf(nameof(UsesOnCollisionStay)), HideLabel, InlineProperty]
    InspectorEvent<Collision> onCollisionStay = new();

    [SerializeField, ShowIf(nameof(UsesOnCollisionExit)), HideLabel, InlineProperty]
    InspectorEvent<Collision> onCollisionExit = new();

    [SerializeField, ShowIf(nameof(UsesOnTriggerEnter)), HideLabel, InlineProperty]
    InspectorEvent<Collider> onTriggerEnter = new();

    [SerializeField, ShowIf(nameof(UsesOnTriggerStay)), HideLabel, InlineProperty]
    InspectorEvent<Collider> onTriggerStay = new();

    [SerializeField, ShowIf(nameof(UsesOnTriggerExit)), HideLabel, InlineProperty]
    InspectorEvent<Collider> onTriggerExit = new();

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

    bool UsesAwake => Has(GameObjectEventCallbacks.Awake);
    bool UsesStart => Has(GameObjectEventCallbacks.Start);
    bool UsesOnEnable => Has(GameObjectEventCallbacks.OnEnable);
    bool UsesOnDisable => Has(GameObjectEventCallbacks.OnDisable);
    bool UsesOnDestroy => Has(GameObjectEventCallbacks.OnDestroy);
    bool UsesOnCollisionEnter => Has(GameObjectEventCallbacks.OnCollisionEnter);
    bool UsesOnCollisionStay => Has(GameObjectEventCallbacks.OnCollisionStay);
    bool UsesOnCollisionExit => Has(GameObjectEventCallbacks.OnCollisionExit);
    bool UsesOnTriggerEnter => Has(GameObjectEventCallbacks.OnTriggerEnter);
    bool UsesOnTriggerStay => Has(GameObjectEventCallbacks.OnTriggerStay);
    bool UsesOnTriggerExit => Has(GameObjectEventCallbacks.OnTriggerExit);
    bool UsesOnCollisionEnter2D => Has(GameObjectEventCallbacks.OnCollisionEnter2D);
    bool UsesOnCollisionStay2D => Has(GameObjectEventCallbacks.OnCollisionStay2D);
    bool UsesOnCollisionExit2D => Has(GameObjectEventCallbacks.OnCollisionExit2D);
    bool UsesOnTriggerEnter2D => Has(GameObjectEventCallbacks.OnTriggerEnter2D);
    bool UsesOnTriggerStay2D => Has(GameObjectEventCallbacks.OnTriggerStay2D);
    bool UsesOnTriggerExit2D => Has(GameObjectEventCallbacks.OnTriggerExit2D);

    bool Has(GameObjectEventCallbacks callback) => (callbacks & callback) != 0;

    void Awake() => InvokeNoPayload(GameObjectEventCallbacks.Awake);

    void Start() => InvokeNoPayload(GameObjectEventCallbacks.Start);

    void OnEnable() => InvokeNoPayload(GameObjectEventCallbacks.OnEnable);

    void OnDisable() => InvokeNoPayload(GameObjectEventCallbacks.OnDisable);

    void OnDestroy() => InvokeNoPayload(GameObjectEventCallbacks.OnDestroy);

    void OnCollisionEnter(Collision collision) => InvokeCollision3D(GameObjectEventCallbacks.OnCollisionEnter, collision);

    void OnCollisionStay(Collision collision) => InvokeCollision3D(GameObjectEventCallbacks.OnCollisionStay, collision);

    void OnCollisionExit(Collision collision) => InvokeCollision3D(GameObjectEventCallbacks.OnCollisionExit, collision);

    void OnTriggerEnter(Collider other) => InvokeTrigger3D(GameObjectEventCallbacks.OnTriggerEnter, other);

    void OnTriggerStay(Collider other) => InvokeTrigger3D(GameObjectEventCallbacks.OnTriggerStay, other);

    void OnTriggerExit(Collider other) => InvokeTrigger3D(GameObjectEventCallbacks.OnTriggerExit, other);

    void OnCollisionEnter2D(Collision2D collision) => InvokeCollision2D(GameObjectEventCallbacks.OnCollisionEnter2D, collision);

    void OnCollisionStay2D(Collision2D collision) => InvokeCollision2D(GameObjectEventCallbacks.OnCollisionStay2D, collision);

    void OnCollisionExit2D(Collision2D collision) => InvokeCollision2D(GameObjectEventCallbacks.OnCollisionExit2D, collision);

    void OnTriggerEnter2D(Collider2D other) => InvokeTrigger2D(GameObjectEventCallbacks.OnTriggerEnter2D, other);

    void OnTriggerStay2D(Collider2D other) => InvokeTrigger2D(GameObjectEventCallbacks.OnTriggerStay2D, other);

    void OnTriggerExit2D(Collider2D other) => InvokeTrigger2D(GameObjectEventCallbacks.OnTriggerExit2D, other);

    internal void InvokeNoPayload(GameObjectEventCallbacks callback) {
        if (!Has(callback)) {
            return;
        }

        switch (callback) {
            case GameObjectEventCallbacks.Awake:
                onAwake.Invoke();
                break;
            case GameObjectEventCallbacks.Start:
                onStart.Invoke();
                break;
            case GameObjectEventCallbacks.OnEnable:
                onEnable.Invoke();
                break;
            case GameObjectEventCallbacks.OnDisable:
                onDisable.Invoke();
                break;
            case GameObjectEventCallbacks.OnDestroy:
                onDestroy.Invoke();
                break;
        }
    }

    internal void InvokeCollision3D(GameObjectEventCallbacks callback, Collision collision) {
        if (!Has(callback)) {
            return;
        }

        switch (callback) {
            case GameObjectEventCallbacks.OnCollisionEnter:
                onCollisionEnter.Invoke(collision);
                break;
            case GameObjectEventCallbacks.OnCollisionStay:
                onCollisionStay.Invoke(collision);
                break;
            case GameObjectEventCallbacks.OnCollisionExit:
                onCollisionExit.Invoke(collision);
                break;
        }
    }

    internal void InvokeTrigger3D(GameObjectEventCallbacks callback, Collider other) {
        if (!Has(callback)) {
            return;
        }

        switch (callback) {
            case GameObjectEventCallbacks.OnTriggerEnter:
                onTriggerEnter.Invoke(other);
                break;
            case GameObjectEventCallbacks.OnTriggerStay:
                onTriggerStay.Invoke(other);
                break;
            case GameObjectEventCallbacks.OnTriggerExit:
                onTriggerExit.Invoke(other);
                break;
        }
    }

    internal void InvokeCollision2D(GameObjectEventCallbacks callback, Collision2D collision) {
        if (!Has(callback)) {
            return;
        }

        switch (callback) {
            case GameObjectEventCallbacks.OnCollisionEnter2D:
                onCollisionEnter2D.Invoke(collision);
                break;
            case GameObjectEventCallbacks.OnCollisionStay2D:
                onCollisionStay2D.Invoke(collision);
                break;
            case GameObjectEventCallbacks.OnCollisionExit2D:
                onCollisionExit2D.Invoke(collision);
                break;
        }
    }

    internal void InvokeTrigger2D(GameObjectEventCallbacks callback, Collider2D other) {
        if (!Has(callback)) {
            return;
        }

        switch (callback) {
            case GameObjectEventCallbacks.OnTriggerEnter2D:
                onTriggerEnter2D.Invoke(other);
                break;
            case GameObjectEventCallbacks.OnTriggerStay2D:
                onTriggerStay2D.Invoke(other);
                break;
            case GameObjectEventCallbacks.OnTriggerExit2D:
                onTriggerExit2D.Invoke(other);
                break;
        }
    }
}
