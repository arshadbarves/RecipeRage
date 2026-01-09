using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Core.Core.Shared.Extensions
{
    public static class TweenExtensions
    {
        public static UniTask ToUniTask(this Tween tween)
        {
            var tcs = new UniTaskCompletionSource();
            tween.OnComplete(() => tcs.TrySetResult());
            tween.OnKill(() => tcs.TrySetCanceled()); // Treat killed tween as canceled/completed
            return tcs.Task;
        }
    }
}