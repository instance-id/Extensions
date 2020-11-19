using System;
 using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace instance.id.Extensions
{
    // Hopefully these features will eventually be in the default TextField eventually.
    public class BetterTextField : TextField
    {
        /// <summary>
        /// USS class name of elements of this type.
        /// </summary>
        public const string UssClassName = "unity-better-text-field";

        /// <summary>
        /// USS class name of placeholder elements of this type.
        /// </summary>
        public const string PlaceholderUssClassName = UssClassName + "__placeholder";

        StyleSheet k_StylePath;

        readonly Label m_PlaceholderLabel;

        /// <summary>
        /// Notify external subscribers that value of text property changed.
        /// </summary>
        public Action<string> OnValueChangedHandler;

        public BetterTextField()
        {
            AddToClassList(UssClassName);

            k_StylePath = typeof(BetterTextField).GetStyleSheet($"InstanceId{nameof(BetterTextField)}");
            if (!(k_StylePath is null)) styleSheets.Add(k_StylePath); else Debug.LogError("Stylesheet not found");

            // Add and configure placeholder
            m_PlaceholderLabel = new Label {pickingMode = PickingMode.Ignore};
            m_PlaceholderLabel.AddToClassList(PlaceholderUssClassName);
            Add(m_PlaceholderLabel);

            RegisterCallback<FocusInEvent>(e => HidePlaceholder());

            RegisterCallback<FocusOutEvent>(e =>
            {
                if (string.IsNullOrEmpty(text))
                {
                    ShowPlaceholder();
                }
            });

            this.RegisterValueChangedCallback(e => OnValueChangedHandler?.Invoke(e.newValue));
        }

        void UpdatePlaceholderVisibility()
        {
            if (string.IsNullOrEmpty(value))
            {
                if (focusController?.focusedElement != this)
                {
                    ShowPlaceholder();
                }
            }
            else
            {
                HidePlaceholder();
            }
        }

        void HidePlaceholder()
        {
            m_PlaceholderLabel?.AddToClassList("hidden");
        }

        void ShowPlaceholder()
        {
            m_PlaceholderLabel?.RemoveFromClassList("hidden");
        }

        public override string value
        {
            get => base.value;
            set
            {
                // Catch case of value being set programatically.
                base.value = value;
                UpdatePlaceholderVisibility();
            }
        }

        public string Placeholder
        {
            get => m_PlaceholderLabel.text;
            set => m_PlaceholderLabel.text = value;
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            base.SetValueWithoutNotify(newValue);
            UpdatePlaceholderVisibility();
        }

        [UsedImplicitly]
        public new class UxmlFactory : UxmlFactory<BetterTextField, UxmlTraits>
        {
        }

        public new class UxmlTraits : TextField.UxmlTraits
        {
            readonly UxmlStringAttributeDescription m_Hint = new UxmlStringAttributeDescription {name = "placeholder"};

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var field = (BetterTextField) ve;
                field.Placeholder = m_Hint.GetValueFromBag(bag, cc);
            }
        }
    }
}
