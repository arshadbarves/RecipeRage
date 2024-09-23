using System;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace GameSystem.UI.Effects
{
    public class UIZoomEffect : IUIEffectTransition
    {
        private readonly float _duration;
        private readonly float _zoomFactor;

        public UIZoomEffect(float duration = 0.5f, float zoomFactor = 1.5f)
        {
            _duration = duration;
            _zoomFactor = zoomFactor;
        }

        public void ApplyTransitionIn(VisualElement uiElement, Action onComplete)
        {
            uiElement.style.display = DisplayStyle.Flex;
            float originalWidth = uiElement.layout.width;
            float originalHeight = uiElement.layout.height;

            uiElement.style.width = 0;
            uiElement.style.height = 0;
            uiElement.style.opacity = 0;

            uiElement.experimental.animation
                .Start(new StyleValues { width = 0, height = 0, opacity = 0 },
                       new StyleValues { width = originalWidth, height = originalHeight, opacity = 1 },
                       (int)(_duration * 1000))
                .Ease(Easing.OutBack)
                .OnCompleted(() => onComplete?.Invoke());
        }

        public void ApplyTransitionOut(VisualElement uiElement, Action onComplete)
        {
            float originalWidth = uiElement.layout.width;
            float originalHeight = uiElement.layout.height;

            uiElement.experimental.animation
                .Start(new StyleValues { width = originalWidth, height = originalHeight, opacity = 1 },
                       new StyleValues { width = originalWidth * _zoomFactor, height = originalHeight * _zoomFactor, opacity = 0 },
                       (int)(_duration * 1000))
                .Ease(Easing.InBack)
                .OnCompleted(() =>
                {
                    uiElement.style.display = DisplayStyle.None;
                    uiElement.style.width = originalWidth;
                    uiElement.style.height = originalHeight;
                    uiElement.style.opacity = 1;
                    onComplete?.Invoke();
                });
        }
    }
}