using System;
using DG.Tweening;
using UnityEngine;

namespace Core.Animation
{
    /// <summary>
    /// DOTween-based Transform animator implementation
    /// Follows Single Responsibility Principle - only handles GameObject/Transform animations
    /// </summary>
    public class DOTweenTransformAnimator : ITransformAnimator
    {
        private static bool _isInitialized;

        public DOTweenTransformAnimator()
        {
            EnsureInitialized();
        }

        private static void EnsureInitialized()
        {
            if (_isInitialized) return;

            // Initialize DOTween with optimal settings
            DOTween.Init(
                recycleAllByDefault: true,
                useSafeMode: true,
                logBehaviour: LogBehaviour.ErrorsOnly
            ).SetCapacity(200, 50);

            _isInitialized = true;
        }

        public void MoveTo(Transform transform, Vector3 target, float duration, Action onComplete = null)
        {
            if (transform == null) return;
            
            transform.DOMove(target, duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void ScaleTo(Transform transform, Vector3 target, float duration, Action onComplete = null)
        {
            if (transform == null) return;
            
            transform.DOScale(target, duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void RotateTo(Transform transform, Vector3 target, float duration, Action onComplete = null)
        {
            if (transform == null) return;
            
            transform.DORotate(target, duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void Punch(Transform transform, Vector3 direction, float duration, Action onComplete = null)
        {
            if (transform == null) return;
            
            transform.DOPunchPosition(direction, duration, 10, 1)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void Shake(Transform transform, float duration, float strength, Action onComplete = null)
        {
            if (transform == null) return;
            
            transform.DOShakePosition(duration, strength, 10, 90, false, true)
                .OnComplete(() => onComplete?.Invoke());
        }
    }
}
