using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Input
{
    /// <summary>
    /// Input provider that uses the Unity Input System package.
    /// </summary>
    public class InputSystemProvider : IInputProvider, PlayerInputActions.IPlayerActions
    {
        /// <summary>
        /// Event triggered when movement input changes.
        /// </summary>
        public event Action<Vector2> OnMovementInput;
        
        /// <summary>
        /// Event triggered when interaction input is detected.
        /// </summary>
        public event Action OnInteractionInput;
        
        /// <summary>
        /// Event triggered when special ability input is detected.
        /// </summary>
        public event Action OnSpecialAbilityInput;
        
        /// <summary>
        /// Event triggered when pause input is detected.
        /// </summary>
        public event Action OnPauseInput;
        
        /// <summary>
        /// Flag to track if the input provider is enabled.
        /// </summary>
        private bool _isEnabled = false;
        
        /// <summary>
        /// Current movement input vector.
        /// </summary>
        private Vector2 _movementInput = Vector2.zero;
        
        /// <summary>
        /// Flag to track if interaction input is active.
        /// </summary>
        private bool _isInteractionActive = false;
        
        /// <summary>
        /// Flag to track if special ability input is active.
        /// </summary>
        private bool _isSpecialAbilityActive = false;
        
        /// <summary>
        /// Reference to the input actions asset.
        /// </summary>
        private PlayerInputActions _inputActions;
        
        /// <summary>
        /// Initialize the input provider.
        /// </summary>
        public void Initialize()
        {
            Debug.Log("[InputSystemProvider] Initializing input system provider");
            
            // Create the input actions asset
            _inputActions = new PlayerInputActions();
            
            // Set up callbacks
            _inputActions.Player.SetCallbacks(this);
            
            _isEnabled = true;
        }
        
        /// <summary>
        /// Update the input provider.
        /// </summary>
        public void Update()
        {
            // No need to update, the Input System handles this automatically
        }
        
        /// <summary>
        /// Enable the input provider.
        /// </summary>
        public void Enable()
        {
            if (!_isEnabled)
            {
                return;
            }
            
            _inputActions.Player.Enable();
            Debug.Log("[InputSystemProvider] Input system provider enabled");
        }
        
        /// <summary>
        /// Disable the input provider.
        /// </summary>
        public void Disable()
        {
            _inputActions.Player.Disable();
            ResetInput();
            Debug.Log("[InputSystemProvider] Input system provider disabled");
        }
        
        /// <summary>
        /// Get the current movement input.
        /// </summary>
        /// <returns>Movement input vector</returns>
        public Vector2 GetMovementInput()
        {
            return _movementInput;
        }
        
        /// <summary>
        /// Check if interaction input is active.
        /// </summary>
        /// <returns>True if interaction input is active</returns>
        public bool IsInteractionActive()
        {
            return _isInteractionActive;
        }
        
        /// <summary>
        /// Check if special ability input is active.
        /// </summary>
        /// <returns>True if special ability input is active</returns>
        public bool IsSpecialAbilityActive()
        {
            return _isSpecialAbilityActive;
        }
        
        /// <summary>
        /// Reset all input to default values.
        /// </summary>
        private void ResetInput()
        {
            _movementInput = Vector2.zero;
            _isInteractionActive = false;
            _isSpecialAbilityActive = false;
        }
        
        /// <summary>
        /// Handle move input from the Input System.
        /// </summary>
        /// <param name="context">The callback context</param>
        public void OnMove(InputAction.CallbackContext context)
        {
            // Get the input value
            _movementInput = context.ReadValue<Vector2>();
            
            // Trigger the event
            OnMovementInput?.Invoke(_movementInput);
        }
        
        /// <summary>
        /// Handle interact input from the Input System.
        /// </summary>
        /// <param name="context">The callback context</param>
        public void OnInteract(InputAction.CallbackContext context)
        {
            // Check if the button was pressed
            if (context.performed)
            {
                _isInteractionActive = true;
                OnInteractionInput?.Invoke();
            }
            else if (context.canceled)
            {
                _isInteractionActive = false;
            }
        }
        
        /// <summary>
        /// Handle special ability input from the Input System.
        /// </summary>
        /// <param name="context">The callback context</param>
        public void OnSpecialAbility(InputAction.CallbackContext context)
        {
            // Check if the button was pressed
            if (context.performed)
            {
                _isSpecialAbilityActive = true;
                OnSpecialAbilityInput?.Invoke();
            }
            else if (context.canceled)
            {
                _isSpecialAbilityActive = false;
            }
        }
        
        /// <summary>
        /// Handle pause input from the Input System.
        /// </summary>
        /// <param name="context">The callback context</param>
        public void OnPause(InputAction.CallbackContext context)
        {
            // Check if the button was pressed
            if (context.performed)
            {
                OnPauseInput?.Invoke();
            }
        }
    }
}
