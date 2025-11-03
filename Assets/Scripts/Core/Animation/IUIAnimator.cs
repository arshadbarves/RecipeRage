using System;
using UnityEngine.UIElements;

namespace Core.Animation
{
    /// <summary>
    /// UI-specific animation interface
    /// Follows Interface Segregation Principle - clients only depend on what they need
    /// Direct method calls instead of enum-based dispatch for cleaner code
    /// </summary>
    public interface IUIAnimator
    {
        void FadeIn(VisualElement element, float duration, Action onComplete = null);
        void FadeOut(VisualElement element, float duration, Action onComplete = null);
        void SlideIn(VisualElement element, SlideDirection direction, float duration, Action onComplete = null);
        void SlideOut(VisualElement element, SlideDirection direction, float duration, Action onComplete = null);
        void ScaleIn(VisualElement element, float duration, Action onComplete = null);
        void ScaleOut(VisualElement element, float duration, Action onComplete = null);
        void BounceIn(VisualElement element, float duration, Action onComplete = null);
        void BounceOut(VisualElement element, float duration, Action onComplete = null);
        void PopupIn(VisualElement container, float duration, Action onComplete = null);
        void PopupOut(VisualElement container, float duration, Action onComplete = null);
        void Pulse(VisualElement element, float duration, Action onComplete = null);
        void Shake(VisualElement element, float duration, float intensity, Action onComplete = null);
    }
    
    public enum SlideDirection
    {
        Left,
        Right,
        Top,
        Bottom
    }
}
