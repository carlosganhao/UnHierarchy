using System;
using UnityEngine;

namespace UnHierarchy.Settings
{
    [Serializable]
    public class CustomizationData
    {
        [SerializeField]
        public Color CustomBackgroundColor;
        [SerializeField]
        public GUIContent CustomIconContent;
    }
}