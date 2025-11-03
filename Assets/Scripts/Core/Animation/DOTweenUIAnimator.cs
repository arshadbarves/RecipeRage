using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Animation
{
    /// <summary>
    /// DOTween-based UI animator implementation
    /// Follows Single Responsibility Principle - only handles UI animations
    /// Uses direct method calls for cleaner, more maintainable code
    /// </summary>
    public class DOTweenUIAnimator : IUIAnimator
    {
        private static bool _isInitialized;

        public DOTweenUIAnimator()
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

        public void FadeIn(VisualElement element, float duration, Action onComplete = null)
        {
            if (element == null) return;

            element.style.opacity = 0f;
            DOTween.To(() => element.style.opacity.value,
                x => element.style.opacity = x,
                1f,
                duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void FadeOut(VisualElement element, float duration, Action onComplete = null)
        {
            if (element == null) return;

            DOTween.To(() => element.style.opacity.value,
                x => element.style.opacity = x,
                0f,
                duration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void SlideIn(VisualElement element, SlideDirection direction, float duration, Action onComplete = null)
        {
            if (element == null) return;

            Vector2 startPos = GetSlideStartPosition(element, direction);
            Vector2 endPos = new Vector2(element.resolvedStyle.left, element.resolvedStyle.top);

            element.style.left = startPos.x;
            element.style.top = startPos.y;

            AnimatePosition(element, startPos, endPos, duration, Ease.OutQuad, onComplete);
        }

        public void SlideOut(VisualElement element, SlideDirection direction, float duration, Action onComplete = null)
        {
            if (element == null) return;

            Vector2 startPos = new Vector2(element.resolvedStyle.left, element.resolvedStyle.top);
            Vector2 endPos = GetSlideEndPosition(element, direction);

            AnimatePosition(element, startPos, endPos, duration, Ease.InQuad, onComplete);
        }

        public void ScaleIn(VisualElement element, float duration, Action onComplete = null)
        {
            if (element == null) return;

            element.style.scale = new StyleScale(Vector2.zero);

            DOTween.To(() => 0f,
                x => element.style.scale = new StyleScale(new Vector2(x, x)),
                1f,
                duration)
                .SetEase(Ease.OutBack)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void ScaleOut(VisualElement element, float duration, Action onComplete = null)
        {
            if (element == null) return;

            DOTween.To(() => 1f,
                x => element.style.scale = new StyleScale(new Vector2(x, x)),
                0f,
                duration)
                .SetEase(Ease.InBack)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void BounceIn(VisualElement element, float duration, Action onComplete = null)
        {
            if (element == null) return;

            element.style.scale = new StyleScale(Vector2.zero);
            element.style.opacity = 0f;

            var sequence = DOTween.Sequence();

            // Scale with elastic bounce
            sequence.Append(DOTween.To(() => 0f,
                x => element.style.scale = new StyleScale(new Vector2(x, x)),
                1f,
                duration)
                .SetEase(Ease.OutElastic));

            // Fade in simultaneously
            sequence.Join(DOTween.To(() => 0f,
                x => element.style.opacity = x,
                1f,
                duration * 0.5f)
                .SetEase(Ease.OutQuad));

            sequence.OnComplete(() => onComplete?.Invoke());
        }

        public void BounceOut(VisualElement element, float duration, Action onComplete = null)
        {
            if (element == null) return;

            var sequence = DOTween.Sequence();

            // Scale down with bounce
            sequence.Append(DOTween.To(() => 1f,
                x => element.style.scale = new StyleScale(new Vector2(x, x)),
                0f,
                duration)
                .SetEase(Ease.InBack));

            // Fade out simultaneously
            sequence.Join(DOTween.To(() => 1f,
                x => element.style.opacity = x,
                0f,
                duration * 0.7f)
                .SetEase(Ease.InQuad));

            sequence.OnComplete(() => onComplete?.Invoke());
        }

        public void PopupIn(VisualElement container, float duration, Action onComplete = null)
        {
            if (container == null) return;

            // Find overlay and content elements
            VisualElement overlay = container.Q<VisualElement>("popup-overlay")
                                    ?? container.Q<VisualElement>("modal-background");

            VisualElement content = container.Q<VisualElement>("popup-container")
                                 ?? container.Q<VisualElement>("modal-content");

            var sequence = DOTween.Sequence();

            // Fade in overlay
            if (overlay != null)
            {
                overlay.style.opacity = 0f;
                sequence.Append(DOTween.To(() => 0f,
                    x => overlay.style.opacity = x,
                    1f,
                    duration * 0.5f)
                    .SetEase(Ease.OutQuad));
            }

            // Bounce in content (starts slightly after overlay)
            if (content != null)
            {
                content.style.scale = new StyleScale(Vector2.zero);
                content.style.opacity = 0f;

                sequence.Insert(duration * 0.2f, DOTween.To(() => 0f,
                    x => content.style.scale = new StyleScale(new Vector2(x, x)),
                    1f,
                    duration * 0.8f)
                    .SetEase(Ease.OutElastic));

                sequence.Insert(duration * 0.2f, DOTween.To(() => 0f,
                    x => content.style.opacity = x,
                    1f,
                    duration * 0.4f)
                    .SetEase(Ease.OutQuad));
            }

            sequence.OnComplete(() => onComplete?.Invoke());
        }

        public void PopupOut(VisualElement container, float duration, Action onComplete = null)
        {
            if (container == null) return;

            // Find overlay and content elements
            VisualElement overlay = container.Q<VisualElement>("popup-background")
                                 ?? container.Q<VisualElement>("modal-background")
                                 ?? container.Q<VisualElement>("modal-bg")
                                 ?? container.Q<VisualElement>("popup-bg");

            VisualElement content = container.Q<VisualElement>("popup-content")
                                 ?? container.Q<VisualElement>("modal-content");

            var sequence = DOTween.Sequence();

            // Bounce out content first
            if (content != null)
            {
                sequence.Append(DOTween.To(() => 1f,
                    x => content.style.scale = new StyleScale(new Vector2(x, x)),
                    0f,
                    duration * 0.6f)
                    .SetEase(Ease.InBack));

                sequence.Join(DOTween.To(() => 1f,
                    x => content.style.opacity = x,
                    0f,
                    duration * 0.4f)
                    .SetEase(Ease.InQuad));
            }

            // Fade out overlay
            if (overlay != null)
            {
                sequence.Append(DOTween.To(() => 1f,
                    x => overlay.style.opacity = x,
                    0f,
                    duration * 0.4f)
                    .SetEase(Ease.InQuad));
            }

            sequence.OnComplete(() => onComplete?.Invoke());
        }

        public void Pulse(VisualElement element, float duration, Action onComplete = null)
        {
            if (element == null) return;

            const float pulseScale = 1.1f;
            float halfDuration = duration * 0.5f;

            var sequence = DOTween.Sequence();
            sequence.Append(DOTween.To(() => 1f,
                x => element.style.scale = new StyleScale(new Vector2(x, x)),
                pulseScale,
                halfDuration)
                .SetEase(Ease.OutQuad));

            sequence.Append(DOTween.To(() => pulseScale,
                x => element.style.scale = new StyleScale(new Vector2(x, x)),
                1f,
                halfDuration)
                .SetEase(Ease.InQuad));

            sequence.OnComplete(() => onComplete?.Invoke());
        }

        public void Shake(VisualElement element, float duration, float intensity, Action onComplete = null)
        {
            if (element == null) return;

            var originalPos = new Vector2(element.resolvedStyle.left, element.resolvedStyle.top);

            DOTween.To(() => 0f,
                progress =>
                {
                    float currentIntensity = intensity * (1f - progress);
                    float shakeX = Mathf.Sin(progress * 20f) * currentIntensity;
                    float shakeY = Mathf.Cos(progress * 25f) * currentIntensity;

                    element.style.left = originalPos.x + shakeX;
                    element.style.top = originalPos.y + shakeY;
                },
                1f,
                duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    element.style.left = originalPos.x;
                    element.style.top = originalPos.y;
                    onComplete?.Invoke();
                });
        }

        private void AnimatePosition(VisualElement element, Vector2 from, Vector2 to, float duration, Ease ease, Action onComplete)
        {
            DOTween.To(() => from,
                pos =>
                {
                    element.style.left = pos.x;
                    element.style.top = pos.y;
                },
                to,
                duration)
                .SetEase(ease)
                .OnComplete(() => onComplete?.Invoke());
        }

        private Vector2 GetSlideStartPosition(VisualElement element, SlideDirection direction)
        {
            float parentWidth = element.parent?.resolvedStyle.width ?? Screen.width;
            float parentHeight = element.parent?.resolvedStyle.height ?? Screen.height;

            return direction switch
            {
                SlideDirection.Left => new Vector2(-element.resolvedStyle.width, element.resolvedStyle.top),
                SlideDirection.Right => new Vector2(parentWidth, element.resolvedStyle.top),
                SlideDirection.Top => new Vector2(element.resolvedStyle.left, -element.resolvedStyle.height),
                SlideDirection.Bottom => new Vector2(element.resolvedStyle.left, parentHeight),
                _ => Vector2.zero
            };
        }

        private Vector2 GetSlideEndPosition(VisualElement element, SlideDirection direction)
        {
            float parentWidth = element.parent?.resolvedStyle.width ?? Screen.width;
            float parentHeight = element.parent?.resolvedStyle.height ?? Screen.height;

            return direction switch
            {
                SlideDirection.Left => new Vector2(-element.resolvedStyle.width, element.resolvedStyle.top),
                SlideDirection.Right => new Vector2(parentWidth, element.resolvedStyle.top),
                SlideDirection.Top => new Vector2(element.resolvedStyle.left, -element.resolvedStyle.height),
                SlideDirection.Bottom => new Vector2(element.resolvedStyle.left, parentHeight),
                _ => Vector2.zero
            };
        }
    }
}
