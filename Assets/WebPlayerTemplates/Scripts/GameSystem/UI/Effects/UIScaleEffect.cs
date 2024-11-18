using System;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace GameSystem.UI.Effects
{
    public class UIScaleEffect : IUIEffectTransition
    {
        private readonly float _duration;
        private readonly float _endScale;
        private readonly float _startScale;

        public UIScaleEffect(float duration = 0.5f, float startScale = 0f, float endScale = 1f)
        {
            _duration = duration;
            _startScale = startScale;
            _endScale = endScale;
        }

        public void ApplyTransitionIn(VisualElement uiElement, Action onComplete)
        {
            uiElement.style.display = DisplayStyle.Flex;
            AnimateScale(uiElement, _startScale, _endScale, onComplete);
        }

        public void ApplyTransitionOut(VisualElement uiElement, Action onComplete)
        {
            AnimateScale(uiElement, _endScale, _startScale, () =>
            {
                uiElement.style.display = DisplayStyle.None;
                onComplete?.Invoke();
            });
        }

        private void AnimateScale(VisualElement uiElement, float startScale, float endScale, Action onComplete)
        {
            float originalWidth = uiElement.layout.width;
            float originalHeight = uiElement.layout.height;

            uiElement.experimental.animation
                .Start(new StyleValues { width = originalWidth * startScale, height = originalHeight * startScale },
                    new StyleValues { width = originalWidth * endScale, height = originalHeight * endScale },
                    (int)(_duration * 1000))
                .Ease(Easing.OutBack)
                .OnCompleted(() => onComplete?.Invoke());
        }
    }
}