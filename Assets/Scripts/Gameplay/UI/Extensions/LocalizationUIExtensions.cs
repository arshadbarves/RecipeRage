using System;
using Core.Localization;
using UnityEngine.UIElements;

namespace Gameplay.UI.Extensions
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

            // If a class is provided, find the specific sub-label
            // Otherwise try to find the standard label or just use button text if it were supported directly (Button is a TextElement too, but often has sub-elements in this project)
            
            Label targetLabel = null;
            if (!string.IsNullOrEmpty(labelClass))
            {
                targetLabel = button.Q<Label>(className: labelClass);
            }
            else
            {
                // Fallback: try to find any label or assume standard structure
                // But generally in this project, buttons have specific label classes
                targetLabel = button.Q<Label>(); 
            }

            if (targetLabel != null)
            {
                manager.RegisterBinding(owner, key, text => targetLabel.text = text);
            }
            else
            {
                // If the button itself is being used as text container (not common in this template but valid in UXML)
                // button.text = ... (Button inherits from TextElement in newer Unity, or just BaseField/BindableElement)
                // Actually Button : TextElement in recent UI Toolkit versions
                button.text = manager.GetText(key); // Initial set if not bindable easily or just direct set
                manager.RegisterBinding(owner, key, text => button.text = text);
            }
        }

        public static void Bind(this ILocalizationManager manager, Tab tab, string key, object owner)
        {
            if (tab == null) return;
            manager.RegisterBinding(owner, key, text => tab.label = text);
        }

        // Generic fallback
        public static void Bind(this ILocalizationManager manager, VisualElement element, string key, object owner, Action<VisualElement, string> applyAction)
        {
            if (element == null) return;
            manager.RegisterBinding(owner, key, text => applyAction(element, text));
        }
    }
}
