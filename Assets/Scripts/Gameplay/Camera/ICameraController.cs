using System;
using UnityEngine;

namespace Gameplay.Camera
{
    public interface ICameraController : IDisposable
    {
        void Initialize();
        void SetFollowTarget(Transform target);
        void ClearFollowTarget();
        void SetArenaBounds(Bounds bounds);
        void Shake(float intensity, float duration);
        void Update(float deltaTime);

        UnityEngine.Camera MainCamera { get; }
        bool IsInitialized { get; }
    }
}
