using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameSystem.UI.Effects
{
    public class SlideTransition : IUITransition
    {
        private readonly float _duration;
        private readonly float _distance;
        private readonly NavigationMoveEvent.Direction _direction;
        private readonly bool _animateOpacity;

        public SlideTransition(float duration = 0.5f, float distance = 100, NavigationMoveEvent.Direction direction = NavigationMoveEvent.Direction.Left, bool animateOpacity = true)
        {
            this._duration = duration;
            this._distance = distance;
            this._direction = direction;
            this._animateOpacity = animateOpacity;
        }

        public void TransitionIn(VisualElement uiElement, Action onComplete)
        {
            uiElement.style.display = DisplayStyle.Flex;

            // Set opacity to 0 if opacity animation is enabled
            if (_animateOpacity)
                uiElement.style.opacity = 0;

            // Set initial position based on direction
            switch (_direction)
            {
                case NavigationMoveEvent.Direction.Left:
                    uiElement.style.left = new StyleLength(_distance);
                    break;
                case NavigationMoveEvent.Direction.Right:
                    uiElement.style.right = new StyleLength(_distance);
                    break;
                case NavigationMoveEvent.Direction.Up:
                    uiElement.style.top = new StyleLength(_distance);
                    break;
                case NavigationMoveEvent.Direction.Down:
                    uiElement.style.bottom = new StyleLength(_distance);
                    break;
            }

            // Animate opacity to 1 if enabled
            if (_animateOpacity)
            {
                uiElement.experimental.animation.Start(
                    (e) => e.style.opacity.value, 1, (int)(_duration * 1000),
                    (element, value) => { element.style.opacity = value; });
            }

            // Animate slide to original position
            AnimateSlideToPosition(uiElement, 0, () => onComplete?.Invoke());
        }

        public void TransitionOut(VisualElement uiElement, Action onComplete)
        {
            // Animate slide to outside based on direction
            AnimateSlideToPosition(uiElement, _distance, null);

            // Animate opacity to 0 if enabled
            if (_animateOpacity)
            {
                uiElement.experimental.animation.Start(
                    (e) => e.style.opacity.value, 0, (int)(_duration * 1000),
                    (element, value) => { element.style.opacity = value; }).OnCompleted(() =>
                {
                    // After transition is complete, hide the element and reset
                    uiElement.style.display = DisplayStyle.None;
                    uiElement.style.opacity = 1;
                    onComplete?.Invoke();
                });
            }
            else
            {
                // Directly hide the element without opacity animation
                uiElement.style.display = DisplayStyle.None;
                onComplete?.Invoke();
            }
        }

        private void AnimateSlideToPosition(VisualElement uiElement, float targetPosition, Action onComplete)
        {
            switch (_direction)
            {
                case NavigationMoveEvent.Direction.Left:
                    uiElement.experimental.animation.Start(
                        (e) => e.style.left.value.value, targetPosition, (int)(_duration * 1000),
                        (element, value) => { element.style.left = new StyleLength(value); }).OnCompleted(onComplete);
                    break;
                case NavigationMoveEvent.Direction.Right:
                    uiElement.experimental.animation.Start(
                        (e) => e.style.right.value.value, targetPosition, (int)(_duration * 1000),
                        (element, value) => { element.style.right = new StyleLength(value); }).OnCompleted(onComplete);
                    break;
                case NavigationMoveEvent.Direction.Up:
                    uiElement.experimental.animation.Start(
                        (e) => e.style.top.value.value, targetPosition, (int)(_duration * 1000),
                        (element, value) => { element.style.top = new StyleLength(value); }).OnCompleted(onComplete);
                    break;
                case NavigationMoveEvent.Direction.Down:
                    uiElement.experimental.animation.Start(
                        (e) => e.style.bottom.value.value, targetPosition, (int)(_duration * 1000),
                        (element, value) => { element.style.bottom = new StyleLength(value); }).OnCompleted(onComplete);
                    break;
            }
        }
    }
}
