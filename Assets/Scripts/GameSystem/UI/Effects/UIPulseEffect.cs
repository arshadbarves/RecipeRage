using System;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace GameSystem.UI.Effects
{
    public class UIPulseEffect : IUIEffectTransition
    {
        private readonly float _duration;
        private readonly float _maxScale;
        private readonly float _minScale;

        public UIPulseEffect(float duration = 0.5f, float minScale = 0.9f, float maxScale = 1.1f)
        {
            _duration = duration;
            _minScale = minScale;
            _maxScale = maxScale;
        }

        public void ApplyTransitionIn(VisualElement uiElement, Action onComplete)
        {
            uiElement.style.display = DisplayStyle.Flex;
            AnimatePulse(uiElement, onComplete);
        }

        public void ApplyTransitionOut(VisualElement uiElement, Action onComplete)
        {
            AnimatePulse(uiElement, () =>
            {
                uiElement.style.display = DisplayStyle.None;
                onComplete?.Invoke();
            });
        }

        private void AnimatePulse(VisualElement uiElement, Action onComplete)
        {
            float originalWidth = uiElement.layout.width;
            float originalHeight = uiElement.layout.height;

            uiElement.experimental.animation
                .Start(new StyleValues { width = originalWidth, height = originalHeight },
                    new StyleValues { width = originalWidth * _maxScale, height = originalHeight * _maxScale },
                    (int)(_duration * 500))
                .Ease(Easing.InOutQuad)
                .OnCompleted(() =>
                {
                    uiElement.experimental.animation
                        .Start(new StyleValues { width = originalWidth * _maxScale, height = originalHeight * _maxScale },
                            new StyleValues { width = originalWidth * _minScale, height = originalHeight * _minScale },
                            (int)(_duration * 500))
                        .Ease(Easing.InOutQuad)
                        .OnCompleted(() =>
                        {
                            uiElement.style.width = originalWidth;
                            uiElement.style.height = originalHeight;
                            onComplete?.Invoke();
                        });
                });
        }
    }
}