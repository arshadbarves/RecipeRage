using System;
using Gameplay.Camera;
using Modules.Logging;
using Unity.Cinemachine;
using UnityEngine;

namespace Gameplay.Camera
{
    /// <summary>
    /// Main camera controller for Brawl Stars-like top-down gameplay.
    /// Manages Cinemachine virtual camera and all camera behaviors.
    /// Follows Single Responsibility Principle by delegating to specialized controllers.
    /// </summary>
    public class CameraController : ICameraController
    {
        private readonly CameraSettings _settings;
        private readonly CameraFollowController _followController;
        private readonly CameraBoundsController _boundsController;
        private readonly CameraZoomController _zoomController;
        private readonly CameraShakeController _shakeController;

        private GameObject _cameraRig;
        private UnityEngine.Camera _mainCamera;
        private CinemachineCamera _virtualCamera;
        private bool _isInitialized;

        public UnityEngine.Camera MainCamera => _mainCamera;
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public CameraController(CameraSettings settings = null)
        {
            _settings = settings ?? CameraSettings.CreateDefault();

            // Create specialized controllers
            _followController = new CameraFollowController(_settings);
            _boundsController = new CameraBoundsController(_settings);
            _zoomController = new CameraZoomController(_settings);
            _shakeController = new CameraShakeController(_settings);
        }

        /// <summary>
        /// Initialize the camera system
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                GameLogger.LogWarning("CameraController already initialized");
                return;
            }

            try
            {
                CreateCameraRig();
                SetupMainCamera();
                SetupVirtualCamera();

                _isInitialized = true;
                GameLogger.Log("CameraController initialized successfully");
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
                throw;
            }
        }

        /// <summary>
        /// Create the camera rig GameObject
        /// </summary>
        private void CreateCameraRig()
        {
            _cameraRig = new GameObject("GameplayCamera");
            _cameraRig.tag = "MainCamera";

            // Add main camera component
            _mainCamera = _cameraRig.AddComponent<UnityEngine.Camera>();
            _mainCamera.orthographic = _settings.useOrthographic;
            _mainCamera.orthographicSize = _settings.orthographicSize;
            _mainCamera.fieldOfView = _settings.fieldOfView;
            _mainCamera.nearClipPlane = 0.1f;
            _mainCamera.farClipPlane = 1000f;

            // Add Cinemachine brain
            var brain = _cameraRig.AddComponent<CinemachineBrain>();
            brain.DefaultBlend.Time = 0.3f;
            brain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.EaseInOut;

            GameLogger.Log("Camera rig created");
        }

        /// <summary>
        /// Setup main camera properties
        /// </summary>
        private void SetupMainCamera()
        {
            // Position camera at angle
            float angleRad = _settings.cameraAngle * Mathf.Deg2Rad;
            Vector3 offset = _settings.followOffset;

            _cameraRig.transform.position = new Vector3(0, offset.y, offset.z);
            _cameraRig.transform.rotation = Quaternion.Euler(_settings.cameraAngle, 0, 0);

            GameLogger.Log($"Main camera setup: Angle={_settings.cameraAngle}, Ortho={_settings.useOrthographic}");
        }

        /// <summary>
        /// Setup Cinemachine virtual camera
        /// </summary>
        private void SetupVirtualCamera()
        {
            var vcamObj = new GameObject("VirtualCamera");
            vcamObj.transform.SetParent(_cameraRig.transform);

            _virtualCamera = vcamObj.AddComponent<CinemachineCamera>();
            _virtualCamera.Priority.Value = 10;

            // Setup follow component
            var follow = vcamObj.AddComponent<CinemachineFollow>();
            follow.FollowOffset = _settings.followOffset;
            // Set damping for smooth following
            follow.TrackerSettings.PositionDamping = new Vector3(_settings.followSmoothTime, _settings.followSmoothTime, _settings.followSmoothTime);

            // Setup rotation component to maintain top-down angle
            var rotation = vcamObj.AddComponent<CinemachineRotationComposer>();
            rotation.Composition.ScreenPosition = new Vector2(0.5f, 0.5f);

            // Initialize controllers with virtual camera
            _followController.Initialize(_virtualCamera);
            _boundsController.Initialize(_virtualCamera);
            _zoomController.Initialize(_virtualCamera, _mainCamera);
            _shakeController.Initialize(_virtualCamera);

            GameLogger.Log("Virtual camera setup complete");
        }

        /// <summary>
        /// Set the target for the camera to follow
        /// </summary>
        public void SetFollowTarget(Transform target)
        {
            if (!_isInitialized)
            {
                GameLogger.LogError("Cannot set follow target - camera not initialized");
                return;
            }

            _followController.SetTarget(target);
            GameLogger.Log($"Camera follow target set: {target?.name ?? "null"}");
        }

        /// <summary>
        /// Clear the current follow target
        /// </summary>
        public void ClearFollowTarget()
        {
            _followController.ClearTarget();
            GameLogger.Log("Camera follow target cleared");
        }

        /// <summary>
        /// Set the arena bounds to constrain camera movement
        /// </summary>
        public void SetArenaBounds(Bounds bounds)
        {
            _boundsController.SetBounds(bounds);
            GameLogger.Log($"Camera bounds set: {bounds}");
        }

        /// <summary>
        /// Trigger a camera shake effect
        /// </summary>
        public void Shake(float intensity, float duration)
        {
            _shakeController.Shake(intensity, duration);
        }

        /// <summary>
        /// Set camera zoom level
        /// </summary>
        public void SetZoom(float zoomLevel, float duration = 0.3f)
        {
            _zoomController.SetZoom(zoomLevel, duration);
        }

        /// <summary>
        /// Enable or disable camera following
        /// </summary>
        public void SetFollowEnabled(bool enabled)
        {
            _followController.SetEnabled(enabled);
        }

        /// <summary>
        /// Update camera (called from GameplayState if needed)
        /// </summary>
        public void Update(float deltaTime)
        {
            if (!_isInitialized) return;

            _shakeController.Update(deltaTime);
            _zoomController.Update(deltaTime);
        }

        /// <summary>
        /// Clean up resources
        /// </summary>
        public void Dispose()
        {
            if (_cameraRig != null)
            {
                UnityEngine.Object.Destroy(_cameraRig);
                _cameraRig = null;
            }

            _followController?.Dispose();
            _boundsController?.Dispose();
            _zoomController?.Dispose();
            _shakeController?.Dispose();

            _mainCamera = null;
            _virtualCamera = null;
            _isInitialized = false;

            GameLogger.Log("CameraController disposed");
        }
    }
}
