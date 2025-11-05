using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Gameplay.Camera
{
    /// <summary>
    /// Handles camera following behavior with smooth tracking.
    /// Follows Single Responsibility Principle - only manages follow logic.
    /// </summary>
    public class CameraFollowController : IDisposable
    {
        private readonly CameraSettings _settings;
        private CinemachineCamera _virtualCamera;
        private Transform _target;
        private bool _isEnabled = true;

        public CameraFollowController(CameraSettings settings)
        {
            _settings = settings;
        }

        public void Initialize(CinemachineCamera virtualCamera)
        {
            _virtualCamera = virtualCamera;
        }

        public void SetTarget(Transform target)
        {
            _target = target;

            if (_virtualCamera != null && target != null)
            {
                _virtualCamera.Follow = target;
                _virtualCamera.LookAt = target;
            }
        }

        public void ClearTarget()
        {
            _target = null;

            if (_virtualCamera != null)
            {
                _virtualCamera.Follow = null;
                _virtualCamera.LookAt = null;
            }
        }

        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;

            if (_virtualCamera != null)
            {
                _virtualCamera.enabled = enabled;
            }
        }

        public Transform GetTarget() => _target;

        public bool IsEnabled() => _isEnabled;

        public void Dispose()
        {
            _target = null;
            _virtualCamera = null;
        }
    }
}
