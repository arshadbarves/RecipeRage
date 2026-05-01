using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitchenClash.Application
{
    public interface IAnimationService
    {
        // UI animations
        UniTask FadeIn(VisualElement element, float duration, CancellationToken token = default);
        UniTask FadeOut(VisualElement element, float duration, CancellationToken token = default);
        UniTask ScaleInUI(VisualElement element, float duration, CancellationToken token = default);
        UniTask ScaleOutUI(VisualElement element, float duration, CancellationToken token = default);

        // Transform animations
        UniTask MoveTo(Transform transform, Vector3 target, float duration, CancellationToken token = default);
        UniTask ScaleTo(Transform transform, Vector3 target, float duration, CancellationToken token = default);
        UniTask RotateTo(Transform transform, Vector3 target, float duration, CancellationToken token = default);
        UniTask Punch(Transform transform, Vector3 direction, float duration, CancellationToken token = default);
        UniTask Shake(Transform transform, float duration, float strength, CancellationToken token = default);

        // Extended UI animations
        void FloatYoyo(VisualElement element, float distance, float duration);
        void CrossfadeLabel(VisualElement label, string newText, float fontSize, float duration);
        void BlurIn(VisualElement element, float blurAmount, float duration);
        void TrackingIn(VisualElement element, float startTracking, float endTracking, float duration);
        void SlideInfinite(VisualElement element, float startPercent, float endPercent, float duration);

        void KillAnimations(VisualElement element);
        void KillAnimations(Transform transform);
        void KillAllAnimations();
    }
}
