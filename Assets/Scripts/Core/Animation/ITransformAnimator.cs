using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Animation
{
    public interface ITransformAnimator
    {
        UniTask MoveTo(Transform transform, Vector3 target, float duration, CancellationToken token = default);
        UniTask ScaleTo(Transform transform, Vector3 target, float duration, CancellationToken token = default);
        UniTask RotateTo(Transform transform, Vector3 target, float duration, CancellationToken token = default);
        UniTask Punch(Transform transform, Vector3 direction, float duration, CancellationToken token = default);
        UniTask Shake(Transform transform, float duration, float strength, CancellationToken token = default);
    }
}