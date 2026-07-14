using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace KitchenClash.Presentation.Common
{
    /// <summary>
    /// DOTween → UniTask bridge for Presentation UI animations.
    /// Kept local so Presentation does not depend on Infrastructure.
    /// </summary>
    public static class TweenExtensions
    {
        public static UniTask ToUniTask(this Tween tween)
        {
            var tcs = new UniTaskCompletionSource();
            tween.OnComplete(() => tcs.TrySetResult());
            tween.OnKill(() => tcs.TrySetCanceled());
            return tcs.Task;
        }
    }
}
