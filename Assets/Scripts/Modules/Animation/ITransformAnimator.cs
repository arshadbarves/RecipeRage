using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Modules.Animation
{
    /// <summary>
    /// GameObject/Transform animation interface using modern async/await patterns
    /// Follows Interface Segregation Principle
    /// </summary>
    public interface ITransformAnimator
    {
        UniTask MoveTo(Transform transform, Vector3 target, float duration, CancellationToken token = default);
        UniTask ScaleTo(Transform transform, Vector3 target, float duration, CancellationToken token = default);
        UniTask RotateTo(Transform transform, Vector3 target, float duration, CancellationToken token = default);
        UniTask Punch(Transform transform, Vector3 direction, float duration, CancellationToken token = default);
        UniTask Shake(Transform transform, float duration, float strength, CancellationToken token = default);
    }
}