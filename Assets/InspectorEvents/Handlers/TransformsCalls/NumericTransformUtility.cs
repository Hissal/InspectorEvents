using System;
using System.Globalization;

namespace InspectorEvents.Handlers;

public static class NumericTransformUtility {
    public static string BuildIntegerSummary<T>(
        string label,
        NumericTransformMode mode,
        T setValue,
        T addValue,
        float multiplyFactor,
        T clampMin,
        T clampMax) where T : IComparable<T> {

        return mode switch {
            NumericTransformMode.Set => $"{label}: x => {FormatValue(setValue)}",
            NumericTransformMode.Add => $"{label}: x => x + {FormatValue(addValue)}",
            NumericTransformMode.Multiply => $"{label}: x => Round(x * {FormatValue(multiplyFactor)})",
            NumericTransformMode.ClampRange => $"{label}: x => Clamp(x, {FormatValue(GetOrderedMin(clampMin, clampMax))}, {FormatValue(GetOrderedMax(clampMin, clampMax))})",
            _ => label
        };
    }

    public static string BuildSummary<T>(
        string label,
        NumericTransformMode mode,
        T setValue,
        T addValue,
        T multiplyFactor,
        T clampMin,
        T clampMax) where T : IComparable<T> {

        return mode switch {
            NumericTransformMode.Set => $"{label}: x => {FormatValue(setValue)}",
            NumericTransformMode.Add => $"{label}: x => x + {FormatValue(addValue)}",
            NumericTransformMode.Multiply => $"{label}: x => x * {FormatValue(multiplyFactor)}",
            NumericTransformMode.ClampRange => $"{label}: x => Clamp(x, {FormatValue(GetOrderedMin(clampMin, clampMax))}, {FormatValue(GetOrderedMax(clampMin, clampMax))})",
            _ => label
        };
    }

    public static string FormatValue<T>(T value) {
        return value is IFormattable formattable
            ? formattable.ToString(null, CultureInfo.InvariantCulture)
            : value?.ToString() ?? string.Empty;
    }

    public static int MultiplyAndRound(int value, float factor) {
        return RoundToInt(value * factor);
    }

    public static long MultiplyAndRound(long value, float factor) {
        return RoundToLong(value * (double)factor);
    }

    public static T GetOrderedMin<T>(T a, T b) where T : IComparable<T> {
        return a.CompareTo(b) <= 0 ? a : b;
    }

    public static T GetOrderedMax<T>(T a, T b) where T : IComparable<T> {
        return a.CompareTo(b) >= 0 ? a : b;
    }

    public static long Clamp(long value, long min, long max) {
        var orderedMin = GetOrderedMin(min, max);
        var orderedMax = GetOrderedMax(min, max);

        if (value < orderedMin) {
            return orderedMin;
        }

        return value > orderedMax ? orderedMax : value;
    }

    public static double Clamp(double value, double min, double max) {
        var orderedMin = GetOrderedMin(min, max);
        var orderedMax = GetOrderedMax(min, max);

        if (value < orderedMin) {
            return orderedMin;
        }

        return value > orderedMax ? orderedMax : value;
    }

    static int RoundToInt(double value) {
        var rounded = RoundInteger(value);
        if (rounded >= int.MaxValue) {
            return int.MaxValue;
        }

        if (rounded <= int.MinValue) {
            return int.MinValue;
        }

        return (int)rounded;
    }

    static long RoundToLong(double value) {
        var rounded = RoundInteger(value);
        if (rounded >= long.MaxValue) {
            return long.MaxValue;
        }

        if (rounded <= long.MinValue) {
            return long.MinValue;
        }

        return (long)rounded;
    }

    static double RoundInteger(double value) {
        if (double.IsNaN(value)) {
            return 0d;
        }

        return Math.Round(value, MidpointRounding.AwayFromZero);
    }
}
