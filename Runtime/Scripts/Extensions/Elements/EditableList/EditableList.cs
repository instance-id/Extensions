// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/Extensions                    --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;

namespace instance.id.Extensions
{
    public class EditableList : BindableElement
    {
        public static readonly string UssClassName = "editable-list";
        public static readonly string HeaderUssClassName = UssClassName + "__header";
        public static readonly string HeaderTextUssClassName = HeaderUssClassName + "__text";
        public static readonly string HeaderAddButtonUssClassName = HeaderUssClassName + "__add-button";
        public static readonly string ItemUssClassName = UssClassName + "__item";
        public static readonly string ItemDragButtonUssClassName = ItemUssClassName + "__drag-button";
        public static readonly string ItemConentUssClassName = ItemUssClassName + "__content";
        public static readonly string ItemRemoveButtonUssClassName = ItemUssClassName + "__remove-button";

        public static readonly string ImageButtonUssClassName = "editable-image-button";

        public event Action<EditableList, int> OnAddCallback;
        public event Action<EditableList, int> OnRemoveCallback;

        public SerializedProperty ArrayProperty
        {
            get => m_ArrayProperty;
        }

        public bool CanAdd
        {
            get => m_AddButton.enabledSelf;
            set => m_AddButton.SetEnabled(value);
        }

        public bool CanRemove
        {
            get => m_CanRemove;
            set
            {
                m_CanRemove = value;
                foreach (var item in m_Items)
                    item.RemoveButton.SetEnabled(m_CanRemove);
            }
        }

        public bool CanDrag { get; set; } = true;

        private SerializedProperty m_ArrayProperty;
        private VisualElement m_ListContent;
        private Button m_AddButton;
        private Func<EditableList, int, VisualElement> m_MakeItemDelegate;
        private bool m_CanRemove = true;
        private string m_CannotAddReason;
        private List<ItemData> m_Items = new List<ItemData>();

        private struct ItemData
        {
            public VisualElement Container;
            public Button RemoveButton;
        }

        public EditableList(SerializedProperty in_Array, Func<EditableList, int, VisualElement> in_MakeItem, bool in_CanDrag = true) : this(in_Array.displayName, in_Array,
            in_MakeItem, in_CanDrag)
        {
        }

        public EditableList(string in_Name, SerializedProperty in_Array, Func<EditableList, int, VisualElement> in_MakeItem, bool in_CanDrag = true)
        {
            if (!in_Array.isArray)
            {
                Debug.LogError("Supplied property is not an array");
                return;
            }

            CanDrag = in_CanDrag;

            m_ArrayProperty = in_Array;
            m_MakeItemDelegate = in_MakeItem;

            Texture2D addIcon = (Texture2D) EditorGUIUtility.TrIconContent("Toolbar Plus").image;

            var header = new VisualElement();
            header.AddToClassList(HeaderUssClassName);

            var title = new Label(in_Name);
            title.AddToClassList(HeaderTextUssClassName);

            m_AddButton = new Button(HandleOnItemAdded);
            m_AddButton.AddToClassList(HeaderAddButtonUssClassName);
            m_AddButton.AddToClassList(ImageButtonUssClassName);
            m_AddButton.style.backgroundImage = new StyleBackground(addIcon);

            header.Add(title);
            header.Add(m_AddButton);
            this.Add(header);

            m_ListContent = new VisualElement();

            Refresh();

            this.Add(m_ListContent);
            this.AddToClassList(UssClassName);
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(typeof(EditableList).GetMonoScriptPathFor() + "/EditableList.uss");
            this.styleSheets.Add(styleSheet);
        }

        public void Refresh()
        {
            m_ListContent.Clear();
            m_Items.Clear();

            if (m_ArrayProperty.arraySize == 0)
            {
                m_ListContent.Add(new Label("No Entries"));
            }

            Texture2D removeIcon = (Texture2D) EditorGUIUtility.TrIconContent("Toolbar Minus").image;
            Texture2D dragIcon = ((GUIStyle) "RL DragHandle").normal.background;

            for (int i = 0; i < m_ArrayProperty.arraySize; i++)
            {
                var item = new VisualElement();
                item.AddToClassList(ItemUssClassName);

                var indexElem = new TextField();
                indexElem.value = i.ToString();
                indexElem.SetEnabled(false);

                var content = m_MakeItemDelegate(this, i);
                content.AddToClassList(ItemConentUssClassName);

                var x = i;

                var removeButton = new Button(() => { HandleOnItemRemoved(x); });
                removeButton.AddToClassList(ItemRemoveButtonUssClassName);
                removeButton.AddToClassList(ImageButtonUssClassName);
                removeButton.style.backgroundImage = new StyleBackground(removeIcon);

                if (CanDrag)
                {
                    var dragButton = new Button();
                    dragButton.AddToClassList(ItemDragButtonUssClassName);
                    dragButton.AddToClassList(ImageButtonUssClassName);
                    dragButton.style.backgroundImage = new StyleBackground(dragIcon);
                    item.Add(dragButton);
                }

                item.Add(indexElem);
                item.Add(content);
                item.Add(removeButton);
                removeButton.SetEnabled(m_CanRemove);
                m_ListContent.Add(item);

                var itemData = new ItemData()
                {
                    Container = item,
                    RemoveButton = removeButton,
                };
                m_Items.Add(itemData);
            }
        }

        private void HandleOnItemAdded()
        {
            m_ArrayProperty.InsertArrayElementAtIndex(m_ArrayProperty.arraySize);
            OnAddCallback?.Invoke(this, m_ArrayProperty.arraySize - 1);
            m_ArrayProperty.serializedObject.ApplyModifiedProperties();
            Refresh();
        }

        private void HandleOnItemRemoved(int in_Index)
        {
            m_ArrayProperty.DeleteArrayElementAtIndex(in_Index);
            OnRemoveCallback?.Invoke(this, in_Index);
            m_ArrayProperty.serializedObject.ApplyModifiedProperties();
            Refresh();
        }
    }
}
