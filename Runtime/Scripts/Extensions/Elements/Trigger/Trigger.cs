// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/Extensions                    --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using UnityEngine.UIElements;

namespace instance.id.Extensions
{
    public class Trigger : Toggle
    {
        /// <summary>
        /// USS class name of elements of this type.
        /// </summary>
        public const string UssClassName = "unity-trigger";


        /// <summary>
        /// Trigger is a non-visual toggle to use as an event callback mechanism.
        /// </summary>
        public Trigger()
        {
            var trigger = new Toggle {style = {display = DisplayStyle.None}};
            trigger.AddToClassList(UssClassName);
            Add(trigger);
        }
    }
}
