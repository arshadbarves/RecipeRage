using System.Threading;
using Core.Shared.Extensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Animation
{
    public class DOTweenTransformAnimator : ITransformAnimator
    {
        private static bool _isInitialized;

        public DOTweenTransformAnimator()
        {
            if (_isInitialized) return;

            var init = DOTween.Init(
                recycleAllByDefault: true,
                useSafeMode: true,
                logBehaviour: LogBehaviour.ErrorsOnly
            );

            init?.SetCapacity(200, 50);

            _isInitialized = true;
        }

        public async UniTask MoveTo(Transform transform, Vector3 target, float duration,
            CancellationToken token = default)
        {
            if (transform == null) return;

            await transform.DOMove(target, duration)
                .SetEase(Ease.OutQuad)
                .ToUniTask()
                .AttachExternalCancellation(token);
        }

        public async UniTask ScaleTo(Transform transform, Vector3 target, float duration,
            CancellationToken token = default)
        {
            if (transform == null) return;

            await transform.DOScale(target, duration)
                .SetEase(Ease.OutQuad)
                .ToUniTask()
                .AttachExternalCancellation(token);
        }

        public async UniTask RotateTo(Transform transform, Vector3 target, float duration,
            CancellationToken token = default)
        {
            if (transform == null) return;

            await transform.DORotate(target, duration)
                .SetEase(Ease.OutQuad)
                .ToUniTask()
                .AttachExternalCancellation(token);
        }

        public async UniTask Punch(Transform transform, Vector3 direction, float duration,
            CancellationToken token = default)
        {
            if (transform == null) return;

            await transform.DOPunchPosition(direction, duration, 10, 1)
                .ToUniTask()
                .AttachExternalCancellation(token);
        }

        public async UniTask Shake(Transform transform, float duration, float strength,
            CancellationToken token = default)
        {
            if (transform == null) return;

            await transform.DOShakePosition(duration, strength, 10, 90, false, true)
                .ToUniTask()
                .AttachExternalCancellation(token);
        }
    }
}