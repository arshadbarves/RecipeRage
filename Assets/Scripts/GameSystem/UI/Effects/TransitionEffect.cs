using System;
using UnityEngine.UIElements;

namespace GameSystem.UI.Effects
{
    public enum TransitionType
    {
        Fade,
        Slide,
        Scale,
        Custom
    }
    public interface IUITransition
    {
        void TransitionIn(VisualElement uiElement, Action onComplete);
        void TransitionOut(VisualElement uiElement, Action onComplete);
    }
}