using System;
using System.Linq;
using InspectorEvents.Core;
using InspectorEvents.Handlers;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace InspectorEvents.Editor.Drawers {
    public sealed class NumericTransformCallDrawer : OdinValueDrawer<IEH_NumericTransformCall> {
        protected override void DrawPropertyLayout(GUIContent label) {
            var handledType = GetHandledType(Property);
            if (!IsSupportedNumericType(handledType)) {
                CallNextDrawer(label);
                return;
            }

            var modeProperty = FindChild(Property, "mode");
            if (modeProperty == null) {
                CallNextDrawer(label);
                return;
            }

            EditorGUILayout.LabelField("Summary", ValueEntry.SmartValue.GetSummary(handledType!));
            modeProperty.Draw();

            var mode = (NumericTransformMode)modeProperty.ValueEntry.WeakSmartValue;
            if (handledType == typeof(int)) {
                DrawModeSpecificFields(mode, "intSetValue", "intAddValue", "intMultiplyFactor", "intClampMin", "intClampMax", "intHandlers");
                return;
            }

            if (handledType == typeof(long)) {
                DrawModeSpecificFields(mode, "longSetValue", "longAddValue", "longMultiplyFactor", "longClampMin", "longClampMax", "longHandlers");
                return;
            }

            if (handledType == typeof(float)) {
                DrawModeSpecificFields(mode, "floatSetValue", "floatAddValue", "floatMultiplyFactor", "floatClampMin", "floatClampMax", "floatHandlers");
                return;
            }

            if (handledType == typeof(double)) {
                DrawModeSpecificFields(mode, "doubleSetValue", "doubleAddValue", "doubleMultiplyFactor", "doubleClampMin", "doubleClampMax", "doubleHandlers");
            }
        }

        void DrawModeSpecificFields(
            NumericTransformMode mode,
            string setValueFieldName,
            string addValueFieldName,
            string multiplyFactorFieldName,
            string clampMinFieldName,
            string clampMaxFieldName,
            string handlersFieldName) {

            switch (mode) {
                case NumericTransformMode.Set:
                    DrawChild(setValueFieldName);
                    break;
                case NumericTransformMode.Add:
                    DrawChild(addValueFieldName);
                    break;
                case NumericTransformMode.Multiply:
                    DrawChild(multiplyFactorFieldName);
                    break;
                case NumericTransformMode.ClampRange:
                    DrawChild(clampMinFieldName);
                    DrawChild(clampMaxFieldName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

            EditorGUILayout.Space();
            DrawChild(handlersFieldName);
        }

        void DrawChild(string childName) {
            FindChild(Property, childName)?.Draw();
        }

        static InspectorProperty? FindChild(InspectorProperty property, string childName) {
            return property.Children.FirstOrDefault(child => child.Name == childName);
        }

        static Type? GetHandledType(InspectorProperty property) {
            for (var current = property; current != null; current = current.Parent) {
                var baseValueType = current.ValueEntry?.BaseValueType;
                if (baseValueType == null || !baseValueType.IsGenericType) {
                    continue;
                }

                if (baseValueType.GetGenericTypeDefinition() != typeof(IInspectorEventHandler<>)) {
                    continue;
                }

                return baseValueType.GetGenericArguments()[0];
            }

            return null;
        }

        static bool IsSupportedNumericType(Type? handledType) {
            return handledType == typeof(int)
                   || handledType == typeof(long)
                   || handledType == typeof(float)
                   || handledType == typeof(double);
        }
    }
}