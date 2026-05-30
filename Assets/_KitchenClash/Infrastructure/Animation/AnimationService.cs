using KitchenClash.Application;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitchenClash.Infrastructure.Animation
{
    /// <summary>
    /// Main animation service - facade for UI and Transform animators
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

        // UI animations
        public UniTask FadeIn(VisualElement element, float duration, CancellationToken token = default)
            => _uiAnimator.FadeIn(element, duration, token);

        public UniTask FadeOut(VisualElement element, float duration, CancellationToken token = default)
            => _uiAnimator.FadeOut(element, duration, token);

        public UniTask ScaleInUI(VisualElement element, float duration, CancellationToken token = default)
            => _uiAnimator.ScaleIn(element, duration, token);

        public UniTask ScaleOutUI(VisualElement element, float duration, CancellationToken token = default)
            => _uiAnimator.ScaleOut(element, duration, token);

        // Transform animations
        public UniTask MoveTo(Transform transform, Vector3 target, float duration, CancellationToken token = default)
            => _transformAnimator.MoveTo(transform, target, duration, token);

        public UniTask ScaleTo(Transform transform, Vector3 target, float duration, CancellationToken token = default)
            => _transformAnimator.ScaleTo(transform, target, duration, token);

        public UniTask RotateTo(Transform transform, Vector3 target, float duration, CancellationToken token = default)
            => _transformAnimator.RotateTo(transform, target, duration, token);

        public UniTask Punch(Transform transform, Vector3 direction, float duration, CancellationToken token = default)
            => _transformAnimator.Punch(transform, direction, duration, token);

        public UniTask Shake(Transform transform, float duration, float strength, CancellationToken token = default)
            => _transformAnimator.Shake(transform, duration, strength, token);

        // Kill animations
        public void KillAnimations(VisualElement element)
        {
            if (element == null)
            {
                return;
            }

            DG.Tweening.DOTween.Kill(element);
        }

        // Extended UI animations
        public void FloatYoyo(VisualElement element, float distance, float duration)
        {
            if (element == null)
            {
                return;
            }

            float startY = element.resolvedStyle.translate.y;
            DG.Tweening.DOTween.To(() => startY, y =>
            {
                element.style.translate = new Translate(0, y, 0);
            }, startY + distance, duration)
            .SetLoops(-1, DG.Tweening.LoopType.Yoyo)
            .SetEase(DG.Tweening.Ease.InOutSine)
            .SetTarget(element);
        }

        public void CrossfadeLabel(VisualElement label, string newText, float fontSize, float duration)
        {
            if (label == null)
            {
                return;
            }

            DG.Tweening.DOTween.To(() => label.style.opacity.value, x => label.style.opacity = x, 0f, duration * 0.5f)
                .SetTarget(label)
                .OnComplete(() =>
                {
                    if (label is Label l)
                    {
                        l.text = newText;
                    }

                    DG.Tweening.DOTween.To(() => 0f, x => label.style.opacity = x, 1f, duration * 0.5f).SetTarget(label);
                });
        }

        public void BlurIn(VisualElement element, float blurAmount, float duration)
        {
            // UI Toolkit doesn't natively support blur; stub implementation
            if (element == null)
            {
                return;
            }
        }

        public void TrackingIn(VisualElement element, float startTracking, float endTracking, float duration)
        {
            if (element == null)
            {
                return;
            }

            element.style.letterSpacing = startTracking;
            float current = startTracking;
            DG.Tweening.DOTween.To(() => current, x =>
            {
                current = x;
                element.style.letterSpacing = x;
            }, endTracking, duration)
            .SetEase(DG.Tweening.Ease.OutQuad)
            .SetTarget(element);
        }

        public void SlideInfinite(VisualElement element, float startPercent, float endPercent, float duration)
        {
            if (element == null)
            {
                return;
            }

            float current = startPercent;
            DG.Tweening.DOTween.To(() => current, x =>
            {
                current = x;
                element.style.translate = new Translate(new Length(x, LengthUnit.Percent), 0, 0);
            }, endPercent, duration)
            .SetLoops(-1, DG.Tweening.LoopType.Restart)
            .SetEase(DG.Tweening.Ease.Linear)
            .SetTarget(element);
        }

        public void KillAnimations(Transform transform)
        {
            if (transform == null)
            {
                return;
            }

            DG.Tweening.ShortcutExtensions.DOKill(transform);
        }

        public void KillAllAnimations()
        {
            DG.Tweening.DOTween.KillAll();
        }
    }
}
