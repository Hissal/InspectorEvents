using System;
using InspectorEvents.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace InspectorEvents.Filters;

[Serializable]
public sealed class IEF_Chance : IInspectorEventFilter {
    [FormerlySerializedAs("chance")] [SerializeField, Range(0f, 1f)] float passChance = 0.5f;

    public bool Evaluate() {
        return ChanceFilterUtility.Evaluate(passChance);
    }
}

[Serializable]
public sealed class IEF_Chance<TEvent> : IInspectorEventFilter<TEvent> {
    [FormerlySerializedAs("chance")] [SerializeField, Range(0f, 1f)] float passChance = 0.5f;

    public bool Evaluate(in TEvent evt) {
        return ChanceFilterUtility.Evaluate(passChance);
    }
}

internal static class ChanceFilterUtility {
    public static bool Evaluate(float passChance) {
        var normalizedPassChance = Mathf.Clamp01(passChance);
        return normalizedPassChance switch {
            <= 0f => false,
            >= 1f => true,
            _ => UnityEngine.Random.value < normalizedPassChance
        };
    }
}