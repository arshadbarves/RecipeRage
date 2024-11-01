using System;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace GameSystem.UI.Effects
{
    public class UIShakeEffect : IUIEffectTransition
    {
        private readonly float _duration;
        private readonly int _shakeCount;
        private readonly float _shakeStrength;

        public UIShakeEffect(float duration = 0.5f, float shakeStrength = 5f, int shakeCount = 10)
        {
            _duration = duration;
            _shakeStrength = shakeStrength;
            _shakeCount = shakeCount;
        }

        public void ApplyTransitionIn(VisualElement uiElement, Action onComplete)
        {
            uiElement.style.display = DisplayStyle.Flex;
            AnimateShake(uiElement, onComplete);
        }

        public void ApplyTransitionOut(VisualElement uiElement, Action onComplete)
        {
            AnimateShake(uiElement, () =>
            {
                uiElement.style.display = DisplayStyle.None;
                onComplete?.Invoke();
            });
        }

        private void AnimateShake(VisualElement uiElement, Action onComplete)
        {
            float startTime = Time.time;
            float originalLeft = uiElement.layout.x;
            float originalTop = uiElement.layout.y;

            uiElement.schedule.Execute(() =>
            {
                float elapsedTime = Time.time - startTime;
                if (elapsedTime < _duration)
                {
                    float progress = elapsedTime / _duration;
                    float shakeX = (Random.value - 0.5f) * 2 * _shakeStrength * (1 - progress);
                    float shakeY = (Random.value - 0.5f) * 2 * _shakeStrength * (1 - progress);
                    uiElement.style.left = originalLeft + shakeX;
                    uiElement.style.top = originalTop + shakeY;
                }
                else
                {
                    uiElement.style.left = originalLeft;
                    uiElement.style.top = originalTop;
                    onComplete?.Invoke();
                }
            }).Every(16).Until(() => Time.time - startTime >= _duration);
        }
    }
}