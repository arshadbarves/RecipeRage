using System;
using Core.Localization;
using UnityEngine.UIElements;

namespace KitchenClash.Presentation.Extensions
{
    public static class LocalizationUIExtensions
    {
        public static void Bind(this ILocalizationManager manager, Label label, string key, object owner)
        {
            if (label == null) return;
            manager.RegisterBinding(owner, key, text => label.text = text);
        }

        public static void Bind(this ILocalizationManager manager, Button button, string key, object owner, string labelClass = null)
        {
            if (button == null) return;

            Label targetLabel = null;
            if (!string.IsNullOrEmpty(labelClass))
            {
                targetLabel = button.Q<Label>(className: labelClass);
            }
            else
            {
                targetLabel = button.Q<Label>(); 
            }

            if (targetLabel != null)
            {
                manager.RegisterBinding(owner, key, text => targetLabel.text = text);
            }
            else
            {
                button.text = manager.GetText(key);
                manager.RegisterBinding(owner, key, text => button.text = text);
            }
        }

        public static void Bind(this ILocalizationManager manager, Tab tab, string key, object owner)
        {
            if (tab == null) return;
            manager.RegisterBinding(owner, key, text => tab.label = text);
        }

        public static void Bind(this ILocalizationManager manager, VisualElement element, string key, object owner, Action<VisualElement, string> applyAction)
        {
            if (element == null) return;
            manager.RegisterBinding(owner, key, text => applyAction(element, text));
        }
    }
}
