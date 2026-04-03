using System;
using InspectorEvents.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InspectorEvents.Filters;

public enum CallCountMode {
    EveryNCalls,
    PassAfterXCalls,
    PassUntilXCalls,
    PassBlockCycle
}

[Serializable]
public sealed class IEF_CallCount : IInspectorEventFilter {
    [SerializeField] CallCountMode mode = CallCountMode.EveryNCalls;
    [SerializeField, Min(1), ShowIf(nameof(mode), CallCountMode.EveryNCalls)] int everyNCalls = 2;
    [SerializeField, Min(0), ShowIf(nameof(UsesThresholdMode))] int thresholdCalls;
    [SerializeField, Min(0), ShowIf(nameof(mode), CallCountMode.PassBlockCycle)] int passWindowSize = 5;
    [SerializeField, Min(0), ShowIf(nameof(mode), CallCountMode.PassBlockCycle)] int blockWindowSize = 5;
    [SerializeField, Min(0)] int initialCount;

    bool initialized;
    int callsSeen;

    bool UsesThresholdMode => mode is CallCountMode.PassAfterXCalls or CallCountMode.PassUntilXCalls;

    public bool Evaluate() {
        return CallCountFilterUtility.Evaluate(
            ref initialized,
            ref callsSeen,
            initialCount,
            mode,
            everyNCalls,
            thresholdCalls,
            passWindowSize,
            blockWindowSize
        );
    }
}

[Serializable]
public sealed class IEF_CallCount<TEvent> : IInspectorEventFilter<TEvent> {
    [SerializeField] CallCountMode mode = CallCountMode.EveryNCalls;
    [SerializeField, Min(1), ShowIf(nameof(mode), CallCountMode.EveryNCalls)] int everyNCalls = 2;
    [SerializeField, Min(0), ShowIf(nameof(UsesThresholdMode))] int thresholdCalls;
    [SerializeField, Min(0), ShowIf(nameof(mode), CallCountMode.PassBlockCycle)] int passWindowSize = 5;
    [SerializeField, Min(0), ShowIf(nameof(mode), CallCountMode.PassBlockCycle)] int blockWindowSize = 5;
    [SerializeField, Min(0)] int initialCount;

    bool initialized;
    int callsSeen;

    bool UsesThresholdMode => mode is CallCountMode.PassAfterXCalls or CallCountMode.PassUntilXCalls;

    public bool Evaluate(in TEvent evt) {
        return CallCountFilterUtility.Evaluate(
            ref initialized,
            ref callsSeen,
            initialCount,
            mode,
            everyNCalls,
            thresholdCalls,
            passWindowSize,
            blockWindowSize
        );
    }
}

internal static class CallCountFilterUtility {
    public static bool Evaluate(
        ref bool initialized,
        ref int callsSeen,
        int initialCount,
        CallCountMode mode,
        int everyNCalls,
        int thresholdCalls,
        int passWindowSize,
        int blockWindowSize
    ) {
        if (!initialized) {
            initialized = true;
            callsSeen = Mathf.Max(0, initialCount);
        }

        AdvanceCounter(ref callsSeen, mode, everyNCalls, passWindowSize, blockWindowSize);

        return mode switch {
            CallCountMode.EveryNCalls => EvaluateEveryN(callsSeen, everyNCalls),
            CallCountMode.PassAfterXCalls => callsSeen > Mathf.Max(0, thresholdCalls),
            CallCountMode.PassUntilXCalls => callsSeen <= Mathf.Max(0, thresholdCalls),
            CallCountMode.PassBlockCycle => EvaluatePassBlockCycle(callsSeen, passWindowSize, blockWindowSize),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    static bool EvaluateEveryN(int callsSeen, int everyNCalls) {
        var n = Mathf.Max(1, everyNCalls);
        return callsSeen % n == 0;
    }

    static bool EvaluatePassBlockCycle(int callsSeen, int passWindowSize, int blockWindowSize) {
        var pass = Mathf.Max(0, passWindowSize);
        var block = Mathf.Max(0, blockWindowSize);
        var cycle = pass + block;
        if (cycle <= 0 || pass <= 0) {
            return false;
        }

        var cyclePosition = (callsSeen - 1) % cycle;
        return cyclePosition < pass;
    }

    static void AdvanceCounter(ref int callsSeen, CallCountMode mode, int everyNCalls, int passWindowSize, int blockWindowSize) {
        if (callsSeen == int.MaxValue) {
            switch (mode) {
                case CallCountMode.EveryNCalls:
                    callsSeen %= Mathf.Max(1, everyNCalls);
                    break;
                case CallCountMode.PassBlockCycle:
                    var cycle = Mathf.Max(0, passWindowSize) + Mathf.Max(0, blockWindowSize);
                    callsSeen = cycle > 0 ? callsSeen % cycle : 0;
                    break;
                case CallCountMode.PassAfterXCalls:
                case CallCountMode.PassUntilXCalls:
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        callsSeen++;
    }
}