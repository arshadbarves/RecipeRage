using System;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace GameSystem.UI.Effects
{
    public class UIFadeEffect : IUIEffectTransition
    {
        private readonly float _duration;

        public UIFadeEffect(float duration = 0.5f)
        {
            _duration = duration;
        }

        public void ApplyTransitionIn(VisualElement uiElement, Action onComplete)
        {
            uiElement.style.display = DisplayStyle.Flex;
            AnimateFade(uiElement, 0, 1, onComplete);
        }

        public void ApplyTransitionOut(VisualElement uiElement, Action onComplete)
        {
            AnimateFade(uiElement, 1, 0, () =>
            {
                uiElement.style.display = DisplayStyle.None;
                onComplete?.Invoke();
            });
        }

        private void AnimateFade(VisualElement uiElement, float start, float end, Action onComplete)
        {
            uiElement.experimental.animation
                .Start(new StyleValues { opacity = start },
                    new StyleValues { opacity = end },
                    (int)(_duration * 1000))
                .Ease(Easing.InOutQuad)
                .OnCompleted(() => onComplete?.Invoke());
        }
    }
}