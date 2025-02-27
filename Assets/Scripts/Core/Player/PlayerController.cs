using UnityEngine;
using System;
using RecipeRage.Core.Input;
using RecipeRage.Core.Interaction;
using Unity.Netcode;

namespace RecipeRage.Core.Player
{
    /// <summary>
    /// Handles player movement and interactions
    /// </summary>
    public class PlayerController : NetworkBehaviour
    {
        #region Events
        public event Action<Vector3> OnPlayerMoved;
        public event Action<GameObject> OnItemPickedUp;
        public event Action<GameObject> OnItemDropped;
        public event Action OnInteractionStarted;
        public event Action OnInteractionCompleted;
        public event Action OnInteractionCanceled;
        #endregion

        #region Properties
        public bool CanMove { get; private set; } = true;
        public bool IsInteracting { get; private set; }
        public GameObject HeldItem { get; set; }
        public float CurrentSpeed => _baseSpeed * _speedMultiplier;
        #endregion

        #region Serialized Fields
        [Header("Movement Settings")]
        [SerializeField] private float _baseSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 360f;
        
        [Header("Interaction Settings")]
        [SerializeField] private Transform _holdPoint;
        [SerializeField] private float _interactionRange = 2f;
        [SerializeField] private LayerMask _interactionLayer;
        [SerializeField] private Transform _interactionPoint;
        
        [Header("References")]
        [SerializeField] private InputManager _inputManager;
        [SerializeField] private Animator _animator;
        #endregion

        #region Private Fields
        private Rigidbody _rb;
        private float _speedMultiplier = 1f;
        private Vector2 _currentMovementInput;
        private IInteractable _currentInteractable;
        private RaycastHit[] _raycastHits = new RaycastHit[4]; // Pool for raycast hits
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            
            if (_inputManager == null)
                _inputManager = GetComponent<InputManager>();
        }

        private void OnEnable()
        {
            if (_inputManager != null)
            {
                _inputManager.OnMovementInput += HandleMovementInput;
                _inputManager.OnInteractionStarted += HandleInteractionStarted;
                _inputManager.OnInteractionCanceled += HandleInteractionCanceled;
            }
        }

        private void OnDisable()
        {
            if (_inputManager != null)
            {
                _inputManager.OnMovementInput -= HandleMovementInput;
                _inputManager.OnInteractionStarted -= HandleInteractionStarted;
                _inputManager.OnInteractionCanceled -= HandleInteractionCanceled;
            }
        }

        private void FixedUpdate()
        {
            if (CanMove)
            {
                HandleMovement();
            }
        }

        private void Update()
        {
            if (CanMove)
            {
                CheckForInteractables();
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Applies a speed modifier to the player
        /// </summary>
        /// <param name="multiplier">Speed multiplier</param>
        /// <param name="duration">Duration in seconds (0 for permanent)</param>
        [ServerRpc]
        public void ApplySpeedModifierServerRpc(float multiplier, float duration = 0)
        {
            _speedMultiplier = multiplier;
            
            if (duration > 0)
            {
                Invoke(nameof(ResetSpeedModifier), duration);
            }
        }

        /// <summary>
        /// Enables or disables player movement
        /// </summary>
        /// <param name="canMove">Whether the player can move</param>
        public void SetMovementState(bool canMove)
        {
            CanMove = canMove;
            if (!canMove)
            {
                _rb.linearVelocity = Vector3.zero;
                UpdateAnimator(Vector3.zero);
            }
        }
        #endregion

        #region Private Methods
        private void HandleMovementInput(Vector2 input)
        {
            _currentMovementInput = input;
        }

        private void HandleMovement()
        {
            Vector3 movement = new Vector3(_currentMovementInput.x, 0f, _currentMovementInput.y);
            
            if (movement != Vector3.zero)
            {
                // Rotate towards movement direction
                Quaternion targetRotation = Quaternion.LookRotation(movement);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    _rotationSpeed * Time.deltaTime
                );
                
                // Move
                _rb.MovePosition(transform.position + movement * (CurrentSpeed * Time.deltaTime));
                OnPlayerMoved?.Invoke(movement);
            }
            
            UpdateAnimator(movement);
        }

        private void CheckForInteractables()
        {
            // Cast a ray forward from the interaction point
            int hitCount = Physics.RaycastNonAlloc(
                _interactionPoint.position,
                _interactionPoint.forward,
                _raycastHits,
                _interactionRange,
                _interactionLayer
            );

            // Find the closest interactable
            float closestDistance = float.MaxValue;
            IInteractable closestInteractable = null;

            for (int i = 0; i < hitCount; i++)
            {
                var interactable = _raycastHits[i].collider.GetComponent<IInteractable>();
                if (interactable != null && interactable.CanInteract)
                {
                    float distance = Vector3.Distance(transform.position, _raycastHits[i].point);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestInteractable = interactable;
                    }
                }
            }

            // Update current interactable
            if (_currentInteractable != closestInteractable)
            {
                _currentInteractable = closestInteractable;
                // TODO: Highlight the current interactable
            }
        }

        private void HandleInteractionStarted()
        {
            if (IsInteracting || _currentInteractable == null)
                return;

            if (_currentInteractable.InteractionType == InteractionType.Container || 
                _currentInteractable.InteractionType == InteractionType.Plate)
            {
                HandlePickup();
            }
            else if (HeldItem != null)
            {
                DropItem();
            }
            else
            {
                StartInteraction();
            }
        }

        private void HandleInteractionCanceled()
        {
            if (IsInteracting && _currentInteractable != null)
            {
                _currentInteractable.CancelInteraction(this);
                IsInteracting = false;
                OnInteractionCanceled?.Invoke();
            }
        }

        private void StartInteraction()
        {
            if (_currentInteractable.StartInteraction(this, () =>
            {
                IsInteracting = false;
                OnInteractionCompleted?.Invoke();
            }))
            {
                IsInteracting = true;
                OnInteractionStarted?.Invoke();
            }
        }

        private void HandlePickup()
        {
            if (HeldItem != null)
            {
                // Only allow dropping in containers
                if (_currentInteractable != null && _currentInteractable.InteractionType == InteractionType.Container)
                {
                    StartInteraction();
                }
                // Ignore drop request if not at a container
            }
            else
            {
                var pickupable = _currentInteractable as IPickupable;
                if (pickupable != null)
                {
                    PickupItem(pickupable.GetGameObject());
                }
            }
        }

        public void PickupItem(GameObject item)
        {
            if (HeldItem != null || item == null)
                return;

            HeldItem = item;
            item.transform.SetParent(_holdPoint);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            
            OnItemPickedUp?.Invoke(item);
        }

        private void DropItem()
        {
            if (HeldItem == null)
                return;

            HeldItem.transform.SetParent(null);
            OnItemDropped?.Invoke(HeldItem);
            HeldItem = null;
        }

        private void UpdateAnimator(Vector3 movement)
        {
            if (_animator != null)
            {
                _animator.SetFloat("Speed", movement.magnitude);
                _animator.SetBool("IsHolding", HeldItem != null);
                _animator.SetBool("IsInteracting", IsInteracting);
            }
        }

        private void ResetSpeedModifier()
        {
            _speedMultiplier = 1f;
        }
        #endregion
    }

    /// <summary>
    /// Interface for objects that can be picked up
    /// </summary>
    public interface IPickupable
    {
        GameObject GetGameObject();
    }
} 