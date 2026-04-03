using System;
using InspectorEvents.Core;
using UnityEngine;

namespace InspectorEvents.Filters;

[Serializable]
public sealed class IEF_Chance : IInspectorEventFilter {
    [SerializeField, Range(0f, 1f)] float chance = 0.5f;

    public bool Evaluate() {
        return ChanceFilterUtility.Evaluate(chance);
    }
}

[Serializable]
public sealed class IEF_Chance<TEvent> : IInspectorEventFilter<TEvent> {
    [SerializeField, Range(0f, 1f)] float chance = 0.5f;

    public bool Evaluate(in TEvent evt) {
        return ChanceFilterUtility.Evaluate(chance);
    }
}

internal static class ChanceFilterUtility {
    public static bool Evaluate(float chance) {
        var normalizedChance = Mathf.Clamp01(chance);
        return normalizedChance switch {
            <= 0f => false,
            >= 1f => true,
            _ => UnityEngine.Random.value < normalizedChance
        };
    }
}