using System;
using UnityEngine.UIElements;


namespace GameSystem.UI.Effects
{
    public class FadeTransition : IUITransition
    {
        private readonly float _duration;

        public FadeTransition(float duration = 0.5f)
        {
            this._duration = duration;
        }

        public void TransitionIn(VisualElement uiElement, Action onComplete)
        {
            uiElement.style.opacity = 0;
            uiElement.style.display = DisplayStyle.Flex;
            
            uiElement.experimental.animation.Start((e) => e.style.opacity.value, 1, (int)(_duration * 1000),
                (element, value) => { element.style.opacity = value; }).OnCompleted(() =>
            {
                onComplete?.Invoke();
            });
        }

        public void TransitionOut(VisualElement uiElement, Action onComplete)
        {
            uiElement.experimental.animation.Start((e) => e.style.opacity.value, 0, (int)(_duration * 1000),
                (element, value) => { element.style.opacity = value; }).OnCompleted(() =>
            {
                uiElement.style.display = DisplayStyle.None;
                uiElement.style.opacity = 1;
                onComplete?.Invoke();
            });
        }
    }
}