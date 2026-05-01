using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;

namespace KitchenClash.Infrastructure.Animation
{
    public interface IUIAnimator
    {
        UniTask FadeIn(VisualElement element, float duration, CancellationToken token = default);
        UniTask FadeOut(VisualElement element, float duration, CancellationToken token = default);
        UniTask ScaleIn(VisualElement element, float duration, CancellationToken token = default);
        UniTask ScaleOut(VisualElement element, float duration, CancellationToken token = default);
    }
}
