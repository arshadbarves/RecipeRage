using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;

namespace Core.Animation
{
    public interface IUIAnimator
    {
        UniTask FadeIn(VisualElement element, float duration, CancellationToken token = default);
        UniTask FadeOut(VisualElement element, float duration, CancellationToken token = default);
        UniTask SlideIn(VisualElement element, SlideDirection direction, float duration, CancellationToken token = default);
        UniTask SlideOut(VisualElement element, SlideDirection direction, float duration, CancellationToken token = default);
        UniTask ScaleIn(VisualElement element, float duration, CancellationToken token = default);
        UniTask ScaleOut(VisualElement element, float duration, CancellationToken token = default);
        UniTask BounceIn(VisualElement element, float duration, CancellationToken token = default);
        UniTask BounceOut(VisualElement element, float duration, CancellationToken token = default);
        UniTask PopupIn(VisualElement container, float duration, CancellationToken token = default);
        UniTask PopupOut(VisualElement container, float duration, CancellationToken token = default);
        UniTask Pulse(VisualElement element, float duration, CancellationToken token = default);
        UniTask Shake(VisualElement element, float duration, float intensity, CancellationToken token = default);

        // New Unity 6 / Advanced typography extensions & loaders
        UniTask BlurIn(VisualElement element, float startRadius, float duration, CancellationToken token = default);
        UniTask TrackingIn(Label element, float startSpacing, float endSpacing, float duration, CancellationToken token = default);
        void SlideInfinite(VisualElement element, float startPercentX, float endPercentX, float duration);

        // Ambient & content transition animations
        void FloatYoyo(VisualElement element, float offsetY, float duration);
        UniTask CrossfadeLabel(Label label, string newText, float slidePx, float duration, CancellationToken token = default);
    }

    public enum SlideDirection
    {
        Left,
        Right,
        Top,
        Bottom
    }
}