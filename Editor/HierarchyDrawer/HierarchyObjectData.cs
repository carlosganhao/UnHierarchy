using UnityEditor;
using UnityEngine;

namespace UnHierarchy.Editor
{
    public class HierarchyObjectData
    {
        public int InstanceId;
        public GlobalObjectId GlobalObjectId;
        public GameObject GameObject;
        public Color BackgroundColor;
        public GUIContent Content;

        public override string ToString()
        {
            return $"[{InstanceId}, {GameObject}, {BackgroundColor}, {Content}]";
        }
    }
}