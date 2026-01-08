using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;

namespace Modules.Animation
{
    /// <summary>
    /// UI-specific animation interface using modern async/await patterns
    /// Follows Interface Segregation Principle
    /// </summary>
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
    }
    
    public enum SlideDirection
    {
        Left,
        Right,
        Top,
        Bottom
    }
}