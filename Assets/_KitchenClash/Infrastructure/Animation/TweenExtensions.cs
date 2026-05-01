using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace KitchenClash.Infrastructure.Animation
{
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
