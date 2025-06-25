using UnHierarchy.Editor;
using UnHierarchy.Settings;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnHierarchy.SettingsProvider
{
    public class HierarchySettingsProvider : UnityEditor.SettingsProvider
    {
        private SerializedObject _serializedSettings;

        private SerializedProperty _useCustomHierarchy;
        private SerializedProperty _selectionColor;
        private SerializedProperty _useAlternatingBackground;
        private AnimBool _alternatingBackgroundAnimBool;
        private SerializedProperty _alternatingBackgroundColor;
        private SerializedProperty _useCustomBackgroundSprite;
        private AnimBool _customBackgroundSpriteAnimBool;
        private SerializedProperty _backgroundSprite;
        private SerializedProperty _useAutomaticIcons;
        private SerializedProperty _useLayerNames;
        private SerializedProperty _useIdentLevel;
        private AnimBool _identLevelAnimBool;
        private SerializedProperty _identColor;
        private SerializedProperty _useCustomFoldout;
        private AnimBool _customFoldoutAnimBool;
        private SerializedProperty _customFoldoutClosed;
        private SerializedProperty _customFoldoutOpen;
        private SerializedProperty _customColors;
        private SerializedProperty _customIcons;

        public HierarchySettingsProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope)
        { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _serializedSettings = HierarchySettings.GetSerializedSettings();

            _useCustomHierarchy = _serializedSettings.FindProperty("_useCustomHierarchy");
            _selectionColor = _serializedSettings.FindProperty("_selectionColor");

            _useAlternatingBackground = _serializedSettings.FindProperty("_useAlternatingBackground");
            _alternatingBackgroundAnimBool = new AnimBool(_useAlternatingBackground.boolValue);
            _alternatingBackgroundAnimBool.valueChanged.AddListener(Repaint);
            _alternatingBackgroundColor = _serializedSettings.FindProperty("_alternatingBackgroundColor");

            _useCustomBackgroundSprite = _serializedSettings.FindProperty("_useCustomBackgroundSprite");
            _customBackgroundSpriteAnimBool = new AnimBool(_useCustomBackgroundSprite.boolValue);
            _customBackgroundSpriteAnimBool.valueChanged.AddListener(Repaint);
            _backgroundSprite = _serializedSettings.FindProperty("_backgroundSprite");

            _useAutomaticIcons = _serializedSettings.FindProperty("_useAutomaticIcons");

            _useLayerNames = _serializedSettings.FindProperty("_useLayerNames");

            _useIdentLevel = _serializedSettings.FindProperty("_useIdentLevel");
            _identLevelAnimBool = new AnimBool(_useIdentLevel.boolValue);
            _identLevelAnimBool.valueChanged.AddListener(Repaint);
            _identColor = _serializedSettings.FindProperty("_identColor");

            _useCustomFoldout = _serializedSettings.FindProperty("_useCustomFoldout");
            _customFoldoutAnimBool = new AnimBool(_useCustomFoldout.boolValue);
            _customFoldoutAnimBool.valueChanged.AddListener(Repaint);
            _customFoldoutClosed = _serializedSettings.FindProperty("_customFoldoutClosed");
            _customFoldoutOpen = _serializedSettings.FindProperty("_customFoldoutOpen");

            _customColors = _serializedSettings.FindProperty("_customColors");
            _customIcons = _serializedSettings.FindProperty("_customIcons");

            EditorApplication.RepaintHierarchyWindow();
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUIUtility.labelWidth = 240f;
            EditorGUILayout.PropertyField(_useCustomHierarchy);

            using (new EditorGUI.DisabledScope(_useCustomHierarchy.boolValue == false))
            {
                // Selection Color
                EditorGUILayout.BeginVertical(GUI.skin.GetStyle("GroupBox"));
                EditorGUILayout.PropertyField(_selectionColor, new GUIContent() {
                    text = "Selection Color",
                    tooltip = "The color used when selecting objects in the hierarchy."
                });
                EditorGUILayout.EndVertical();
                // Alternating Background
                EditorGUILayout.BeginVertical(GUI.skin.GetStyle("GroupBox"));
                EditorGUILayout.PropertyField(_useAlternatingBackground, new GUIContent() {
                    text = "Use Alternating Background",
                    tooltip = "When enabled, adds a background to every even line of the hierarchy."
                });
                _alternatingBackgroundAnimBool.target = _useAlternatingBackground.boolValue;
                if (EditorGUILayout.BeginFadeGroup(_alternatingBackgroundAnimBool.faded))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_alternatingBackgroundColor, new GUIContent() {
                        text = "Alternating Background Color",
                        tooltip = "The color to use of alternating backgrounds."
                    });
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFadeGroup();
                EditorGUILayout.EndVertical();
                // Custom Background
                EditorGUILayout.BeginVertical(GUI.skin.GetStyle("GroupBox"));
                EditorGUILayout.PropertyField(_useCustomBackgroundSprite, new GUIContent() {
                    text = "Use Custom Background Sprite",
                    tooltip = "When enabled, customized lines will use the configured sprite for background instead of a simple rectangle."
                });
                _customBackgroundSpriteAnimBool.target = _useCustomBackgroundSprite.boolValue;
                if (EditorGUILayout.BeginFadeGroup(_customBackgroundSpriteAnimBool.faded))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_backgroundSprite, new GUIContent() {
                        text = "Background Sprite",
                        tooltip = "The sprite to be used as a background for customized lines."
                    });
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFadeGroup();
                EditorGUILayout.EndVertical();
                // Automatic Icons
                EditorGUILayout.BeginVertical(GUI.skin.GetStyle("GroupBox"));
                EditorGUILayout.PropertyField(_useAutomaticIcons, new GUIContent() {
                    text = "Use Automatic Icons",
                    tooltip = "When enabled, replaces the default hierarchy object icons with icons of the scripts present on the object."
                });
                EditorGUILayout.EndVertical();
                // Layer Names
                EditorGUILayout.BeginVertical(GUI.skin.GetStyle("GroupBox"));
                EditorGUILayout.PropertyField(_useLayerNames, new GUIContent() {
                    text = "Show Layer Names",
                    tooltip = "When enabled, shows the layer name of the objects on the right of each hierarchy line."
                });
                EditorGUILayout.EndVertical();
                // Ident Level
                EditorGUILayout.BeginVertical(GUI.skin.GetStyle("GroupBox"));
                EditorGUILayout.PropertyField(_useIdentLevel, new GUIContent() {
                    text = "Show Indent Level",
                    tooltip = "When enabled, shows a line behind nested objects."
                });
                _identLevelAnimBool.target = _useIdentLevel.boolValue;
                if (EditorGUILayout.BeginFadeGroup(_identLevelAnimBool.faded))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_identColor, new GUIContent() {
                        text = "Indent Color",
                        tooltip = "The color of the line used to show the indentation of nested objects."
                    });
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFadeGroup();
                EditorGUILayout.EndVertical();
                // Custom Foldout
                EditorGUILayout.BeginVertical(GUI.skin.GetStyle("GroupBox"));
                EditorGUILayout.PropertyField(_useCustomFoldout, new GUIContent() {
                    text = "Use Custom Foldout",
                    tooltip = "When enabled, changes the default foldout sprite to the configured one."
                });
                _customFoldoutAnimBool.target = _useCustomFoldout.boolValue;
                if (EditorGUILayout.BeginFadeGroup(_customFoldoutAnimBool.faded))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_customFoldoutClosed, new GUIContent() {
                        text = "Custom Closed Sprite",
                        tooltip = "The custom sprite to be used when the foldout is closed."
                    });
                    EditorGUILayout.PropertyField(_customFoldoutOpen, new GUIContent() {
                        text = "Custom Open Sprite",
                        tooltip = "The custom sprite to be used when the foldout is open."
                    });
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFadeGroup();
                EditorGUILayout.EndVertical();
                // Custom Values
                EditorGUILayout.BeginVertical(GUI.skin.GetStyle("GroupBox"));
                EditorGUILayout.PropertyField(_customColors);
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(GUI.skin.GetStyle("GroupBox"));
                EditorGUILayout.PropertyField(_customIcons);
                EditorGUILayout.EndVertical();

                _serializedSettings.ApplyModifiedPropertiesWithoutUndo();
                HierarchyDrawer.Restart();
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        [SettingsProvider]
        public static UnityEditor.SettingsProvider HookHierarchySettingsProvider()
        {
            var provider = new HierarchySettingsProvider("Project/UnHierarchy");
            return provider;
        }
    }
}