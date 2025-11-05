using System;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

namespace Gameplay.Camera
{
    /// <summary>
    /// Handles dynamic camera zoom with smooth transitions.
    /// Uses DOTween via AnimationService for smooth zoom animations.
    /// Follows Single Responsibility Principle - only manages zoom.
    /// </summary>
    public class CameraZoomController : IDisposable
    {
        private readonly CameraSettings _settings;
        private CinemachineCamera _virtualCamera;
        private UnityEngine.Camera _mainCamera;
        private float _currentZoom;
        private float _targetZoom;
        private Tween _zoomTween;

        public CameraZoomController(CameraSettings settings)
        {
            _settings = settings;
            _currentZoom = settings.defaultZoom;
            _targetZoom = settings.defaultZoom;
        }

        public void Initialize(CinemachineCamera virtualCamera, UnityEngine.Camera mainCamera)
        {
            _virtualCamera = virtualCamera;
            _mainCamera = mainCamera;
            _currentZoom = _settings.defaultZoom;
            _targetZoom = _settings.defaultZoom;
        }

        public void SetZoom(float zoomLevel, float duration)
        {
            // Clamp zoom level
            zoomLevel = Mathf.Clamp(zoomLevel, _settings.minZoom, _settings.maxZoom);
            _targetZoom = zoomLevel;

            // Kill existing tween
            _zoomTween?.Kill();

            if (_mainCamera == null)
                return;

            // Calculate target orthographic size
            float baseSize = _settings.orthographicSize;
            float targetSize = baseSize / zoomLevel;

            // Animate zoom using DOTween
            if (duration > 0)
            {
                _zoomTween = DOTween.To(
                    () => _mainCamera.orthographicSize,
                    x => _mainCamera.orthographicSize = x,
                    targetSize,
                    duration
                ).SetEase(Ease.OutCubic);
            }
            else
            {
                _mainCamera.orthographicSize = targetSize;
            }

            _currentZoom = zoomLevel;
        }

        public void Update(float deltaTime)
        {
            // Smooth zoom update if needed
            // Currently handled by DOTween
        }

        public float GetCurrentZoom() => _currentZoom;

        public void Dispose()
        {
            _zoomTween?.Kill();
            _zoomTween = null;
            _virtualCamera = null;
            _mainCamera = null;
        }
    }
}
