using System;
using RecipeRage.Core.Input;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Core.Player
{
    /// <summary>
    /// Controls player movement and interactions.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : NetworkBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 10f;
        [SerializeField] private float _interactionDistance = 2f;

        [Header("References")]
        [SerializeField] private Transform _modelTransform;
        [SerializeField] private Transform _itemHoldPoint;

        /// <summary>
        /// Event triggered when the player interacts with something.
        /// </summary>
        public event Action OnInteraction;

        /// <summary>
        /// Event triggered when the player uses their special ability.
        /// </summary>
        public event Action OnSpecialAbility;

        /// <summary>
        /// Event triggered when the player picks up an item.
        /// </summary>
        public event Action<NetworkObject> OnItemPickedUp;

        /// <summary>
        /// Event triggered when the player drops an item.
        /// </summary>
        public event Action<NetworkObject> OnItemDropped;

        /// <summary>
        /// The player's rigidbody component.
        /// </summary>
        private Rigidbody _rigidbody;

        /// <summary>
        /// The current movement input.
        /// </summary>
        private Vector2 _movementInput;

        /// <summary>
        /// Flag to track if the player is currently moving.
        /// </summary>
        private bool _isMoving;

        /// <summary>
        /// The player's current velocity.
        /// </summary>
        private Vector3 _velocity;

        /// <summary>
        /// The player's current facing direction.
        /// </summary>
        private Vector3 _facingDirection;

        /// <summary>
        /// Flag to track if the player is currently interacting.
        /// </summary>
        private bool _isInteracting;

        /// <summary>
        /// Flag to track if the player is currently using their special ability.
        /// </summary>
        private bool _isUsingSpecialAbility;

        /// <summary>
        /// The object the player is currently interacting with.
        /// </summary>
        private GameObject _currentInteractable;

        /// <summary>
        /// The item the player is currently holding.
        /// </summary>
        private NetworkVariable<NetworkObjectReference> _heldItem = new NetworkVariable<NetworkObjectReference>();

        /// <summary>
        /// Initialize the player controller.
        /// </summary>
        private void Awake()
        {
            // Get components
            _rigidbody = GetComponent<Rigidbody>();

            // Ensure model transform is set
            if (_modelTransform == null)
            {
                _modelTransform = transform;
            }

            // Ensure item hold point is set
            if (_itemHoldPoint == null)
            {
                _itemHoldPoint = transform;
            }

            // Initialize values
            _movementInput = Vector2.zero;
            _isMoving = false;
            _velocity = Vector3.zero;
            _facingDirection = transform.forward;
            _isInteracting = false;
            _isUsingSpecialAbility = false;
            _currentInteractable = null;
        }

        /// <summary>
        /// Subscribe to events when the network object is spawned.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            // Subscribe to held item changes
            _heldItem.OnValueChanged += OnHeldItemChanged;

            // Only subscribe to input events if this is the local player
            if (IsLocalPlayer)
            {
                // Subscribe to input events
                if (InputManager.Instance != null)
                {
                    InputManager.Instance.OnMovementInput += HandleMovementInput;
                    InputManager.Instance.OnInteractionInput += HandleInteractionInput;
                    InputManager.Instance.OnSpecialAbilityInput += HandleSpecialAbilityInput;
                }
            }
        }

        /// <summary>
        /// Unsubscribe from events when the network object is despawned.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            // Unsubscribe from held item changes
            _heldItem.OnValueChanged -= OnHeldItemChanged;

            // Only unsubscribe from input events if this is the local player
            if (IsLocalPlayer)
            {
                // Unsubscribe from input events
                if (InputManager.Instance != null)
                {
                    InputManager.Instance.OnMovementInput -= HandleMovementInput;
                    InputManager.Instance.OnInteractionInput -= HandleInteractionInput;
                    InputManager.Instance.OnSpecialAbilityInput -= HandleSpecialAbilityInput;
                }
            }
        }

        /// <summary>
        /// Update the player controller.
        /// </summary>
        private void Update()
        {
            // Only process input for the local player
            if (!IsLocalPlayer)
            {
                return;
            }

            // Check for interactable objects
            CheckForInteractables();
        }

        /// <summary>
        /// Update physics-based movement.
        /// </summary>
        private void FixedUpdate()
        {
            // Only process movement for the local player
            if (!IsLocalPlayer)
            {
                return;
            }

            // Move the player
            MovePlayer();

            // Rotate the player
            RotatePlayer();
        }

        /// <summary>
        /// Handle movement input from the input manager.
        /// </summary>
        /// <param name="movementInput">The movement input vector</param>
        private void HandleMovementInput(Vector2 movementInput)
        {
            _movementInput = movementInput;
            _isMoving = _movementInput.sqrMagnitude > 0.01f;
        }

        /// <summary>
        /// Handle interaction input from the input manager.
        /// </summary>
        private void HandleInteractionInput()
        {
            // Trigger interaction
            TryInteract();
        }

        /// <summary>
        /// Handle special ability input from the input manager.
        /// </summary>
        private void HandleSpecialAbilityInput()
        {
            // Trigger special ability
            TryUseSpecialAbility();
        }

        /// <summary>
        /// Move the player based on input.
        /// </summary>
        private void MovePlayer()
        {
            if (!_isMoving)
            {
                // Apply friction to slow down when not moving
                _velocity = Vector3.Lerp(_velocity, Vector3.zero, Time.fixedDeltaTime * 10f);

                // Apply velocity
                _rigidbody.linearVelocity = _velocity;
                return;
            }

            // Convert input to world space movement
            Vector3 movement = new Vector3(_movementInput.x, 0f, _movementInput.y);

            // Calculate target velocity
            Vector3 targetVelocity = movement * _moveSpeed;

            // Smoothly interpolate to target velocity
            _velocity = Vector3.Lerp(_velocity, targetVelocity, Time.fixedDeltaTime * 10f);

            // Apply velocity
            _rigidbody.linearVelocity = _velocity;

            // Update facing direction if moving
            if (movement.sqrMagnitude > 0.01f)
            {
                _facingDirection = movement.normalized;
            }
        }

        /// <summary>
        /// Rotate the player to face the movement direction.
        /// </summary>
        private void RotatePlayer()
        {
            if (!_isMoving)
            {
                return;
            }

            // Calculate target rotation
            Quaternion targetRotation = Quaternion.LookRotation(_facingDirection);

            // Smoothly rotate towards target
            _modelTransform.rotation = Quaternion.Slerp(_modelTransform.rotation, targetRotation, Time.fixedDeltaTime * _rotationSpeed);
        }

        /// <summary>
        /// Check for interactable objects in front of the player.
        /// </summary>
        private void CheckForInteractables()
        {
            // Cast a ray in front of the player
            Ray ray = new Ray(transform.position, _facingDirection);
            RaycastHit hit;

            // Check if ray hits an interactable object
            if (Physics.Raycast(ray, out hit, _interactionDistance))
            {
                // Check if the hit object has an interactable component
                IInteractable interactable = hit.collider.gameObject.GetComponent<IInteractable>();
                if (interactable != null && interactable.CanInteract(gameObject))
                {
                    // Set current interactable
                    _currentInteractable = hit.collider.gameObject;

                    // TODO: Show interaction prompt with interactable.GetInteractionPrompt()
                }
                else
                {
                    // Clear current interactable
                    _currentInteractable = null;

                    // TODO: Hide interaction prompt
                }
            }
            else
            {
                // Clear current interactable
                _currentInteractable = null;

                // TODO: Hide interaction prompt
            }
        }

        /// <summary>
        /// Try to interact with the current interactable object.
        /// </summary>
        private void TryInteract()
        {
            if (_isInteracting || _currentInteractable == null)
            {
                return;
            }

            // Set interacting flag
            _isInteracting = true;

            // Get interactable component
            IInteractable interactable = _currentInteractable.GetComponent<IInteractable>();

            // Interact with the object
            if (interactable != null)
            {
                interactable.Interact(gameObject);

                // Trigger interaction event
                OnInteraction?.Invoke();

                Debug.Log($"[PlayerController] Interacted with {_currentInteractable.name}");
            }

            // Reset interacting flag
            _isInteracting = false;
        }

        /// <summary>
        /// Try to use the player's special ability.
        /// </summary>
        private void TryUseSpecialAbility()
        {
            if (_isUsingSpecialAbility)
            {
                return;
            }

            // Set using special ability flag
            _isUsingSpecialAbility = true;

            // Request to use special ability via RPC
            UseSpecialAbilityServerRpc();

            // Reset using special ability flag
            _isUsingSpecialAbility = false;
        }

        /// <summary>
        /// Handle changes to the held item.
        /// </summary>
        /// <param name="previousValue">The previous value.</param>
        /// <param name="newValue">The new value.</param>
        private void OnHeldItemChanged(NetworkObjectReference previousValue, NetworkObjectReference newValue)
        {
            // Check if the previous item exists
            if (previousValue.TryGet(out NetworkObject previousItem))
            {
                // Trigger item dropped event
                OnItemDropped?.Invoke(previousItem);
            }

            // Check if the new item exists
            if (newValue.TryGet(out NetworkObject newItem))
            {
                // Position the item at the hold point
                if (_itemHoldPoint != null)
                {
                    newItem.transform.position = _itemHoldPoint.position;
                    newItem.transform.rotation = _itemHoldPoint.rotation;
                    newItem.transform.SetParent(_itemHoldPoint);
                }

                // Trigger item picked up event
                OnItemPickedUp?.Invoke(newItem);
            }
        }

        /// <summary>
        /// Try to pick up an item.
        /// </summary>
        /// <param name="item">The item to pick up.</param>
        /// <returns>True if the item was picked up successfully, false otherwise.</returns>
        public bool TryPickUpItem(NetworkObject item)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[PlayerController] Only the server can pick up items.");
                return false;
            }

            // Check if the player is already holding an item
            if (_heldItem.Value.TryGet(out _))
            {
                return false;
            }

            // Set the held item
            _heldItem.Value = item;

            return true;
        }

        /// <summary>
        /// Drop the currently held item.
        /// </summary>
        /// <returns>The dropped item, or null if no item was held.</returns>
        public NetworkObject DropItem()
        {
            if (!IsServer)
            {
                Debug.LogWarning("[PlayerController] Only the server can drop items.");
                return null;
            }

            // Check if the player is holding an item
            if (!_heldItem.Value.TryGet(out NetworkObject heldItem))
            {
                return null;
            }

            // Unparent the item
            heldItem.transform.SetParent(null);

            // Position the item in front of the player
            heldItem.transform.position = transform.position + _facingDirection * 1.0f + Vector3.up * 0.5f;

            // Clear the held item
            _heldItem.Value = new NetworkObjectReference();

            return heldItem;
        }

        /// <summary>
        /// Get the currently held item.
        /// </summary>
        /// <returns>The held item, or null if no item is held.</returns>
        public NetworkObject GetHeldItem()
        {
            if (_heldItem.Value.TryGet(out NetworkObject heldItem))
            {
                return heldItem;
            }

            return null;
        }

        /// <summary>
        /// Check if the player is holding an item.
        /// </summary>
        /// <returns>True if the player is holding an item, false otherwise.</returns>
        public bool IsHoldingItem()
        {
            return _heldItem.Value.TryGet(out _);
        }

        /// <summary>
        /// Set the player's position.
        /// </summary>
        /// <param name="position">The new position</param>
        public void SetPosition(Vector3 position)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[PlayerController] Only the server can set player positions.");
                return;
            }

            transform.position = position;
        }

        /// <summary>
        /// Set the player's rotation.
        /// </summary>
        /// <param name="rotation">The new rotation</param>
        public void SetRotation(Quaternion rotation)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[PlayerController] Only the server can set player rotations.");
                return;
            }

            _modelTransform.rotation = rotation;
            _facingDirection = _modelTransform.forward;
        }

        /// <summary>
        /// Get the player's current velocity.
        /// </summary>
        /// <returns>The player's velocity</returns>
        public Vector3 GetVelocity()
        {
            return _velocity;
        }

        /// <summary>
        /// Get the player's current facing direction.
        /// </summary>
        /// <returns>The player's facing direction</returns>
        public Vector3 GetFacingDirection()
        {
            return _facingDirection;
        }

        /// <summary>
        /// Check if the player is currently moving.
        /// </summary>
        /// <returns>True if the player is moving</returns>
        public bool IsMoving()
        {
            return _isMoving;
        }

        /// <summary>
        /// Request to use the player's special ability.
        /// </summary>
        [ServerRpc]
        private void UseSpecialAbilityServerRpc()
        {
            // TODO: Implement special ability logic

            // Notify all clients that the special ability was used
            UseSpecialAbilityClientRpc();
        }

        /// <summary>
        /// Notify clients that the player used their special ability.
        /// </summary>
        [ClientRpc]
        private void UseSpecialAbilityClientRpc()
        {
            // Trigger special ability event
            OnSpecialAbility?.Invoke();

            Debug.Log("[PlayerController] Used special ability");
        }
    }
}
