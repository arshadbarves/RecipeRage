using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameSystem.UI.Effects
{
    public class UISpinEffect : IUIEffectTransition
    {
        private readonly float _duration;
        private readonly float _rotations;
        private bool _continueSpinning;

        public UISpinEffect(float duration = 0.5f, float rotations = 1f)
        {
            _duration = duration;
            _rotations = rotations;
            _continueSpinning = false;
        }

        public void SetContinueSpinning(bool continueSpinning)
        {
            _continueSpinning = continueSpinning;
        }

        public void ApplyTransitionIn(VisualElement uiElement, Action onComplete)
        {
            uiElement.style.display = DisplayStyle.Flex;
            AnimateSpin(uiElement, 0, 360 * _rotations, () =>
            {
                if (_continueSpinning)
                {
                    ApplyTransitionIn(uiElement, onComplete);
                }
                else
                {
                    onComplete?.Invoke();
                }
            });
        }

        public void ApplyTransitionOut(VisualElement uiElement, Action onComplete)
        {
            AnimateSpin(uiElement, 360 * _rotations, 0, () =>
            {
                uiElement.style.display = DisplayStyle.None;
                onComplete?.Invoke();
            });
        }

        private void AnimateSpin(VisualElement uiElement, float startAngle, float endAngle, Action onComplete)
        {
            float startTime = Time.time;
            uiElement.schedule.Execute(() => {
                float elapsedTime = Time.time - startTime;
                if (elapsedTime < _duration)
                {
                    float t = elapsedTime / _duration;
                    float angle = Mathf.Lerp(startAngle, endAngle, t);
                    uiElement.style.rotate = new Rotate(new Angle(angle, AngleUnit.Degree));
                }
                else
                {
                    uiElement.style.rotate = new Rotate(new Angle(endAngle, AngleUnit.Degree));
                    onComplete?.Invoke();
                }
            }).Every(16).Until(() => Time.time - startTime >= _duration);
        }
    }
}