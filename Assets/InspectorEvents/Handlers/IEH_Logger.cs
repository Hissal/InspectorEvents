using System;
using InspectorEvents.Core;
using UnityEngine;

namespace InspectorEvents.Listeners;

internal enum LogLevel {
    Debug,
    Warning,
    Error
}

[Serializable]
public sealed class IEH_Logger : IInspectorEventHandler {
    [SerializeField] LogLevel logLevel;
    [SerializeField] string message;

    public void Handle() {
        switch (logLevel) {
            case LogLevel.Debug:
                Debug.Log(message);
                break;
            case LogLevel.Warning:
                Debug.LogWarning(message);
                break;
            case LogLevel.Error:
                Debug.LogError(message);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

[Serializable]
public sealed class IEH_Logger<TEvent> : IInspectorEventHandler<TEvent> {
    [SerializeField] LogLevel logLevel = LogLevel.Debug;
    [SerializeField] string prefix = "[SerializedEventLogger]";
    
    public void Handle(in TEvent e) {
        switch (logLevel) {
            case LogLevel.Debug:
                Debug.Log($"{prefix} : {e?.ToString() ?? "null"}");
                break;
            case LogLevel.Warning:
                Debug.LogWarning($"{prefix} : {e?.ToString() ?? "null"}");
                break;
            case LogLevel.Error:
                Debug.LogError($"{prefix} : {e?.ToString() ?? "null"}");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}