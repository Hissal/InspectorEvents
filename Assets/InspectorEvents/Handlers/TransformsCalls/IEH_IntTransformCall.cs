using System;
using InspectorEvents.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InspectorEvents.Handlers;

[Serializable]
public sealed class IEH_IntTransformCall : IInspectorEventHandler<int> {
#if UNITY_EDITOR
    [ShowInInspector, ReadOnly, PropertyOrder(-1), LabelText("Summary")]
    string Summary => GetSummary();
#endif

    [SerializeField]
    NumericTransformMode mode = NumericTransformMode.Add;

    [SerializeField, ShowIf(nameof(IsSetMode)), LabelText("Value")]
    int setValue;
    [SerializeField, ShowIf(nameof(IsAddMode)), LabelText("Operand")]
    int addValue;
    [SerializeField, ShowIf(nameof(IsMultiplyMode)), LabelText("Factor")]
    float multiplyFactor = 1f;
    [SerializeField, ShowIf(nameof(IsClampRangeMode)), LabelText("Min"), OnValueChanged(nameof(NormalizeClampBounds))]
    int clampMin;
    [SerializeField, ShowIf(nameof(IsClampRangeMode)), LabelText("Max"), OnValueChanged(nameof(NormalizeClampBounds))]
    int clampMax;

    [PropertySpace]
    [SerializeReference, LabelText("Handlers")]
    // ReSharper disable once UseArrayEmptyMethod disabled due to SerializeReferece actually serializing the empty array instance
    IInspectorEventHandler<int>[] handlers = new IInspectorEventHandler<int>[0];

    bool IsSetMode => mode == NumericTransformMode.Set;
    bool IsAddMode => mode == NumericTransformMode.Add;
    bool IsMultiplyMode => mode == NumericTransformMode.Multiply;
    bool IsClampRangeMode => mode == NumericTransformMode.ClampRange;

    public void Handle(in int value) {
        var transformed = Transform(value);
        foreach (var handler in handlers) {
            handler.Handle(transformed);
        }
    }

#if UNITY_EDITOR
    public string GetSummary() {
        return NumericTransformUtility.BuildIntegerSummary(
            "Int Transform",
            mode,
            setValue,
            addValue,
            multiplyFactor,
            clampMin,
            clampMax
        );
    }
#endif

    int Transform(int value) {
        return mode switch {
            NumericTransformMode.Set => setValue,
            NumericTransformMode.Add => unchecked(value + addValue),
            NumericTransformMode.Multiply => NumericTransformUtility.MultiplyAndRound(value, multiplyFactor),
            NumericTransformMode.ClampRange => Mathf.Clamp(
                value,
                NumericTransformUtility.GetOrderedMin(clampMin, clampMax),
                NumericTransformUtility.GetOrderedMax(clampMin, clampMax)
            ),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    void NormalizeClampBounds() {
        if (clampMin > clampMax) {
            (clampMin, clampMax) = (clampMax, clampMin);
        }
    }
}
