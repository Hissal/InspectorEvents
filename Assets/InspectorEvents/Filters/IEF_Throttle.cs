using System;
using InspectorEvents.Core;
using UnityEngine;

namespace InspectorEvents.Filters;

public enum ThrottleTimeSource {
    Scaled,
    Unscaled,
    Realtime
}

[Serializable]
public sealed class IEF_Throttle : IInspectorEventFilter {
    [SerializeField, Min(0f)] float intervalSeconds = 0.1f;
    [SerializeField] bool allowFirstPass = true;
    [SerializeField] ThrottleTimeSource timeSource = ThrottleTimeSource.Realtime;

    bool initialized;
    double lastPassTime;

    public bool Evaluate() {
        if (intervalSeconds <= 0f) {
            return true;
        }

        var now = GetCurrentTime();
        if (!initialized) {
            initialized = true;
            lastPassTime = now;
            return allowFirstPass;
        }

        // If time moved backwards (e.g. play mode reset), rebase safely.
        if (now < lastPassTime) {
            lastPassTime = now;
            return false;
        }

        if (now - lastPassTime < intervalSeconds) {
            return false;
        }

        lastPassTime = now;
        return true;
    }

    double GetCurrentTime() {
        return timeSource switch {
            ThrottleTimeSource.Scaled => Time.timeAsDouble,
            ThrottleTimeSource.Unscaled => Time.unscaledTimeAsDouble,
            ThrottleTimeSource.Realtime => Time.realtimeSinceStartupAsDouble,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

[Serializable]
public sealed class IEF_Throttle<TEvent> : IInspectorEventFilter<TEvent> {
    [SerializeField, Min(0f)] float intervalSeconds = 0.1f;
    [SerializeField] bool allowFirstPass = true;
    [SerializeField] ThrottleTimeSource timeSource = ThrottleTimeSource.Realtime;

    bool initialized;
    double lastPassTime;

    public bool Evaluate(in TEvent evt) {
        if (intervalSeconds <= 0f) {
            return true;
        }

        var now = GetCurrentTime();
        if (!initialized) {
            initialized = true;
            lastPassTime = now;
            return allowFirstPass;
        }

        // If time moved backwards (e.g. play mode reset), rebase safely.
        if (now < lastPassTime) {
            lastPassTime = now;
            return false;
        }

        if (now - lastPassTime < intervalSeconds) {
            return false;
        }

        lastPassTime = now;
        return true;
    }

    double GetCurrentTime() {
        return timeSource switch {
            ThrottleTimeSource.Scaled => Time.timeAsDouble,
            ThrottleTimeSource.Unscaled => Time.unscaledTimeAsDouble,
            ThrottleTimeSource.Realtime => Time.realtimeSinceStartupAsDouble,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}