using System;
using UnityEngine;

namespace Core.Animation
{
    /// <summary>
    /// GameObject/Transform animation interface
    /// Follows Interface Segregation Principle
    /// </summary>
    public interface ITransformAnimator
    {
        void MoveTo(Transform transform, Vector3 target, float duration, Action onComplete = null);
        void ScaleTo(Transform transform, Vector3 target, float duration, Action onComplete = null);
        void RotateTo(Transform transform, Vector3 target, float duration, Action onComplete = null);
        void Punch(Transform transform, Vector3 direction, float duration, Action onComplete = null);
        void Shake(Transform transform, float duration, float strength, Action onComplete = null);
    }
}
