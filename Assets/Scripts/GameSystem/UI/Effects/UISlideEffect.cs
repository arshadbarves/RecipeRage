using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace GameSystem.UI.Effects
{
    public class UISlideEffect : IUIEffectTransition
    {
        public enum SlideDirection { Left, Right, Up, Down }

        private readonly float _duration;
        private readonly SlideDirection _direction;

        public UISlideEffect(float duration = 0.5f, SlideDirection direction = SlideDirection.Left)
        {
            _duration = duration;
            _direction = direction;
        }

        public void ApplyTransitionIn(VisualElement uiElement, Action onComplete)
        {
            // Ensure the element is visible and interactive before animation
            uiElement.style.display = DisplayStyle.Flex;
            uiElement.pickingMode = PickingMode.Position;  // Enable interaction

            // Start the slide-in animation
            AnimateSlide(uiElement, true, () => onComplete?.Invoke());
        }

        public void ApplyTransitionOut(VisualElement uiElement, Action onComplete)
        {
            // Make the element non-interactive during the slide-out animation
            uiElement.pickingMode = PickingMode.Ignore;  // Disable interaction

            // Start the slide-out animation
            AnimateSlide(uiElement, false, () =>
            {
                // Hide the element and invoke the completion callback when done
                uiElement.style.display = DisplayStyle.None;
                onComplete?.Invoke();
            });
        }

        private void AnimateSlide(VisualElement uiElement, bool isIn, Action onComplete)
        {
            // Calculate the start and end positions based on whether it's sliding in or out
            Vector2 startValue = isIn ? GetOffScreenPosition(uiElement) : Vector2.zero;
            Vector2 endValue = isIn ? Vector2.zero : GetOffScreenPosition(uiElement);

            // Perform the animation using the left and top style values
            uiElement.experimental.animation
                .Start(new StyleValues { left = startValue.x, top = startValue.y },
                       new StyleValues { left = endValue.x, top = endValue.y },
                       (int)(_duration * 1000))  // Convert duration to milliseconds
                .Ease(isIn ? Easing.OutCubic : Easing.InCubic)
                .OnCompleted(() => onComplete?.Invoke());
        }

        private Vector2 GetOffScreenPosition(VisualElement uiElement)
        {
            // Force layout update to ensure the element's size is correct
            uiElement.MarkDirtyRepaint();

            float x = 0, y = 0;
            switch (_direction)
            {
                case SlideDirection.Left:
                    x = -uiElement.resolvedStyle.width;
                    break;
                case SlideDirection.Right:
                    x = uiElement.resolvedStyle.width;
                    break;
                case SlideDirection.Up:
                    y = -uiElement.resolvedStyle.height;
                    break;
                case SlideDirection.Down:
                    y = uiElement.resolvedStyle.height;
                    break;
            }

            // Return the off-screen position as a Vector2
            return new Vector2(x, y);
        }
    }
}
