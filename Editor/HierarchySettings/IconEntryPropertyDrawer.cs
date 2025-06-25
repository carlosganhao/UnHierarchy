using UnityEditor;
using UnityEngine;

namespace UnHierarchy.Settings
{
    [CustomPropertyDrawer(typeof(IconEntry), true)]
    public class IconEntryPropertyDrawer : PropertyDrawer
    {
        private SerializedProperty _useBuiltin;
        private SerializedProperty _builtinName;
        private SerializedProperty _texture;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _useBuiltin = property.FindPropertyRelative("_useBuiltin");
            _builtinName = property.FindPropertyRelative("_builtinName");
            _texture = property.FindPropertyRelative("_texture");

            var fieldPosition = EditorGUI.PrefixLabel(position, label);
            var popupPosition = new Rect(fieldPosition);
            popupPosition.yMin += Styles.PopupStyle.margin.top;
            popupPosition.width = Styles.PopupStyle.fixedWidth + Styles.PopupStyle.margin.right + Styles.PopupStyle.margin.left;
            popupPosition.height = Styles.PopupStyle.fixedHeight + Styles.PopupStyle.margin.top;
            var valuePosition = new Rect(fieldPosition);
            valuePosition.x += popupPosition.width + 2;
            valuePosition.width -= popupPosition.width + 2;
            _useBuiltin.boolValue = EditorGUI.Popup(popupPosition, _useBuiltin.boolValue ? 0 : 1, new string[] { "Use Builtin Icon", "Use Custom Icon" }, Styles.PopupStyle) == 0;
            if (_useBuiltin.boolValue)
            {
                EditorGUI.PropertyField(valuePosition, _builtinName, GUIContent.none);
            }
            else
            {
                EditorGUI.PropertyField(valuePosition, _texture, GUIContent.none);
            }
            property.serializedObject.ApplyModifiedProperties();
        }

        static class Styles
        {
            static Styles()
            {
                PopupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"))
                {
                    imagePosition = ImagePosition.ImageOnly,
                };
                PopupStyle.margin.left = PopupStyle.margin.right;
            }

            public static GUIStyle PopupStyle { get; set; }
        }
    }
}