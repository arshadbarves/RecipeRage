using System;
using UnityEngine.UIElements;

namespace RecipeRage.UI.Animation
{
    /// <summary>
    /// Extension methods for UIDocument and VisualElement to simplify animations
    /// </summary>
    public static class UIDocumentExtensions
    {
        /// <summary>
        /// Get an animation builder for an element by name within a UIDocument.
        /// </summary>
        /// <param name="document">The UI document.</param>
        /// <param name="name">The name of the element to animate.</param>
        /// <returns>Animation builder for the element, or null if not found.</returns>
        public static UIAnimationBuilder AnimateElement(this UIDocument document, string name)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            VisualElement element = document.rootVisualElement?.Q(name);
            if (element == null)
            {
                // LogHelper.Warning("AnimateElement", $"Element with name '{name}' not found in document");
                return null; // Return null instead of throwing
            }
            return UIAnimationController.Instance.Create(element);
        }

        /// <summary>
        /// Get an animation builder for an element with a specific class within a UIDocument.
        /// </summary>
        /// <param name="document">The UI document.</param>
        /// <param name="className">The class name to look for.</param>
        /// <param name="index">Index if multiple elements have the same class (default 0).</param>
        /// <returns>Animation builder for the element, or null if not found.</returns>
        public static UIAnimationBuilder AnimateElementByClass(this UIDocument document, string className, int index = 0)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            VisualElement element = document.rootVisualElement?.Query().Class(className).AtIndex(index);
            if (element == null)
            {
                // LogHelper.Warning("AnimateElementByClass", $"Element with class '{className}' at index {index} not found in document");
                return null; // Return null instead of throwing
            }
            return UIAnimationController.Instance.Create(element);
        }

        /// <summary>
        /// Get animation builders for all elements with a specific class within a UIDocument.
        /// </summary>
        /// <param name="document">The UI document.</param>
        /// <param name="className">The class name to look for.</param>
        /// <returns>Array of animation builders (empty if none found).</returns>
        public static UIAnimationBuilder[] AnimateElementsByClass(this UIDocument document, string className)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            var elements = document.rootVisualElement?.Query().Class(className).ToList() ?? new System.Collections.Generic.List<VisualElement>();

            UIAnimationBuilder[] builders = new UIAnimationBuilder[elements.Count];
            for (int i = 0; i < elements.Count; i++)
            {
                builders[i] = UIAnimationController.Instance.Create(elements[i]);
            }
            return builders;
        }

        /// <summary>
        /// Get an animation builder for a specific VisualElement.
        /// </summary>
        /// <param name="element">The element to animate.</param>
        /// <returns>Animation builder for the element.</returns>
        public static UIAnimationBuilder Animate(this VisualElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            return UIAnimationController.Instance.Create(element);
        }

        /// <summary>
        /// Stop all animations currently running on a specific VisualElement.
        /// </summary>
        /// <param name="element">The element whose animations should be stopped.</param>
        public static void StopAnimations(this VisualElement element)
        {
            if (element == null) return;

            // Call the controller's method directly
            UIAnimationController.Instance?.StopAnimations(element);
        }
    }
}