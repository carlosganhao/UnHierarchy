namespace UnHierarchy.Editor
{
    public class VisibilityObjectData
    {
        public int InstanceId;
        public bool Visible;
        public bool Pickable;
        public bool AllDescendantsVisible;
        public bool AllDescendantsPickable;
        public bool NoDescendantsVisible;
        public bool NoDescendantsPickable;

        public bool MixedDescendantVisibility => !AllDescendantsVisible && !NoDescendantsVisible;
        public bool MixedDescendantPickability => !AllDescendantsPickable && !NoDescendantsPickable;
        public bool NoIcon => Visible && Pickable && AllDescendantsVisible && AllDescendantsPickable;
    }
}