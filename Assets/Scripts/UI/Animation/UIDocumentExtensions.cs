using System;
using UnityEngine.UIElements;

namespace RecipeRage.UI.Animation
{
    /// <summary>
    /// Extension methods for UIDocument to simplify animations
    /// </summary>
    public static class UIDocumentExtensions
    {
        /// <summary>
        /// Get an animation controller for an element
        /// </summary>
        /// <param name="document">The UI document</param>
        /// <param name="name">The name of the element to animate</param>
        /// <returns>Animation builder for the element</returns>
        public static UIAnimationBuilder AnimateElement(this UIDocument document, string name)
        {
            VisualElement element = document.rootVisualElement.Q(name);
            if (element == null)
            {
                throw new ArgumentException($"Element with name '{name}' not found in document", nameof(name));
            }

            return UIAnimationController.Instance.Create(element);
        }

        /// <summary>
        /// Get an animation controller for an element with a specific class
        /// </summary>
        /// <param name="document">The UI document</param>
        /// <param name="className">The class name to look for</param>
        /// <param name="index">Index if multiple elements have the same class (default 0)</param>
        /// <returns>Animation builder for the element</returns>
        public static UIAnimationBuilder AnimateElementByClass(this UIDocument document, string className, int index = 0)
        {
            VisualElement element = document.rootVisualElement.Query().Class(className).AtIndex(index);
            if (element == null)
            {
                throw new ArgumentException($"Element with class '{className}' at index {index} not found in document", nameof(className));
            }

            return UIAnimationController.Instance.Create(element);
        }

        /// <summary>
        /// Get animation builders for all elements with a specific class
        /// </summary>
        /// <param name="document">The UI document</param>
        /// <param name="className">The class name to look for</param>
        /// <returns>Array of animation builders</returns>
        public static UIAnimationBuilder[] AnimateElementsByClass(this UIDocument document, string className)
        {
            UQueryBuilder<VisualElement> query = document.rootVisualElement.Query().Class(className);
            VisualElement[] elements = query.ToList().ToArray();

            if (elements.Length == 0)
            {
                throw new ArgumentException($"No elements with class '{className}' found in document", nameof(className));
            }

            UIAnimationBuilder[] builders = new UIAnimationBuilder[elements.Length];
            for (int i = 0; i < elements.Length; i++)
            {
                builders[i] = UIAnimationController.Instance.Create(elements[i]);
            }

            return builders;
        }

        /// <summary>
        /// Get an animation controller for a specific VisualElement
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <returns>Animation builder for the element</returns>
        public static UIAnimationBuilder Animate(this VisualElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return UIAnimationController.Instance.Create(element);
        }

        /// <summary>
        /// Stop all animations for a specific element
        /// </summary>
        /// <param name="element">The element</param>
        public static void StopAnimations(this VisualElement element)
        {
            // We don't have a direct way to find animations by element,
            // so we create a helper method to store IDs
            if (element == null) return;

            if (element.userData is string[] animationIds)
            {
                foreach (string id in animationIds)
                {
                    UIAnimationController.Instance.Stop(id);
                }
                element.userData = null;
            }
        }

        /// <summary>
        /// Helper to store animation ID in element userData
        /// </summary>
        /// <param name="element">The element</param>
        /// <param name="animationId">Animation ID to store</param>
        internal static void StoreAnimationId(this VisualElement element, string animationId)
        {
            if (element == null || string.IsNullOrEmpty(animationId)) return;

            string[] existingIds = element.userData as string[];
            if (existingIds == null)
            {
                element.userData = new[] { animationId };
            }
            else
            {
                string[] newIds = new string[existingIds.Length + 1];
                Array.Copy(existingIds, newIds, existingIds.Length);
                newIds[existingIds.Length] = animationId;
                element.userData = newIds;
            }
        }
    }
}