using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace instance.id.Extensions.Editors
{
    [CustomEditor(typeof(Object), true, isFallback = true)]
    [CanEditMultipleObjects]
    public class DefaultUIElementsEditor : Editor
    {
        private VisualElement visualElement;
        public VisualElement customElements;

        public override VisualElement CreateInspectorGUI()
        {
            visualElement = new VisualElement();
            UIElementsEditorHelper.FillDefaultInspector(visualElement, serializedObject, false);
            visualElement.Add(customElements);
            return visualElement;
        }
    }
}
