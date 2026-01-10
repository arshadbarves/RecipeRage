using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.UI
{
    /// <summary>
    /// Extension methods for UI Toolkit elements
    /// </summary>
    public static class UIExtensions
    {
        /// <summary>
        /// Set the background image of a VisualElement
        /// </summary>
        /// <param name="element">The element to set the background image for</param>
        /// <param name="sprite">The sprite to use as background</param>
        public static void SetBackgroundImage(this VisualElement element, Sprite sprite)
        {
            if (element == null || sprite == null) return;
            element.style.backgroundImage = new StyleBackground(sprite);
        }

        /// <summary>
        /// Set the tint color of a VisualElement
        /// </summary>
        /// <param name="element">The element to set the tint color for</param>
        /// <param name="color">The color to use</param>
        public static void SetTintColor(this VisualElement element, Color color)
        {
            if (element == null) return;
            element.style.unityBackgroundImageTintColor = color;
        }

        /// <summary>
        /// Add a click event handler to a VisualElement
        /// </summary>
        /// <param name="element">The element to add the click handler to</param>
        /// <param name="callback">The callback to invoke when clicked</param>
        public static void AddClickHandler(this VisualElement element, Action callback)
        {
            if (element == null || callback == null) return;

            element.RegisterCallback<ClickEvent>(evt => callback());
        }

        /// <summary>
        /// Make an element draggable within its parent bounds
        /// </summary>
        /// <param name="element">The element to make draggable</param>
        /// <param name="dragHandle">Optional drag handle (if null, the entire element is draggable)</param>
        public static void MakeDraggable(this VisualElement element, VisualElement dragHandle = null)
        {
            if (element == null) return;

            VisualElement handle = dragHandle ?? element;
            Vector2 startPosition = Vector2.zero;
            Vector2 pointerStartPosition = Vector2.zero;

            handle.RegisterCallback<PointerDownEvent>(evt =>
            {
                startPosition = element.transform.position;
                pointerStartPosition = evt.position;
                handle.CapturePointer(evt.pointerId);
            });

            handle.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (handle.HasPointerCapture(evt.pointerId))
                {
                    Vector2 pointerDelta = new Vector2(evt.position.x - pointerStartPosition.x, evt.position.y - pointerStartPosition.y);
                    element.transform.position = startPosition + pointerDelta;
                }
            });

            handle.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (handle.HasPointerCapture(evt.pointerId))
                {
                    handle.ReleasePointer(evt.pointerId);
                }
            });
        }

        /// <summary>
        /// Add a hover effect to a VisualElement
        /// </summary>
        /// <param name="element">The element to add the hover effect to</param>
        /// <param name="hoverScale">The scale factor when hovered</param>
        /// <param name="duration">The duration of the scale animation</param>
        public static void AddHoverEffect(this VisualElement element, float hoverScale = 1.05f, float duration = 0.1f)
        {
            if (element == null) return;

            element.RegisterCallback<MouseEnterEvent>(evt =>
            {
                element.experimental.animation.Scale(hoverScale, (int)(duration * 1000));
            });

            element.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                element.experimental.animation.Scale(1f, (int)(duration * 1000));
            });
        }

        /// <summary>
        /// Add a press effect to a VisualElement
        /// </summary>
        /// <param name="element">The element to add the press effect to</param>
        /// <param name="pressScale">The scale factor when pressed</param>
        /// <param name="duration">The duration of the scale animation</param>
        public static void AddPressEffect(this VisualElement element, float pressScale = 0.95f, float duration = 0.1f)
        {
            if (element == null) return;

            element.RegisterCallback<MouseDownEvent>(evt =>
            {
                element.experimental.animation.Scale(pressScale, (int)(duration * 1000));
            });

            element.RegisterCallback<MouseUpEvent>(evt =>
            {
                element.experimental.animation.Scale(1f, (int)(duration * 1000));
            });
        }

        /// <summary>
        /// Create a circular mask for a VisualElement
        /// </summary>
        /// <param name="element">The element to apply the circular mask to</param>
        public static void MakeCircular(this VisualElement element)
        {
            if (element == null) return;

            element.style.borderTopLeftRadius = new StyleLength(Length.Percent(50));
            element.style.borderTopRightRadius = new StyleLength(Length.Percent(50));
            element.style.borderBottomLeftRadius = new StyleLength(Length.Percent(50));
            element.style.borderBottomRightRadius = new StyleLength(Length.Percent(50));
        }

        /// <summary>
        /// Set rounded corners for a VisualElement
        /// </summary>
        /// <param name="element">The element to apply rounded corners to</param>
        /// <param name="radius">The radius in pixels</param>
        public static void SetRoundedCorners(this VisualElement element, float radius)
        {
            if (element == null) return;

            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
        }

        /// <summary>
        /// Add a shadow effect to a VisualElement
        /// </summary>
        /// <param name="element">The element to add the shadow to</param>
        /// <param name="color">The shadow color</param>
        /// <param name="blurRadius">The shadow blur radius</param>
        public static void AddShadow(this VisualElement element, Color color, float blurRadius)
        {
            if (element == null) return;

            // Note: boxShadow is not directly supported in Unity UI Toolkit
            // Using background color tint as a simple alternative
            element.style.unityBackgroundImageTintColor = color;

            // Apply a slight border to simulate shadow effect
            element.style.borderBottomWidth = blurRadius / 2;
            element.style.borderRightWidth = blurRadius / 2;
            element.style.borderBottomColor = color;
            element.style.borderRightColor = color;
        }


    }
}
