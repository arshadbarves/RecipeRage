using System;
using UnityEngine;

namespace Core.Input
{
    /// <summary>
    /// Input provider that handles keyboard input for development and testing.
    /// </summary>
    public class KeyboardInputProvider : IInputProvider
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
        /// Previous interaction input state.
        /// </summary>
        private bool _wasInteractionActive = false;
        
        /// <summary>
        /// Previous special ability input state.
        /// </summary>
        private bool _wasSpecialAbilityActive = false;
        
        /// <summary>
        /// Previous pause input state.
        /// </summary>
        private bool _wasPauseActive = false;
        
        /// <summary>
        /// Initialize the input provider.
        /// </summary>
        public void Initialize()
        {
            Debug.Log("[KeyboardInputProvider] Initializing keyboard input provider");
            _isEnabled = true;
        }
        
        /// <summary>
        /// Update the input provider.
        /// </summary>
        public void Update()
        {
            if (!_isEnabled)
            {
                return;
            }
            
            // Process keyboard input
            ProcessKeyboardInput();
        }
        
        /// <summary>
        /// Enable the input provider.
        /// </summary>
        public void Enable()
        {
            _isEnabled = true;
            Debug.Log("[KeyboardInputProvider] Keyboard input provider enabled");
        }
        
        /// <summary>
        /// Disable the input provider.
        /// </summary>
        public void Disable()
        {
            _isEnabled = false;
            ResetInput();
            Debug.Log("[KeyboardInputProvider] Keyboard input provider disabled");
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
        /// Process keyboard input.
        /// </summary>
        private void ProcessKeyboardInput()
        {
            // Process movement input (WASD or arrow keys)
            float horizontal = UnityEngine.Input.GetAxisRaw("Horizontal");
            float vertical = UnityEngine.Input.GetAxisRaw("Vertical");
            
            Vector2 newMovementInput = new Vector2(horizontal, vertical);
            
            // Normalize if magnitude is greater than 1
            if (newMovementInput.sqrMagnitude > 1f)
            {
                newMovementInput.Normalize();
            }
            
            // Update movement input if changed
            if (_movementInput != newMovementInput)
            {
                _movementInput = newMovementInput;
                OnMovementInput?.Invoke(_movementInput);
            }
            
            // Process interaction input (E key or Space)
            bool isInteractionKeyDown = UnityEngine.Input.GetKey(KeyCode.E) || UnityEngine.Input.GetKey(KeyCode.Space);
            
            if (isInteractionKeyDown != _isInteractionActive)
            {
                _isInteractionActive = isInteractionKeyDown;
                
                // Only trigger event on key down
                if (_isInteractionActive && !_wasInteractionActive)
                {
                    OnInteractionInput?.Invoke();
                }
                
                _wasInteractionActive = _isInteractionActive;
            }
            
            // Process special ability input (Q key or Left Shift)
            bool isSpecialAbilityKeyDown = UnityEngine.Input.GetKey(KeyCode.Q) || UnityEngine.Input.GetKey(KeyCode.LeftShift);
            
            if (isSpecialAbilityKeyDown != _isSpecialAbilityActive)
            {
                _isSpecialAbilityActive = isSpecialAbilityKeyDown;
                
                // Only trigger event on key down
                if (_isSpecialAbilityActive && !_wasSpecialAbilityActive)
                {
                    OnSpecialAbilityInput?.Invoke();
                }
                
                _wasSpecialAbilityActive = _isSpecialAbilityActive;
            }
            
            // Process pause input (Escape key or P)
            bool isPauseKeyDown = UnityEngine.Input.GetKey(KeyCode.Escape) || UnityEngine.Input.GetKey(KeyCode.P);
            
            // Only trigger event on key down
            if (isPauseKeyDown && !_wasPauseActive)
            {
                OnPauseInput?.Invoke();
            }
            
            _wasPauseActive = isPauseKeyDown;
        }
        
        /// <summary>
        /// Reset all input to default values.
        /// </summary>
        private void ResetInput()
        {
            _movementInput = Vector2.zero;
            _isInteractionActive = false;
            _isSpecialAbilityActive = false;
            _wasInteractionActive = false;
            _wasSpecialAbilityActive = false;
            _wasPauseActive = false;
        }
    }
}
