﻿namespace instance.id.Extensions
{
    public class DictionaryAttribute : PropertyTraitAttribute
    {
        public const string Always = "";
        public const string Never = null;

        public string AllowAdd = Always;
        public string AllowRemove = Always;

        public bool AllowReorder = true;
        public bool IsCollapsable = true;

        public string AddPlaceholder = null;
        public string EmptyLabel = null;

        public string AddCallback = null;
        public string RemoveCallback = null;
        public string ReorderCallback = null;
        public string ChangeCallback = null;

        public DictionaryAttribute() : base(ContainerPhase, 0)
        {
        }
    }
	
    public class FrameAttribute : PropertyTraitAttribute
    {
        public bool IsCollapsable = true;

        public FrameAttribute() : base(ControlPhase, 0)
        {
        }
    }
}
