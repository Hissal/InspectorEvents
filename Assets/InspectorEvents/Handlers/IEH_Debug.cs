using System;
using InspectorEvents.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InspectorEvents.Handlers;

internal enum DebugAction {
    Log,
    Break
    
}
internal enum LogLevel {
    Debug,
    Warning,
    Error
}

[Serializable]
public sealed class IEH_Debug : IInspectorEventHandler {
    [SerializeField] DebugAction action = DebugAction.Log;
    [SerializeField, ShowIf(nameof(action), DebugAction.Log)] LogLevel logLevel = LogLevel.Debug;
    [SerializeField, ShowIf(nameof(action), DebugAction.Log)] string prefix = "[SerializedEventLogger] : ";
    [SerializeField, ShowIf(nameof(action), DebugAction.Log)] string message = "Log message";


    public void Handle() {
#if DEBUG
        switch (action) {
            case DebugAction.Log:
                var formattedMessage = prefix + message;
                switch (logLevel) {
                    case LogLevel.Debug:
                        Debug.Log(formattedMessage);
                        break;
                    case LogLevel.Warning:
                        Debug.LogWarning(formattedMessage);
                        break;
                    case LogLevel.Error:
                        Debug.LogError(formattedMessage);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                break;
            case DebugAction.Break:
                Debug.Break();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
#endif
    }
}

[Serializable]
public sealed class IEH_Debug<TEvent> : IInspectorEventHandler<TEvent> {
    [SerializeField] DebugAction action = DebugAction.Log;
    [SerializeField, ShowIf(nameof(action), DebugAction.Log)] LogLevel logLevel = LogLevel.Debug;
    [SerializeField, ShowIf(nameof(action), DebugAction.Log)] string prefix = "[SerializedEventLogger<<type>>] : ";
    [SerializeField, ShowIf(nameof(action), DebugAction.Log)] string message = "<value>";
    
    public void Handle(in TEvent e) {
#if DEBUG
        switch (action) {
            case DebugAction.Log:
                var formattedMessage = GetFormattedMessage(e);
                switch (logLevel) {
                    case LogLevel.Debug:
                        Debug.Log(formattedMessage);
                        break;
                    case LogLevel.Warning:
                        Debug.LogWarning(formattedMessage);
                        break;
                    case LogLevel.Error:
                        Debug.LogError(formattedMessage);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                break;
            case DebugAction.Break:
                Debug.Break();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
#endif
    }

#if DEBUG
    string GetFormattedMessage(in TEvent e) {
        var typeTag = typeof(TEvent).Name;
        var valueTag = e?.ToString() ?? "null";

        return (prefix + message)
            .Replace("<type>", typeTag)
            .Replace("<value>", valueTag);
    }
#endif
}