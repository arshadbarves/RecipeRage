using System;
using RecipeRage.Core.Input;
using UnityEngine;

namespace RecipeRage.Core.Player
{
    /// <summary>
    /// Controls player movement and interactions.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 10f;
        [SerializeField] private float _interactionDistance = 2f;
        
        [Header("References")]
        [SerializeField] private Transform _modelTransform;
        
        /// <summary>
        /// Event triggered when the player interacts with something.
        /// </summary>
        public event Action OnInteraction;
        
        /// <summary>
        /// Event triggered when the player uses their special ability.
        /// </summary>
        public event Action OnSpecialAbility;
        
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
        /// Subscribe to input events.
        /// </summary>
        private void OnEnable()
        {
            // Subscribe to input events
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnMovementInput += HandleMovementInput;
                InputManager.Instance.OnInteractionInput += HandleInteractionInput;
                InputManager.Instance.OnSpecialAbilityInput += HandleSpecialAbilityInput;
            }
        }
        
        /// <summary>
        /// Unsubscribe from input events.
        /// </summary>
        private void OnDisable()
        {
            // Unsubscribe from input events
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnMovementInput -= HandleMovementInput;
                InputManager.Instance.OnInteractionInput -= HandleInteractionInput;
                InputManager.Instance.OnSpecialAbilityInput -= HandleSpecialAbilityInput;
            }
        }
        
        /// <summary>
        /// Update the player controller.
        /// </summary>
        private void Update()
        {
            // Check for interactable objects
            CheckForInteractables();
        }
        
        /// <summary>
        /// Update physics-based movement.
        /// </summary>
        private void FixedUpdate()
        {
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
                _rigidbody.velocity = _velocity;
                return;
            }
            
            // Convert input to world space movement
            Vector3 movement = new Vector3(_movementInput.x, 0f, _movementInput.y);
            
            // Calculate target velocity
            Vector3 targetVelocity = movement * _moveSpeed;
            
            // Smoothly interpolate to target velocity
            _velocity = Vector3.Lerp(_velocity, targetVelocity, Time.fixedDeltaTime * 10f);
            
            // Apply velocity
            _rigidbody.velocity = _velocity;
            
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
                if (hit.collider.gameObject.GetComponent<IInteractable>() != null)
                {
                    // Set current interactable
                    _currentInteractable = hit.collider.gameObject;
                    
                    // TODO: Show interaction prompt
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
            
            // TODO: Implement special ability logic
            
            // Trigger special ability event
            OnSpecialAbility?.Invoke();
            
            Debug.Log("[PlayerController] Used special ability");
            
            // Reset using special ability flag
            _isUsingSpecialAbility = false;
        }
        
        /// <summary>
        /// Set the player's position.
        /// </summary>
        /// <param name="position">The new position</param>
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }
        
        /// <summary>
        /// Set the player's rotation.
        /// </summary>
        /// <param name="rotation">The new rotation</param>
        public void SetRotation(Quaternion rotation)
        {
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
    }
}
