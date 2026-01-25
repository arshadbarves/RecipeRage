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
            _mainCamera.orthographic = false;
            _mainCamera.fieldOfView = _settings.fieldOfView;
            _mainCamera.nearClipPlane = 0.1f;
            _mainCamera.farClipPlane = 1000f;

            var brain = _cameraRig.AddComponent<CinemachineBrain>();
            brain.DefaultBlend.Time = 0.5f;
            brain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.EaseInOut;

            _virtualCameraObj = new GameObject("VirtualCamera");
            _virtualCameraObj.transform.rotation = Quaternion.Euler(_settings.cameraAngle, 0, 0);

            _virtualCamera = _virtualCameraObj.AddComponent<CinemachineCamera>();
            _virtualCamera.Priority.Value = 10;

            var follow = _virtualCameraObj.AddComponent<CinemachineFollow>();
            follow.FollowOffset = new Vector3(_settings.cameraSideOffset, _settings.cameraHeight, -_settings.cameraDistance);
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

        public void SetArenaBounds(Bounds bounds)
        {
        }

        public void AutoDetectBounds()
        {
            if (!_settings.enableBounds || _confiner == null) return;

            var marker = UnityEngine.Object.FindFirstObjectByType<CameraBoundsMarker>();
            if (marker != null)
            {
                var boundsCollider = marker.GetBoundsCollider();
                if (boundsCollider != null)
                {
                    _confiner.BoundingVolume = boundsCollider;
                    GameLogger.Log($"Camera bounds auto-detected from CameraBoundsMarker: {marker.gameObject.name}");
                    return;
                }
            }

            GameLogger.LogWarning("CameraBoundsMarker not found. Add CameraBoundsMarker component to a BoxCollider in your map scene.");
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

            _cameraRig = null;
            _virtualCameraObj = null;
            _mainCamera = null;
            _virtualCamera = null;
            _noise = null;
            _confiner = null;
            _zoomTween = null;
            _isInitialized = false;
        }
    }
}
