using System;
using UnityEngine;

namespace Gameplay.Camera
{
    public interface ICameraController : IDisposable
    {
        void Initialize();
        void AddFollowTarget(Transform target, float weight = 1f, float radius = 1f);
        void RemoveFollowTarget(Transform target);
        void SetFollowTarget(Transform target);
        void ClearFollowTarget();
        void PositionForArena(Bounds arenaBounds);
        void SetArenaBounds(Bounds bounds);
        void Shake(float intensity, float duration);
        void SetZoom(float zoomLevel, float duration = 0.3f);
        void SetFollowEnabled(bool enabled);
        void Update(float deltaTime);

        UnityEngine.Camera MainCamera { get; }
        bool IsInitialized { get; }
    }
}
