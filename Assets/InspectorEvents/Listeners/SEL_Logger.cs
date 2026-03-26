using System;
using UnityEngine;

namespace InspectorEvents.Listeners;

[Serializable]
public sealed class SEL_Logger<TEvent> : ISerializedEventListener<TEvent> {
    [SerializeField] LogLevel logLevel = LogLevel.Debug;
    [SerializeField] string prefix = "[SerializedEventLogger]";
    
    public void OnEvent(in TEvent e) {
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
    
    enum LogLevel {
        Debug,
        Warning,
        Error
    }
}