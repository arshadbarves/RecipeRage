using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Gameplay.Camera
{
    /// <summary>
    /// Handles camera shake effects for impact feedback.
    /// Uses Cinemachine's built-in noise for performant shake.
    /// Follows Single Responsibility Principle - only manages shake effects.
    /// </summary>
    public class CameraShakeController : IDisposable
    {
        private readonly CameraSettings _settings;
        private CinemachineCamera _virtualCamera;
        private CinemachineBasicMultiChannelPerlin _noise;
        private float _shakeTimer;
        private float _shakeDuration;
        private float _shakeIntensity;

        public CameraShakeController(CameraSettings settings)
        {
            _settings = settings;
        }

        public void Initialize(CinemachineCamera virtualCamera)
        {
            _virtualCamera = virtualCamera;
            SetupNoise();
        }

        private void SetupNoise()
        {
            // Add noise component for shake
            _noise = _virtualCamera.gameObject.AddComponent<CinemachineBasicMultiChannelPerlin>();
            _noise.FrequencyGain = _settings.shakeFrequency;
            _noise.AmplitudeGain = 0f; // Start with no shake
        }

        public void Shake(float intensity, float duration)
        {
            if (_noise == null)
                return;

            // Clamp intensity
            intensity = Mathf.Clamp01(intensity);

            _shakeIntensity = intensity * _settings.maxShakeIntensity;
            _shakeDuration = duration;
            _shakeTimer = 0f;

            _noise.AmplitudeGain = _shakeIntensity;
        }

        public void Update(float deltaTime)
        {
            if (_shakeTimer < _shakeDuration)
            {
                _shakeTimer += deltaTime;

                // Fade out shake
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

        public bool IsShaking() => _shakeTimer < _shakeDuration;

        public void Dispose()
        {
            if (_noise != null)
            {
                UnityEngine.Object.Destroy(_noise);
                _noise = null;
            }

            _virtualCamera = null;
        }
    }
}
