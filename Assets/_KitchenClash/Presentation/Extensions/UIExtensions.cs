using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitchenClash.Presentation.Extensions
{
    /// <summary>
    /// Extension methods for UI Toolkit elements
    /// </summary>
    public static class UIExtensions
    {
        public static void SetBackgroundImage(this VisualElement element, Sprite sprite)
        {
            if (element == null || sprite == null) return;
            element.style.backgroundImage = new StyleBackground(sprite);
        }

        public static void SetTintColor(this VisualElement element, Color color)
        {
            if (element == null) return;
            element.style.unityBackgroundImageTintColor = color;
        }

        public static void AddClickHandler(this VisualElement element, Action callback)
        {
            if (element == null || callback == null) return;
            element.RegisterCallback<ClickEvent>(evt => callback());
        }

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

        public static void MakeCircular(this VisualElement element)
        {
            if (element == null) return;

            element.style.borderTopLeftRadius = new StyleLength(Length.Percent(50));
            element.style.borderTopRightRadius = new StyleLength(Length.Percent(50));
            element.style.borderBottomLeftRadius = new StyleLength(Length.Percent(50));
            element.style.borderBottomRightRadius = new StyleLength(Length.Percent(50));
        }

        public static void SetRoundedCorners(this VisualElement element, float radius)
        {
            if (element == null) return;

            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
        }

        public static void AddShadow(this VisualElement element, Color color, float blurRadius)
        {
            if (element == null) return;

            element.style.unityBackgroundImageTintColor = color;
            element.style.borderBottomWidth = blurRadius / 2;
            element.style.borderRightWidth = blurRadius / 2;
            element.style.borderBottomColor = color;
            element.style.borderRightColor = color;
        }
    }
}
