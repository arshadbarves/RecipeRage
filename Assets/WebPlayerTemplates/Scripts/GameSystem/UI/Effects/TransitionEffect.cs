using System;
using UnityEngine.UIElements;

namespace GameSystem.UI.Effects
{
    public enum TransitionType
    {
        Fade,
        Slide,
        Scale,
        Flip,
        Rotate,
        Bounce
    }
    public interface IUIEffectTransition
    {
        void ApplyTransitionIn(VisualElement uiElement, Action onComplete);
        void ApplyTransitionOut(VisualElement uiElement, Action onComplete);
    }
}