using System;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace GameSystem.UI.Effects
{
    public class UIFlipEffect : IUIEffectTransition
    {
        private readonly float _duration;
        private readonly bool _isHorizontal;

        public UIFlipEffect(float duration = 0.5f, bool isHorizontal = true)
        {
            _duration = duration;
            _isHorizontal = isHorizontal;
        }

        public void ApplyTransitionIn(VisualElement uiElement, Action onComplete)
        {
            uiElement.style.display = DisplayStyle.Flex;
            AnimateFlip(uiElement, 0, 1, onComplete);
        }

        public void ApplyTransitionOut(VisualElement uiElement, Action onComplete)
        {
            AnimateFlip(uiElement, 1, 0, () =>
            {
                uiElement.style.display = DisplayStyle.None;
                onComplete?.Invoke();
            });
        }

        private void AnimateFlip(VisualElement uiElement, float startScale, float endScale, Action onComplete)
        {
            float originalWidth = uiElement.layout.width;
            float originalHeight = uiElement.layout.height;

            uiElement.experimental.animation
                .Start(
                    new StyleValues {
                        width = _isHorizontal ? originalWidth * startScale : originalWidth,
                        height = _isHorizontal ? originalHeight : originalHeight * startScale,
                        opacity = startScale
                    },
                    new StyleValues {
                        width = _isHorizontal ? originalWidth * endScale : originalWidth,
                        height = _isHorizontal ? originalHeight : originalHeight * endScale,
                        opacity = endScale
                    },
                    (int)(_duration * 1000))
                .Ease(Easing.InOutQuad)
                .OnCompleted(() =>
                {
                    uiElement.style.width = originalWidth;
                    uiElement.style.height = originalHeight;
                    onComplete?.Invoke();
                });
        }
    }
}