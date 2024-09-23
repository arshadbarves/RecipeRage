using System;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace GameSystem.UI.Effects
{
    public class UIBounceEffect : IUIEffectTransition
    {
        public enum BounceDirection { Up, Down, Left, Right }

        private readonly float _duration;
        private readonly float _strength;
        private readonly int _bounces;
        private readonly BounceDirection _direction;

        public UIBounceEffect(float duration = 0.5f, float strength = 100f, int bounces = 3, BounceDirection direction = BounceDirection.Up)
        {
            _duration = duration;
            _strength = strength;
            _bounces = bounces;
            _direction = direction;
        }

        public void ApplyTransitionIn(VisualElement uiElement, Action onComplete)
        {
            uiElement.style.display = DisplayStyle.Flex;
            uiElement.style.position = Position.Relative;

            StyleValues startValue = GetDirectionalStyleValues(_strength);
            StyleValues endValue = GetDirectionalStyleValues(0);

            uiElement.experimental.animation
                .Start(startValue, endValue, (int)(_duration * 1000))
                .Ease(Easing.OutBounce)
                .OnCompleted(() => 
                {
                    ResetPosition(uiElement);
                    onComplete?.Invoke();
                });
        }

        public void ApplyTransitionOut(VisualElement uiElement, Action onComplete)
        {
            StyleValues startValue = GetDirectionalStyleValues(0);
            StyleValues endValue = GetDirectionalStyleValues(_strength);

            uiElement.experimental.animation
                .Start(startValue, endValue, (int)(_duration * 1000))
                .Ease(Easing.InBounce)
                .OnCompleted(() => 
                {
                    uiElement.style.display = DisplayStyle.None;
                    ResetPosition(uiElement);
                    onComplete?.Invoke();
                });
        }

        private StyleValues GetDirectionalStyleValues(float value)
        {
            switch (_direction)
            {
                case BounceDirection.Up:
                    return new StyleValues { top = -value };
                case BounceDirection.Down:
                    return new StyleValues { top = value };
                case BounceDirection.Left:
                    return new StyleValues { left = -value };
                case BounceDirection.Right:
                    return new StyleValues { left = value };
                default:
                    return new StyleValues { top = -value };
            }
        }

        private void ResetPosition(VisualElement uiElement)
        {
            uiElement.style.top = 0;
            uiElement.style.left = 0;
        }
    }
}