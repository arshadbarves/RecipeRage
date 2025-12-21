using System;
using Core.Bootstrap;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Animation
{
    /// <summary>
    /// Main animation service - facade for UI and Transform animators
    /// Follows Facade Pattern and Dependency Inversion Principle
    /// </summary>
    public class AnimationService : IAnimationService
    {
        private readonly IUIAnimator _uiAnimator;
        private readonly ITransformAnimator _transformAnimator;

        public AnimationService(IUIAnimator uiAnimator, ITransformAnimator transformAnimator)
        {
            _uiAnimator = uiAnimator ?? throw new ArgumentNullException(nameof(uiAnimator));
            _transformAnimator = transformAnimator ?? throw new ArgumentNullException(nameof(transformAnimator));
        }

        /// <summary>
        /// Called after all services are constructed.
        /// </summary>
        public void Initialize()
        {
            // AnimationService doesn't need cross-service setup
        }

        // UI Element animations
        public void AnimateOpacity(VisualElement element, float from, float to, float duration, Action onComplete = null)
        {
            if (element == null) return;
            
            element.style.opacity = from;
            DOTween.To(() => element.style.opacity.value, 
                x => element.style.opacity = x, 
                to, 
                duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void AnimatePosition(VisualElement element, Vector2 from, Vector2 to, float duration, Action onComplete = null)
        {
            if (element == null) return;
            
            element.style.left = from.x;
            element.style.top = from.y;
            
            DOTween.To(() => from, 
                pos =>
                {
                    element.style.left = pos.x;
                    element.style.top = pos.y;
                }, 
                to, 
                duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void AnimateScale(VisualElement element, Vector2 from, Vector2 to, float duration, Action onComplete = null)
        {
            if (element == null) return;
            
            element.style.scale = new StyleScale(from);
            
            DOTween.To(() => from, 
                scale => element.style.scale = new StyleScale(scale), 
                to, 
                duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void AnimateRotation(VisualElement element, float from, float to, float duration, Action onComplete = null)
        {
            if (element == null) return;
            
            element.style.rotate = new Rotate(from);
            
            DOTween.To(() => from, 
                rotation => element.style.rotate = new Rotate(rotation), 
                to, 
                duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => onComplete?.Invoke());
        }

        // Transform animations
        public void AnimateTransformPosition(Transform transform, Vector3 to, float duration, Action onComplete = null)
        {
            _transformAnimator.MoveTo(transform, to, duration, onComplete);
        }

        public void AnimateTransformScale(Transform transform, Vector3 to, float duration, Action onComplete = null)
        {
            _transformAnimator.ScaleTo(transform, to, duration, onComplete);
        }

        public void AnimateTransformRotation(Transform transform, Vector3 to, float duration, Action onComplete = null)
        {
            _transformAnimator.RotateTo(transform, to, duration, onComplete);
        }

        // Kill animations
        public void KillAnimations(VisualElement element)
        {
            if (element == null) return;
            DOTween.Kill(element);
        }

        public void KillAnimations(Transform transform)
        {
            if (transform == null) return;
            transform.DOKill();
        }

        public void KillAllAnimations()
        {
            DOTween.KillAll();
        }
        
        // Convenience accessors for specialized animators
        public IUIAnimator UI => _uiAnimator;
        public ITransformAnimator Transform => _transformAnimator;
    }
}
