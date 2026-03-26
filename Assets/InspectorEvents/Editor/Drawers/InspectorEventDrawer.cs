using InspectorEvents.Core;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace InspectorEvents.Editor.Drawers {
    public sealed class InspectorEventDrawer : OdinValueDrawer<InspectorEvent> {
        protected override void DrawPropertyLayout(GUIContent label) {
            DrawInlineFoldout(Property);
        }

        // Draw a FoldoutGroup-like boxed block with a left foldout toggle and nicified property title.
        static void DrawInlineFoldout(InspectorProperty property) {
            SirenixEditorGUI.BeginBox();
            SirenixEditorGUI.BeginBoxHeader();
            property.State.Expanded = EditorGUILayout.Foldout(property.State.Expanded, property.NiceName, true);
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
