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
        const float c_invokeButtonWidth = 54f;
        const float c_configureButtonWidth = 76f;
        const float c_headerButtonGap = 4f;

        protected override void DrawPropertyLayout(GUIContent label) {
            DrawInlineFoldout(Property, ValueEntry.SmartValue);
        }

        static void DrawInlineFoldout(InspectorProperty property, InspectorEvent<TEvent> inspectorEvent) {
            SirenixEditorGUI.BeginBox();
            SirenixEditorGUI.BeginBoxHeader();

            var headerRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            var reservedWidth = c_invokeButtonWidth + c_configureButtonWidth + (c_headerButtonGap * 2f);
            var foldoutWidth = Mathf.Max(0f, headerRect.width - reservedWidth);
            var foldoutRect = new Rect(headerRect.x, headerRect.y, foldoutWidth, headerRect.height);

            var buttonHeight = Mathf.Min(18f, Mathf.Max(0f, headerRect.height - 2f));
            var buttonY = headerRect.y + (headerRect.height - buttonHeight) * 0.5f;
            var configureX = headerRect.xMax - c_configureButtonWidth;
            var invokeX = configureX - c_headerButtonGap - c_invokeButtonWidth;
            var invokeRect = new Rect(invokeX, buttonY, c_invokeButtonWidth, buttonHeight);
            var configureRect = new Rect(configureX, buttonY, c_configureButtonWidth, buttonHeight);

            property.State.Expanded = EditorGUI.Foldout(foldoutRect, property.State.Expanded, new GUIContent($"{property.NiceName} ({typeof(TEvent).Name})"), true);

            if (GUI.Button(invokeRect, "Invoke", EditorStyles.miniButton)) {
                if (!inspectorEvent.TryInvokeConfigured(out var error)) {
                    Debug.LogWarning($"Failed invoking '{property.NiceName}' with configured value. {error}");
                }
            }

            if (GUI.Button(configureRect, "Configure", EditorStyles.miniButton)) {
                var constructorObject = inspectorEvent.GetOrCreateValueConstructorObject();
                ValueConstructorPopupContent.Show(configureRect, constructorObject, $"Configure {property.NiceName}");
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

    sealed class ValueConstructorPopupContent : PopupWindowContent {
        readonly object _target;
        readonly string _title;

        PropertyTree _tree;
        Vector2 _scroll;

        ValueConstructorPopupContent(object target, string title) {
            _target = target;
            _title = title;
        }

        public static void Show(Rect triggerRect, object target, string title) {
            PopupWindow.Show(triggerRect, new ValueConstructorPopupContent(target, title));
        }

        public override Vector2 GetWindowSize() {
            return new Vector2(480f, 560f);
        }

        public override void OnOpen() {
            DisposeTree();
            _tree = PropertyTree.Create(_target);
            editorWindow.titleContent = new GUIContent(_title);
        }

        public override void OnClose() {
            DisposeTree();
        }

        public override void OnGUI(Rect rect) {
            if (_tree == null) {
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            _tree.Draw(false);
            EditorGUILayout.EndScrollView();
        }

        void DisposeTree() {
            if (_tree == null) {
                return;
            }

            _tree.Dispose();
            _tree = null;
        }
    }
}
