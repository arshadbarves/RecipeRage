using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameSystem.UI.Effects
{
    public class UIWobbleEffect : IUIEffectTransition
    {
        private readonly float _duration;
        private readonly float _strength;
        private readonly int _wobbles;

        public UIWobbleEffect(float duration = 0.5f, float strength = 10f, int wobbles = 5)
        {
            _duration = duration;
            _strength = strength;
            _wobbles = wobbles;
        }

        public void ApplyTransitionIn(VisualElement uiElement, Action onComplete)
        {
            uiElement.style.display = DisplayStyle.Flex;
            AnimateWobble(uiElement, onComplete);
        }

        public void ApplyTransitionOut(VisualElement uiElement, Action onComplete)
        {
            AnimateWobble(uiElement, () =>
            {
                uiElement.style.display = DisplayStyle.None;
                onComplete?.Invoke();
            });
        }

        private void AnimateWobble(VisualElement uiElement, Action onComplete)
        {
            float startTime = Time.time;
            uiElement.schedule.Execute(() => {
                float elapsedTime = Time.time - startTime;
                if (elapsedTime < _duration)
                {
                    float progress = elapsedTime / _duration;
                    float angle = Mathf.Sin(progress * _wobbles * Mathf.PI * 2) * _strength * (1 - progress);
                    float translation = Mathf.Cos(progress * _wobbles * Mathf.PI * 2) * _strength * (1 - progress);
                    
                    uiElement.style.translate = new Translate(translation, 0, 0);
                    uiElement.style.rotate = new Rotate(new Angle(angle, AngleUnit.Degree));
                }
                else
                {
                    uiElement.style.translate = new Translate(0, 0, 0);
                    uiElement.style.rotate = new Rotate(new Angle(0, AngleUnit.Degree));
                    onComplete?.Invoke();
                }
            }).Every(16).Until(() => Time.time - startTime >= _duration);
        }
    }
}