using System;
using UnityEditor;
using UnityEngine;

namespace UnHierarchy.Settings
{
    [Serializable]
    public class IconEntry
    {
        [SerializeField]
        private bool _useBuiltin = false;
        [SerializeField]
        private string _builtinName;
        [SerializeField]
        private Texture _texture;

        public IconEntry(string builtinName)
        {
            _useBuiltin = true;
            _builtinName = builtinName;
        }

        public IconEntry(Texture texture)
        {
            _useBuiltin = false;
            _texture = texture;
        }

        public GUIContent getIconContent()
        {
            return _useBuiltin ? EditorGUIUtility.IconContent(_builtinName) : new GUIContent(_texture);
        }
    }
}