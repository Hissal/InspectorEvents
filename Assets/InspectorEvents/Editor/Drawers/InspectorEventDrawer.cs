using InspectorEvents.Core;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace InspectorEvents.Editor.Drawers {
    public sealed class InspectorEventDrawer : OdinValueDrawer<InspectorEvent> {
        const float c_headerButtonWidth = 54f;
        const float c_headerButtonGap = 6f;

        protected override void DrawPropertyLayout(GUIContent label) {
            DrawInlineFoldout(Property, ValueEntry.SmartValue);
        }

        // Draw a FoldoutGroup-like boxed block with a left foldout toggle and nicified property title.
        static void DrawInlineFoldout(InspectorProperty property, InspectorEvent inspectorEvent) {
            SirenixEditorGUI.BeginBox();
            SirenixEditorGUI.BeginBoxHeader();

            var headerRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            var foldoutWidth = Mathf.Max(0f, headerRect.width - c_headerButtonWidth - c_headerButtonGap);
            var foldoutRect = new Rect(headerRect.x, headerRect.y, foldoutWidth, headerRect.height);

            var buttonHeight = Mathf.Min(18f, Mathf.Max(0f, headerRect.height - 2f));
            var buttonY = headerRect.y + (headerRect.height - buttonHeight) * 0.5f;
            var buttonRect = new Rect(headerRect.xMax - c_headerButtonWidth, buttonY, c_headerButtonWidth, buttonHeight);

            property.State.Expanded = EditorGUI.Foldout(foldoutRect, property.State.Expanded, property.NiceName, true);

            if (GUI.Button(buttonRect, "Invoke", EditorStyles.miniButton)) {
                inspectorEvent?.Invoke();
            }

            SirenixEditorGUI.EndBoxHeader();

            if (property.State.Expanded) {
                EditorGUI.indentLevel++;

                foreach (var propertyChild in property.Children) {
                    propertyChild.Draw();
                }

                EditorGUI.indentLevel--;
            }

            SirenixEditorGUI.EndBox();
        }
    }

    public sealed class InspectorEventDrawer<TEvent> : OdinValueDrawer<InspectorEvent<TEvent>> {
        protected override void DrawPropertyLayout(GUIContent label) {
            DrawInlineFoldout(Property);
        }

        static void DrawInlineFoldout(InspectorProperty property) {
            SirenixEditorGUI.BeginBox();
            SirenixEditorGUI.BeginBoxHeader();
            property.State.Expanded = EditorGUILayout.Foldout(property.State.Expanded, $"{property.NiceName} ({typeof(TEvent).Name})", true);
            SirenixEditorGUI.EndBoxHeader();

            if (property.State.Expanded) {
                EditorGUI.indentLevel++;

                foreach (var propertyChild in property.Children) {
                    propertyChild.Draw();
                }

                EditorGUI.indentLevel--;
            }

            SirenixEditorGUI.EndBox();
        }
    }
}
