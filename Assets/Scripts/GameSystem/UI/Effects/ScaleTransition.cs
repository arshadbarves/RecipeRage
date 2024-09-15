using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameSystem.UI.Effects
{
    public class ScaleTransition : IUITransition
    {
        private readonly float _duration;
        private readonly float _scaleIn;
        private readonly float _scaleOut;

        public ScaleTransition(float duration = 0.5f, float scaleIn = 1.2f, float scaleOut = 0.8f)
        {
            this._duration = duration;
            this._scaleIn = scaleIn;
            this._scaleOut = scaleOut;
        }

        public void TransitionIn(VisualElement uiElement, Action onComplete)
        {
            uiElement.style.opacity = 0;
            uiElement.style.display = DisplayStyle.Flex;
            uiElement.transform.scale = new Vector3(0.5f, 0.5f, 1); // Start small

            uiElement.experimental.animation.Start((e) => e.style.opacity.value, 1, (int)(_duration * 1000),
                (element, value) => 
                { 
                    element.style.opacity = value; 
                    element.transform.scale = new Vector3(_scaleIn, _scaleIn, 1);
                }).OnCompleted(() => onComplete?.Invoke());
        }

        public void TransitionOut(VisualElement uiElement, Action onComplete)
        {
            uiElement.experimental.animation.Start((e) => e.style.opacity.value, 0, (int)(_duration * 1000),
                (element, value) => 
                { 
                    element.style.opacity = value; 
                    element.transform.scale = new Vector3(_scaleOut, _scaleOut, 1); // Shrink out
                }).OnCompleted(() =>
            {
                uiElement.style.display = DisplayStyle.None;
                uiElement.style.opacity = 1;
                onComplete?.Invoke();
            });
        }
    }
}