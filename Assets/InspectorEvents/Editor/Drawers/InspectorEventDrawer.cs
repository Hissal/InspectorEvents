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
        const float c_maxWidth = 480f;
        const float c_maxHeight = 560f;
        const float c_minWidth = 320f;
        const float c_minHeight = 180f;
        const float c_contentPadding = 24f;
        const float c_resizeApplyThreshold = 6f;
        const float c_heightJitterTolerance = 2f;
        const int c_shrinkStableRepaints = 6;

        readonly object _target;
        readonly string _title;

        PropertyTree _tree;
        Vector2 _scroll;
        Vector2 _windowSize = new(c_maxWidth, c_maxHeight);
        float _lastMeasuredHeight = c_maxHeight;
        int _stableShrinkRepaintCount;

        ValueConstructorPopupContent(object target, string title) {
            _target = target;
            _title = title;
        }

        public static void Show(Rect triggerRect, object target, string title) {
            PopupWindow.Show(triggerRect, new ValueConstructorPopupContent(target, title));
        }

        public override Vector2 GetWindowSize() {
            return _windowSize;
        }

        public override void OnOpen() {
            DisposeTree();
            _windowSize = new Vector2(c_maxWidth, c_maxHeight);
            _lastMeasuredHeight = c_maxHeight;
            _stableShrinkRepaintCount = 0;
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
            EditorGUILayout.BeginVertical();
            _tree.Draw(false);
            EditorGUILayout.EndVertical();
            var contentRect = GUILayoutUtility.GetLastRect();
            EditorGUILayout.EndScrollView();

            if (Event.current.type != EventType.Repaint) {
                return;
            }

            // Measure drawn content height rather than the scroll viewport size.
            var targetHeight = Mathf.Clamp(contentRect.height + c_contentPadding, c_minHeight, c_maxHeight);
            UpdateWindowSizeForContent(targetHeight);
        }

        void UpdateWindowSizeForContent(float targetHeight) {
            if (editorWindow == null) {
                return;
            }

            // Expand quickly so newly revealed content is never clipped.
            if (targetHeight > _windowSize.y + c_resizeApplyThreshold) {
                _stableShrinkRepaintCount = 0;
                _lastMeasuredHeight = targetHeight;
                ApplyWindowSize(targetHeight);
                return;
            }

            // Shrink only after a stable sequence to avoid oscillating resize flicker.
            if (targetHeight < _windowSize.y - c_resizeApplyThreshold) {
                if (Mathf.Abs(targetHeight - _lastMeasuredHeight) <= c_heightJitterTolerance) {
                    _stableShrinkRepaintCount++;
                }
                else {
                    _stableShrinkRepaintCount = 0;
                }

                _lastMeasuredHeight = targetHeight;

                if (_stableShrinkRepaintCount >= c_shrinkStableRepaints) {
                    _stableShrinkRepaintCount = 0;
                    ApplyWindowSize(targetHeight);
                }

                return;
            }

            _stableShrinkRepaintCount = 0;
            _lastMeasuredHeight = targetHeight;
        }

        void ApplyWindowSize(float targetHeight) {
            if (editorWindow == null) {
                return;
            }

            var clampedWidth = Mathf.Clamp(c_maxWidth, c_minWidth, c_maxWidth);
            if (Mathf.Abs(_windowSize.y - targetHeight) < 2f && Mathf.Abs(_windowSize.x - clampedWidth) < 0.5f) {
                return;
            }

            _windowSize = new Vector2(clampedWidth, targetHeight);
            var position = editorWindow.position;
            editorWindow.position = new Rect(position.x, position.y, _windowSize.x, _windowSize.y);
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
