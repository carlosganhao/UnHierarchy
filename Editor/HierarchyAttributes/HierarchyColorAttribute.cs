using System;
using UnityEngine;

namespace UnHierarchy.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HierarchyColorAttribute : Attribute
    {
        public Color hierarchyColor;

        public HierarchyColorAttribute(float r, float g, float b)
        {
            hierarchyColor = new Color(r, g, b);
        }
    }
}
