using System;
using UnityEngine;

namespace KitchenClash.Application
{
    public interface ICameraController : IDisposable
    {
        UnityEngine.Camera MainCamera { get; }
        bool IsInitialized { get; }
        void Initialize();
        void SetFollowTarget(Transform target);
        void ClearFollowTarget();
        void SetArenaBounds(Bounds bounds);
        void AutoDetectBounds();
        void Shake(float intensity, float duration);
        void Update(float deltaTime);
    }
}
