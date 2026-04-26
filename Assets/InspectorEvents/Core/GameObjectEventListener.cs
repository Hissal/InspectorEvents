using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InspectorEvents.Core;

public enum GameObjectEventMessage {
    Awake,
    Start,
    OnEnable,
    OnDisable,
    OnDestroy,

    OnCollisionEnter,
    OnCollisionStay,
    OnCollisionExit,
    OnTriggerEnter,
    OnTriggerStay,
    OnTriggerExit,

    OnCollisionEnter2D,
    OnCollisionStay2D,
    OnCollisionExit2D,
    OnTriggerEnter2D,
    OnTriggerStay2D,
    OnTriggerExit2D
}

public sealed class GameObjectEventListener : MonoBehaviour {
    [SerializeField]
    List<GameObjectEventListenerEntry> listeners = new();

    void Awake() => InvokeNoPayload(GameObjectEventMessage.Awake);

    void Start() => InvokeNoPayload(GameObjectEventMessage.Start);

    void OnEnable() => InvokeNoPayload(GameObjectEventMessage.OnEnable);

    void OnDisable() => InvokeNoPayload(GameObjectEventMessage.OnDisable);

    void OnDestroy() => InvokeNoPayload(GameObjectEventMessage.OnDestroy);

    void OnCollisionEnter(Collision collision) => InvokeCollision3D(GameObjectEventMessage.OnCollisionEnter, collision);

    void OnCollisionStay(Collision collision) => InvokeCollision3D(GameObjectEventMessage.OnCollisionStay, collision);

    void OnCollisionExit(Collision collision) => InvokeCollision3D(GameObjectEventMessage.OnCollisionExit, collision);

    void OnTriggerEnter(Collider other) => InvokeTrigger3D(GameObjectEventMessage.OnTriggerEnter, other);

    void OnTriggerStay(Collider other) => InvokeTrigger3D(GameObjectEventMessage.OnTriggerStay, other);

    void OnTriggerExit(Collider other) => InvokeTrigger3D(GameObjectEventMessage.OnTriggerExit, other);

    void OnCollisionEnter2D(Collision2D collision) => InvokeCollision2D(GameObjectEventMessage.OnCollisionEnter2D, collision);

    void OnCollisionStay2D(Collision2D collision) => InvokeCollision2D(GameObjectEventMessage.OnCollisionStay2D, collision);

    void OnCollisionExit2D(Collision2D collision) => InvokeCollision2D(GameObjectEventMessage.OnCollisionExit2D, collision);

    void OnTriggerEnter2D(Collider2D other) => InvokeTrigger2D(GameObjectEventMessage.OnTriggerEnter2D, other);

    void OnTriggerStay2D(Collider2D other) => InvokeTrigger2D(GameObjectEventMessage.OnTriggerStay2D, other);

    void OnTriggerExit2D(Collider2D other) => InvokeTrigger2D(GameObjectEventMessage.OnTriggerExit2D, other);

    internal void InvokeNoPayload(GameObjectEventMessage message) {
        foreach (var listener in listeners) {
            listener.InvokeNoPayload(message);
        }
    }

    internal void InvokeCollision3D(GameObjectEventMessage message, Collision collision) {
        foreach (var listener in listeners) {
            listener.InvokeCollision3D(message, collision);
        }
    }

    internal void InvokeTrigger3D(GameObjectEventMessage message, Collider other) {
        foreach (var listener in listeners) {
            listener.InvokeTrigger3D(message, other);
        }
    }

    internal void InvokeCollision2D(GameObjectEventMessage message, Collision2D collision) {
        foreach (var listener in listeners) {
            listener.InvokeCollision2D(message, collision);
        }
    }

    internal void InvokeTrigger2D(GameObjectEventMessage message, Collider2D other) {
        foreach (var listener in listeners) {
            listener.InvokeTrigger2D(message, other);
        }
    }
}

[Serializable]
public sealed class GameObjectEventListenerEntry {
    [SerializeField] GameObjectEventMessage message;

    [SerializeField, ShowIf(nameof(UsesNoPayload)), HideLabel, InlineProperty]
    InspectorEvent noPayloadEvent = new();

    [SerializeField, ShowIf(nameof(UsesCollision3D)), HideLabel, InlineProperty]
    InspectorEvent<Collision> collision3DEvent = new();

    [SerializeField, ShowIf(nameof(UsesTrigger3D)), HideLabel, InlineProperty]
    InspectorEvent<Collider> trigger3DEvent = new();

    [SerializeField, ShowIf(nameof(UsesCollision2D)), HideLabel, InlineProperty]
    InspectorEvent<Collision2D> collision2DEvent = new();

    [SerializeField, ShowIf(nameof(UsesTrigger2D)), HideLabel, InlineProperty]
    InspectorEvent<Collider2D> trigger2DEvent = new();

    bool UsesNoPayload => message is
        GameObjectEventMessage.Awake or
        GameObjectEventMessage.Start or
        GameObjectEventMessage.OnEnable or
        GameObjectEventMessage.OnDisable or
        GameObjectEventMessage.OnDestroy;

    bool UsesCollision3D => message is
        GameObjectEventMessage.OnCollisionEnter or
        GameObjectEventMessage.OnCollisionStay or
        GameObjectEventMessage.OnCollisionExit;

    bool UsesTrigger3D => message is
        GameObjectEventMessage.OnTriggerEnter or
        GameObjectEventMessage.OnTriggerStay or
        GameObjectEventMessage.OnTriggerExit;

    bool UsesCollision2D => message is
        GameObjectEventMessage.OnCollisionEnter2D or
        GameObjectEventMessage.OnCollisionStay2D or
        GameObjectEventMessage.OnCollisionExit2D;

    bool UsesTrigger2D => message is
        GameObjectEventMessage.OnTriggerEnter2D or
        GameObjectEventMessage.OnTriggerStay2D or
        GameObjectEventMessage.OnTriggerExit2D;

    internal void InvokeNoPayload(GameObjectEventMessage invokedMessage) {
        if (message == invokedMessage && UsesNoPayload) {
            noPayloadEvent.Invoke();
        }
    }

    internal void InvokeCollision3D(GameObjectEventMessage invokedMessage, Collision collision) {
        if (message == invokedMessage && UsesCollision3D) {
            collision3DEvent.Invoke(collision);
        }
    }

    internal void InvokeTrigger3D(GameObjectEventMessage invokedMessage, Collider other) {
        if (message == invokedMessage && UsesTrigger3D) {
            trigger3DEvent.Invoke(other);
        }
    }

    internal void InvokeCollision2D(GameObjectEventMessage invokedMessage, Collision2D collision) {
        if (message == invokedMessage && UsesCollision2D) {
            collision2DEvent.Invoke(collision);
        }
    }

    internal void InvokeTrigger2D(GameObjectEventMessage invokedMessage, Collider2D other) {
        if (message == invokedMessage && UsesTrigger2D) {
            trigger2DEvent.Invoke(other);
        }
    }
}
