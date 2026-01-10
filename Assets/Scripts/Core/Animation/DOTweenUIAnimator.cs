using System.Threading;
using Core.Shared.Extensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Animation
{
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

            IDOTweenInit init = DOTween.Init(
                recycleAllByDefault: true,
                useSafeMode: true,
                logBehaviour: LogBehaviour.ErrorsOnly
            );

            init?.SetCapacity(200, 50);

            _isInitialized = true;
        }

        public async UniTask FadeIn(VisualElement element, float duration, CancellationToken token = default)
        {
            if (element == null) return;

            element.style.opacity = 0f;
            await DOTween.To(() => element.style.opacity.value,
                x => element.style.opacity = x,
                1f,
                duration)
                .SetEase(Ease.OutQuad)
                .ToUniTask()
                .AttachExternalCancellation(token);
        }

        public async UniTask FadeOut(VisualElement element, float duration, CancellationToken token = default)
        {
            if (element == null) return;

            await DOTween.To(() => element.style.opacity.value,
                x => element.style.opacity = x,
                0f,
                duration)
                .SetEase(Ease.InQuad)
                .ToUniTask()
                .AttachExternalCancellation(token);
        }

        public async UniTask SlideIn(VisualElement element, SlideDirection direction, float duration, CancellationToken token = default)
        {
            if (element == null) return;

            Vector2 startPos = GetSlideStartPosition(element, direction);
            Vector2 endPos = new Vector2(element.resolvedStyle.left, element.resolvedStyle.top);

            element.style.left = startPos.x;
            element.style.top = startPos.y;

            await AnimatePosition(element, startPos, endPos, duration, Ease.OutQuad, token);
        }

        public async UniTask SlideOut(VisualElement element, SlideDirection direction, float duration, CancellationToken token = default)
        {
            if (element == null) return;

            Vector2 startPos = new Vector2(element.resolvedStyle.left, element.resolvedStyle.top);
            Vector2 endPos = GetSlideEndPosition(element, direction);

            await AnimatePosition(element, startPos, endPos, duration, Ease.InQuad, token);
        }

        public async UniTask ScaleIn(VisualElement element, float duration, CancellationToken token = default)
        {
            if (element == null) return;

            element.style.scale = new StyleScale(Vector2.zero);

            await DOTween.To(() => 0f,
                x => element.style.scale = new StyleScale(new Vector2(x, x)),
                1f,
                duration)
                .SetEase(Ease.OutBack)
                .ToUniTask()
                .AttachExternalCancellation(token);
        }

        public async UniTask ScaleOut(VisualElement element, float duration, CancellationToken token = default)
        {
            if (element == null) return;

            await DOTween.To(() => 1f,
                x => element.style.scale = new StyleScale(new Vector2(x, x)),
                0f,
                duration)
                .SetEase(Ease.InBack)
                .ToUniTask()
                .AttachExternalCancellation(token);
        }

        public async UniTask BounceIn(VisualElement element, float duration, CancellationToken token = default)
        {
            if (element == null) return;

            element.style.scale = new StyleScale(Vector2.zero);
            element.style.opacity = 0f;

            Sequence sequence = DOTween.Sequence();

            sequence.Append(DOTween.To(() => 0f,
                x => element.style.scale = new StyleScale(new Vector2(x, x)),
                1f,
                duration)
                .SetEase(Ease.OutElastic));

            sequence.Join(DOTween.To(() => 0f,
                x => element.style.opacity = x,
                1f,
                duration * 0.5f)
                .SetEase(Ease.OutQuad));

            await sequence.ToUniTask().AttachExternalCancellation(token);
        }

        public async UniTask BounceOut(VisualElement element, float duration, CancellationToken token = default)
        {
            if (element == null) return;

            Sequence sequence = DOTween.Sequence();

            sequence.Append(DOTween.To(() => 1f,
                x => element.style.scale = new StyleScale(new Vector2(x, x)),
                0f,
                duration)
                .SetEase(Ease.InBack));

            sequence.Join(DOTween.To(() => 1f,
                x => element.style.opacity = x,
                0f,
                duration * 0.7f)
                .SetEase(Ease.InQuad));

            await sequence.ToUniTask().AttachExternalCancellation(token);
        }

        public async UniTask PopupIn(VisualElement container, float duration, CancellationToken token = default)
        {
            if (container == null) return;

            VisualElement overlay = container.Q<VisualElement>("modal-background");
            VisualElement content = container.Q<VisualElement>("modal-content");

            Sequence sequence = DOTween.Sequence();

            if (overlay != null)
            {
                overlay.style.opacity = 0f;
                sequence.Append(DOTween.To(() => 0f,
                    x => overlay.style.opacity = x,
                    1f,
                    duration * 0.5f)
                    .SetEase(Ease.OutQuad));
            }

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

            await sequence.ToUniTask().AttachExternalCancellation(token);
        }

        public async UniTask PopupOut(VisualElement container, float duration, CancellationToken token = default)
        {
            if (container == null) return;

            VisualElement overlay = container.Q<VisualElement>("modal-background");
            VisualElement content = container.Q<VisualElement>("modal-content");

            Sequence sequence = DOTween.Sequence();

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

            if (overlay != null)
            {
                sequence.Append(DOTween.To(() => 1f,
                    x => overlay.style.opacity = x,
                    0f,
                    duration * 0.4f)
                    .SetEase(Ease.InQuad));
            }

            await sequence.ToUniTask().AttachExternalCancellation(token);
        }

        public async UniTask Pulse(VisualElement element, float duration, CancellationToken token = default)
        {
            if (element == null) return;

            const float pulseScale = 1.1f;
            float halfDuration = duration * 0.5f;

            Sequence sequence = DOTween.Sequence();
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

            await sequence.ToUniTask().AttachExternalCancellation(token);
        }

        public async UniTask Shake(VisualElement element, float duration, float intensity, CancellationToken token = default)
        {
            if (element == null) return;

            var originalPos = new Vector2(element.resolvedStyle.left, element.resolvedStyle.top);

            await DOTween.To(() => 0f,
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
                .ToUniTask()
                .AttachExternalCancellation(token);

            element.style.left = originalPos.x;
            element.style.top = originalPos.y;
        }

        private async UniTask AnimatePosition(VisualElement element, Vector2 from, Vector2 to, float duration, Ease ease, CancellationToken token)
        {
            await DOTween.To(() => from,
                pos =>
                {
                    element.style.left = pos.x;
                    element.style.top = pos.y;
                },
                to,
                duration)
                .SetEase(ease)
                .ToUniTask()
                .AttachExternalCancellation(token);
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