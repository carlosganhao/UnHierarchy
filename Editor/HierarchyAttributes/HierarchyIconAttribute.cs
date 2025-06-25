using System;
using UnityEditor;
using UnityEngine;

namespace UnHierarchy.Attributes
{
    public class HierarchyIconAttribute : Attribute
    {
        public Texture Icon;

        public HierarchyIconAttribute(string iconName)
        {
            Icon = EditorGUIUtility.IconContent(iconName).image;
        }
    }
}