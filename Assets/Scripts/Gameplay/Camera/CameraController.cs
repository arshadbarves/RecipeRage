using System;
using Core.Logging;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

namespace Gameplay.Camera
{
    public class CameraController : ICameraController
    {
        private readonly CameraSettings _settings;

        private GameObject _cameraRig;
        private GameObject _virtualCameraObj;
        private UnityEngine.Camera _mainCamera;
        private CinemachineCamera _virtualCamera;
        private CinemachineBasicMultiChannelPerlin _noise;
        private CinemachineConfiner3D _confiner;
        private BoxCollider _boundsCollider;

        private float _shakeTimer;
        private float _shakeDuration;
        private float _shakeIntensity;
        private Tween _zoomTween;
        private bool _isInitialized;

        public UnityEngine.Camera MainCamera => _mainCamera;
        public bool IsInitialized => _isInitialized;

        public CameraController(CameraSettings settings = null)
        {
            _settings = settings ?? CameraSettings.CreateDefault();
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                GameLogger.LogWarning("CameraController already initialized");
                return;
            }

            _cameraRig = new GameObject("GameplayCamera") { tag = "MainCamera" };

            _mainCamera = _cameraRig.AddComponent<UnityEngine.Camera>();
            _mainCamera.orthographic = _settings.useOrthographic;
            _mainCamera.orthographicSize = _settings.orthographicSize;
            _mainCamera.fieldOfView = _settings.fieldOfView;
            _mainCamera.nearClipPlane = 0.1f;
            _mainCamera.farClipPlane = 1000f;

            var brain = _cameraRig.AddComponent<CinemachineBrain>();
            brain.DefaultBlend.Time = 0.3f;
            brain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.EaseInOut;

            _cameraRig.transform.position = new Vector3(0, _settings.cameraHeight, 0);
            _cameraRig.transform.rotation = Quaternion.Euler(90f, 0, 0);

            _virtualCameraObj = new GameObject("VirtualCamera");
            _virtualCameraObj.transform.position = Vector3.zero;
            _virtualCameraObj.transform.rotation = Quaternion.identity;

            _virtualCamera = _virtualCameraObj.AddComponent<CinemachineCamera>();
            _virtualCamera.Priority.Value = 10;

            var follow = _virtualCameraObj.AddComponent<CinemachineFollow>();
            follow.FollowOffset = new Vector3(0, _settings.cameraHeight, 0);
            follow.TrackerSettings.PositionDamping = new Vector3(_settings.followSmoothTime, _settings.followSmoothTime, _settings.followSmoothTime);

            _noise = _virtualCameraObj.AddComponent<CinemachineBasicMultiChannelPerlin>();
            _noise.FrequencyGain = _settings.shakeFrequency;
            _noise.AmplitudeGain = 0f;

            if (_settings.enableBounds)
            {
                _confiner = _virtualCameraObj.AddComponent<CinemachineConfiner3D>();
            }

            _isInitialized = true;
            GameLogger.Log("CameraController initialized");
        }

        public void AddFollowTarget(Transform target, float weight = 1f, float radius = 1f)
        {
            SetFollowTarget(target);
        }

        public void RemoveFollowTarget(Transform target)
        {
            ClearFollowTarget();
        }

        public void SetFollowTarget(Transform target)
        {
            if (!_isInitialized || _virtualCamera == null) return;

            _virtualCamera.Follow = target;
            _virtualCamera.LookAt = target;
        }

        public void ClearFollowTarget()
        {
            if (_virtualCamera == null) return;

            _virtualCamera.Follow = null;
            _virtualCamera.LookAt = null;
        }

        public void PositionForArena(Bounds arenaBounds)
        {
            if (!_isInitialized) return;

            Vector3 arenaCenter = arenaBounds.center;
            Vector3 arenaSize = arenaBounds.size;

            float distance = Mathf.Max(arenaSize.x, arenaSize.z) * 0.35f + _settings.cameraDistance;
            Vector3 cameraPos = arenaCenter + new Vector3(0, _settings.cameraHeight, -distance);

            _cameraRig.transform.position = cameraPos;
            _cameraRig.transform.LookAt(arenaCenter);
        }

        public void SetArenaBounds(Bounds bounds)
        {
            if (!_settings.enableBounds || _confiner == null) return;

            if (_boundsCollider != null)
            {
                UnityEngine.Object.Destroy(_boundsCollider.gameObject);
            }

            var boundsObj = new GameObject("CameraBounds");
            boundsObj.transform.position = bounds.center;

            _boundsCollider = boundsObj.AddComponent<BoxCollider>();
            _boundsCollider.size = bounds.size - Vector3.one * _settings.boundsPadding;
            _boundsCollider.isTrigger = true;

            _confiner.BoundingVolume = _boundsCollider;
        }

        public void Shake(float intensity, float duration)
        {
            if (_noise == null) return;

            intensity = Mathf.Clamp01(intensity);
            _shakeIntensity = intensity * _settings.maxShakeIntensity;
            _shakeDuration = duration;
            _shakeTimer = 0f;
            _noise.AmplitudeGain = _shakeIntensity;
        }

        public void SetZoom(float zoomLevel, float duration = 0.3f)
        {
            if (_mainCamera == null) return;

            zoomLevel = Mathf.Clamp(zoomLevel, _settings.minZoom, _settings.maxZoom);
            float targetSize = _settings.orthographicSize / zoomLevel;

            _zoomTween?.Kill();

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
        }

        public void SetFollowEnabled(bool enabled)
        {
            if (_virtualCamera != null)
            {
                _virtualCamera.enabled = enabled;
            }
        }

        public void Update(float deltaTime)
        {
            if (!_isInitialized) return;

            if (_shakeTimer < _shakeDuration)
            {
                _shakeTimer += deltaTime;
                float progress = _shakeTimer / _shakeDuration;
                float currentIntensity = Mathf.Lerp(_shakeIntensity, 0f, progress);

                if (_noise != null)
                {
                    _noise.AmplitudeGain = currentIntensity;
                }
            }
            else if (_noise != null && _noise.AmplitudeGain > 0f)
            {
                _noise.AmplitudeGain = 0f;
            }
        }

        public void Dispose()
        {
            _zoomTween?.Kill();

            if (_cameraRig != null)
            {
                UnityEngine.Object.Destroy(_cameraRig);
            }

            if (_virtualCameraObj != null)
            {
                UnityEngine.Object.Destroy(_virtualCameraObj);
            }

            if (_boundsCollider != null)
            {
                UnityEngine.Object.Destroy(_boundsCollider.gameObject);
            }

            _cameraRig = null;
            _virtualCameraObj = null;
            _mainCamera = null;
            _virtualCamera = null;
            _noise = null;
            _confiner = null;
            _boundsCollider = null;
            _zoomTween = null;
            _isInitialized = false;
        }
    }
}
