using System;
using InspectorEvents.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InspectorEvents.Handlers;

[Serializable]
public sealed class IEH_DoubleTransformCall : IInspectorEventHandler<double> {
#if UNITY_EDITOR
    [ShowInInspector, ReadOnly, PropertyOrder(-1), LabelText("Summary")]
    string Summary => GetSummary();
#endif

    [SerializeField]
    NumericTransformMode mode = NumericTransformMode.Add;

    [SerializeField, ShowIf(nameof(IsSetMode)), LabelText("Value")]
    double setValue;
    [SerializeField, ShowIf(nameof(IsAddMode)), LabelText("Operand")]
    double addValue;
    [SerializeField, ShowIf(nameof(IsMultiplyMode)), LabelText("Factor")]
    double multiplyFactor = 1d;
    [SerializeField, ShowIf(nameof(IsClampRangeMode)), LabelText("Min"), OnValueChanged(nameof(NormalizeClampBounds))]
    double clampMin;
    [SerializeField, ShowIf(nameof(IsClampRangeMode)), LabelText("Max"), OnValueChanged(nameof(NormalizeClampBounds))]
    double clampMax;

    [PropertySpace]
    [SerializeReference, LabelText("Handlers")]
    // ReSharper disable once UseArrayEmptyMethod disabled due to SerializeReferece actually serializing the empty array instance
    IInspectorEventHandler<double>[] handlers = new IInspectorEventHandler<double>[0];

    bool IsSetMode => mode == NumericTransformMode.Set;
    bool IsAddMode => mode == NumericTransformMode.Add;
    bool IsMultiplyMode => mode == NumericTransformMode.Multiply;
    bool IsClampRangeMode => mode == NumericTransformMode.ClampRange;

    public void Handle(in double value) {
        var transformed = Transform(value);
        foreach (var handler in handlers) {
            handler.Handle(transformed);
        }
    }

#if UNITY_EDITOR
    public string GetSummary() {
        return NumericTransformUtility.BuildSummary(
            "Double Transform",
            mode,
            setValue,
            addValue,
            multiplyFactor,
            clampMin,
            clampMax
        );
    }
#endif

    double Transform(double value) {
        return mode switch {
            NumericTransformMode.Set => setValue,
            NumericTransformMode.Add => value + addValue,
            NumericTransformMode.Multiply => value * multiplyFactor,
            NumericTransformMode.ClampRange => NumericTransformUtility.Clamp(value, clampMin, clampMax),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    void NormalizeClampBounds() {
        if (clampMin > clampMax) {
            (clampMin, clampMax) = (clampMax, clampMin);
        }
    }
}
