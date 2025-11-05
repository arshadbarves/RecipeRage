using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Gameplay.Camera
{
    /// <summary>
    /// Constrains camera movement within arena bounds.
    /// Follows Single Responsibility Principle - only manages bounds constraint.
    /// </summary>
    public class CameraBoundsController : IDisposable
    {
        private readonly CameraSettings _settings;
        private CinemachineCamera _virtualCamera;
        private CinemachineConfiner3D _confiner;
        private Bounds _bounds;
        private BoxCollider _boundsCollider;

        public CameraBoundsController(CameraSettings settings)
        {
            _settings = settings;
        }

        public void Initialize(CinemachineCamera virtualCamera)
        {
            _virtualCamera = virtualCamera;

            if (_settings.enableBounds)
            {
                SetupConfiner();
            }
        }

        private void SetupConfiner()
        {
            // Add confiner extension to virtual camera
            _confiner = _virtualCamera.gameObject.AddComponent<CinemachineConfiner3D>();
            // Note: Cinemachine 3.x handles damping automatically
        }

        public void SetBounds(Bounds bounds)
        {
            _bounds = bounds;

            if (!_settings.enableBounds || _confiner == null)
                return;

            // Create a box collider for the bounds
            var boundsObj = new GameObject("CameraBounds");
            boundsObj.transform.position = bounds.center;

            _boundsCollider = boundsObj.AddComponent<BoxCollider>();
            _boundsCollider.size = bounds.size - Vector3.one * _settings.boundsPadding;
            _boundsCollider.isTrigger = true;

            // Assign to confiner
            _confiner.BoundingVolume = _boundsCollider;
        }

        public Bounds GetBounds() => _bounds;

        public void Dispose()
        {
            if (_boundsCollider != null)
            {
                UnityEngine.Object.Destroy(_boundsCollider.gameObject);
                _boundsCollider = null;
            }

            _confiner = null;
            _virtualCamera = null;
        }
    }
}
