using Core.Input;
using Gameplay.Interfaces;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using Gameplay.Camera;

namespace Gameplay.Character.Controller
{
    public class PlayerController : BaseCharacter
    {
        [Header("Player Settings")]
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private LayerMask interactableLayers;
        [SerializeField] private ParticleSystem interactionEffect;

        // Interaction state
        private IInteractable _currentInteractable;
        private RaycastHit _hitInfo;

        // Input handling   
        private InputManager _inputManager;
        private Ray _interactionRay;

        // Components
        private CameraController _cameraController;

        [Inject]
        public void Construct(InputManager inputManager, CameraController cameraController)
        {
            _inputManager = inputManager;
            _cameraController = cameraController;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner)
            {
                RegisterInputCallbacks();
                InitializeCamera();
            }
        }

        private void InitializeCamera()
        {
            if (_cameraController != null)
            {
                _cameraController.SetTarget(this);
            }
        }

        protected override void Update()
        {
            base.Update();

            if (!IsOwner) return;

            UpdateMovement();
            UpdateInteraction();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }

        private void UpdateMovement()
        {
            if (CharacterStats.IsStunned) return;

            GroundedPlayer = CharacterController.isGrounded;
            if (GroundedPlayer && PlayerVelocity.y < 0)
            {
                PlayerVelocity.y = 0f;
            }

            MoveInput = _inputManager.GetMovementInput();

            Vector3 movement = new Vector3(MoveInput.x, 0, MoveInput.y) * CharacterStats.GetMovementSpeed();
            
            // Calculate movement direction relative to camera
            if (_cameraController != null && movement != Vector3.zero)
            {
                // Get the camera's forward and right vectors
                UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
                if (mainCamera != null)
                {
                    Vector3 cameraForward = mainCamera.transform.forward;
                    Vector3 cameraRight = mainCamera.transform.right;

                    // Project vectors onto the horizontal plane
                    cameraForward.y = 0;
                    cameraRight.y = 0;
                    cameraForward.Normalize();
                    cameraRight.Normalize();

                    // Calculate the movement direction relative to camera
                    movement = cameraRight * MoveInput.x + cameraForward * MoveInput.y;
                    movement = movement.normalized * CharacterStats.GetMovementSpeed();
                }
            }

            // Apply movement
            CharacterController.Move(movement * Time.deltaTime);

            // Update rotation to face movement direction
            if (movement != Vector3.zero)
            {
                transform.forward = Vector3.Slerp(transform.forward, movement.normalized, Time.deltaTime * RotationSpeed);
            }

            // Apply gravity
            PlayerVelocity.y += GravityValue * Time.deltaTime;
            CharacterController.Move(PlayerVelocity * Time.deltaTime);

            // Update animation parameters if needed
            if (NetworkAnimator != null)
            {
                NetworkAnimator.Animator.SetFloat("Speed", movement.magnitude);
            }
        }

        private void UpdateInteraction()
        {
            if (CharacterStats.IsStunned || !IsInteracting || _currentInteractable == null) return;

            _interactionRay = new Ray(interactionSource.position, interactionSource.forward);

            if (Physics.Raycast(_interactionRay, out _hitInfo, interactionRange, interactableLayers))
            {
                if (_hitInfo.collider.TryGetComponent(out IInteractable interactable) &&
                    interactable.CanInteract(this))
                {
                    _currentInteractable = interactable;
                }
            }
            else
            {
                _currentInteractable = null;
            }
        }

        private void HandleInteractInput()
        {
            if (CharacterStats.IsStunned || _currentInteractable == null) return;

            StartInteractionServerRpc(_currentInteractable.GetNetworkObjectId());
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                UnregisterInputCallbacks();
            }

            base.OnNetworkDespawn();
        }

        private void RegisterInputCallbacks()
        {
            _inputManager.OnInteract += HandleInteractInput;
        }

        private void UnregisterInputCallbacks()
        {
            _inputManager.OnInteract -= HandleInteractInput;
        }

        #region RPCs

        [ServerRpc]
        private void StartInteractionServerRpc(ulong interactableId)
        {
            if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(interactableId, out NetworkObject netObj))
            {
                if (netObj.TryGetComponent(out IInteractable interactable) &&
                    interactable.CanInteract(this))
                {
                    interactable.OnInteractionStarted(this);
                }
            }
        }

        #endregion
    }
}