using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameSystem.UI.Effects
{
    public class UIBlinkEffect : IUIEffectTransition
    {
        private readonly float _duration;
        private readonly int _blinkCount;

        public UIBlinkEffect(float duration = 0.5f, int blinkCount = 3)
        {
            _duration = duration;
            _blinkCount = blinkCount;
        }

        public void ApplyTransitionIn(VisualElement uiElement, Action onComplete)
        {
            uiElement.style.display = DisplayStyle.Flex;
            AnimateBlink(uiElement, onComplete);
        }

        public void ApplyTransitionOut(VisualElement uiElement, Action onComplete)
        {
            AnimateBlink(uiElement, () =>
            {
                uiElement.style.display = DisplayStyle.None;
                onComplete?.Invoke();
            });
        }

        private void AnimateBlink(VisualElement uiElement, Action onComplete)
        {
            float startTime = Time.time;
            float blinkDuration = _duration / (_blinkCount * 2);

            uiElement.schedule.Execute(() => {
                float elapsedTime = Time.time - startTime;
                if (elapsedTime < _duration)
                {
                    int blinkPhase = Mathf.FloorToInt(elapsedTime / blinkDuration);
                    uiElement.style.opacity = blinkPhase % 2 == 0 ? 1 : 0;
                }
                else
                {
                    uiElement.style.opacity = 1;
                    onComplete?.Invoke();
                }
            }).Every(16).Until(() => Time.time - startTime >= _duration);
        }
    }
}