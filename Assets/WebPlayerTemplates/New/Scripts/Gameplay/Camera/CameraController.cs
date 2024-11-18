using UnityEngine;
using Cinemachine;
using Gameplay.Character.Controller;
using VContainer;

namespace Gameplay.Camera
{
    public class CameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private float rotationSpeed = 2f;
        [SerializeField] private float heightOffset = 10f;
        [SerializeField] private float distanceOffset = 7f;
        [SerializeField] private float lookAheadDistance = 2f;
        
        [Header("Damping Settings")]
        [SerializeField] private float positionDamping = 1f;
        [SerializeField] private float rotationDamping = 1f;
        
        [Header("Boundaries")]
        [SerializeField] private float minHeight = 5f;
        [SerializeField] private float maxHeight = 15f;
        [SerializeField] private float minDistance = 5f;
        [SerializeField] private float maxDistance = 10f;
        
        private Transform _targetTransform;
        private CinemachineTransposer _transposer;
        private CinemachineComposer _composer;
        private PlayerController _player;
        private Vector3 _currentVelocity;
        private Vector3 _targetPosition;
        private bool _isInitialized;

        [Inject]
        public void Construct()
        {
            // VContainer initialization if needed
        }

        private void Start()
        {
            InitializeCamera();
        }

        private void InitializeCamera()
        {
            if (!_isInitialized && virtualCamera != null)
            {
                _transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
                _composer = virtualCamera.GetCinemachineComponent<CinemachineComposer>();
                
                if (_transposer != null)
                {
                    // Set initial camera position
                    _transposer.m_FollowOffset = new Vector3(0, heightOffset, -distanceOffset);
                    _transposer.m_XDamping = positionDamping;
                    _transposer.m_YDamping = positionDamping;
                    _transposer.m_ZDamping = positionDamping;
                }

                if (_composer != null)
                {
                    _composer.m_TrackedObjectOffset = new Vector3(0, 0, lookAheadDistance);
                    _composer.m_HorizontalDamping = rotationDamping;
                    _composer.m_VerticalDamping = rotationDamping;
                }

                _isInitialized = true;
            }
        }

        public void SetTarget(PlayerController player)
        {
            if (player == null) return;
            
            _player = player;
            _targetTransform = player.transform;
            
            if (virtualCamera != null && _targetTransform != null)
            {
                virtualCamera.Follow = _targetTransform;
                virtualCamera.LookAt = _targetTransform;
            }
        }

        private void LateUpdate()
        {
            if (!_isInitialized || _targetTransform == null || _player == null) return;

            UpdateCameraPosition();
        }

        private void UpdateCameraPosition()
        {
            if (_transposer == null) return;

            // Calculate the desired camera position based on player movement
            Vector3 playerVelocity = _player.CharacterController.velocity;
            float playerSpeed = playerVelocity.magnitude;

            // Adjust look-ahead based on player speed
            if (_composer != null && playerSpeed > 0.1f)
            {
                Vector3 movementDirection = playerVelocity.normalized;
                Vector3 lookAheadOffset = movementDirection * (lookAheadDistance * (playerSpeed / 10f));
                _composer.m_TrackedObjectOffset = new Vector3(lookAheadOffset.x, 0, lookAheadOffset.z);
            }

            // Adjust camera height based on player context (optional)
            float targetHeight = Mathf.Clamp(heightOffset, minHeight, maxHeight);
            float targetDistance = Mathf.Clamp(distanceOffset, minDistance, maxDistance);

            // Update camera offset with smooth damping
            Vector3 currentOffset = _transposer.m_FollowOffset;
            Vector3 targetOffset = new Vector3(0, targetHeight, -targetDistance);
            
            _transposer.m_FollowOffset = Vector3.SmoothDamp(
                currentOffset,
                targetOffset,
                ref _currentVelocity,
                1f / followSpeed
            );
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure the virtual camera reference is set
            if (virtualCamera == null)
            {
                virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
                if (virtualCamera == null)
                {
                    Debug.LogWarning("Virtual Camera reference is missing! Please assign it in the inspector.");
                }
            }
        }
#endif
    }
}