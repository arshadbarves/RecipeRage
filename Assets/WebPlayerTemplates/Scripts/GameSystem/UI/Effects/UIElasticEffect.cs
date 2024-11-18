using System;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace GameSystem.UI.Effects
{
    public class UIElasticEffect : IUIEffectTransition
    {
        private readonly float _duration;
        private readonly float _overshootFactor;

        public UIElasticEffect(float duration = 0.5f, float overshootFactor = 1.2f)
        {
            _duration = duration;
            _overshootFactor = overshootFactor;
        }

        public void ApplyTransitionIn(VisualElement uiElement, Action onComplete)
        {
            uiElement.style.display = DisplayStyle.Flex;
            AnimateElastic(uiElement, 0, 1, onComplete);
        }

        public void ApplyTransitionOut(VisualElement uiElement, Action onComplete)
        {
            AnimateElastic(uiElement, 1, 0, () =>
            {
                uiElement.style.display = DisplayStyle.None;
                onComplete?.Invoke();
            });
        }

        private void AnimateElastic(VisualElement uiElement, float start, float end, Action onComplete)
        {
            float originalWidth = uiElement.layout.width;
            float originalHeight = uiElement.layout.height;

            uiElement.experimental.animation
                .Start(
                    new StyleValues {
                        width = originalWidth * start,
                        height = originalHeight * start,
                        opacity = start
                    },
                    new StyleValues {
                        width = originalWidth * end * _overshootFactor,
                        height = originalHeight * end * _overshootFactor,
                        opacity = end
                    },
                    (int)(_duration * 750))
                .Ease(Easing.OutBack)
                .OnCompleted(() =>
                {
                    uiElement.experimental.animation
                        .Start(
                            new StyleValues {
                                width = originalWidth * end * _overshootFactor,
                                height = originalHeight * end * _overshootFactor
                            },
                            new StyleValues {
                                width = originalWidth * end,
                                height = originalHeight * end
                            },
                            (int)(_duration * 250))
                        .Ease(Easing.InOutQuad)
                        .OnCompleted(() => onComplete?.Invoke());
                });
        }
    }
}