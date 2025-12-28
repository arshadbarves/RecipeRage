using System;
using Core.Animation;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tests.Editor.Mocks
{
    public class MockUIAnimator : IUIAnimator
    {
        public void FadeIn(VisualElement element, float duration, Action onComplete = null) { onComplete?.Invoke(); }
        public void FadeOut(VisualElement element, float duration, Action onComplete = null) { onComplete?.Invoke(); }
        public void SlideIn(VisualElement element, Vector2 from, float duration, Action onComplete = null) { onComplete?.Invoke(); }
        public void SlideIn(VisualElement element, SlideDirection direction, float duration, Action onComplete = null) { onComplete?.Invoke(); }
        public void SlideOut(VisualElement element, Vector2 to, float duration, Action onComplete = null) { onComplete?.Invoke(); }
        public void SlideOut(VisualElement element, SlideDirection direction, float duration, Action onComplete = null) { onComplete?.Invoke(); }
        public void ScaleIn(VisualElement element, float duration, Action onComplete = null) { onComplete?.Invoke(); }
        public void ScaleOut(VisualElement element, float duration, Action onComplete = null) { onComplete?.Invoke(); }
        public void BounceIn(VisualElement element, float duration, Action onComplete = null) { onComplete?.Invoke(); }
        public void BounceOut(VisualElement element, float duration, Action onComplete = null) { onComplete?.Invoke(); }
        public void PopupIn(VisualElement element, float duration, Action onComplete = null) { onComplete?.Invoke(); }
        public void PopupOut(VisualElement element, float duration, Action onComplete = null) { onComplete?.Invoke(); }
        public void Pulse(VisualElement element, float duration, Action onComplete = null) { onComplete?.Invoke(); }
        public void Shake(VisualElement element, float duration, float intensity, Action onComplete = null) { onComplete?.Invoke(); }
    }
}
