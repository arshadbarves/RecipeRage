using System;
using UnityEngine;

namespace Core.Input
{
    /// <summary>
    /// Input provider that handles touch input for mobile devices.
    /// </summary>
    public class TouchInputProvider : IInputProvider
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
        /// Reference to the virtual joystick for movement.
        /// </summary>
        private RectTransform _joystickArea;
        
        /// <summary>
        /// Center position of the joystick.
        /// </summary>
        private Vector2 _joystickCenter;
        
        /// <summary>
        /// Current touch ID for the joystick.
        /// </summary>
        private int _joystickTouchId = -1;
        
        /// <summary>
        /// Maximum distance for joystick movement.
        /// </summary>
        private float _joystickRadius = 100f;
        
        /// <summary>
        /// Initialize the input provider.
        /// </summary>
        public void Initialize()
        {
            Debug.Log("[TouchInputProvider] Initializing touch input provider");
            
            // Find joystick area in the UI
            // In a real implementation, this would be passed in or found in the UI
            // For now, we'll use a placeholder
            
            // TODO: Implement actual joystick area reference
            
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
            
            // Process touch input
            ProcessTouchInput();
        }
        
        /// <summary>
        /// Enable the input provider.
        /// </summary>
        public void Enable()
        {
            _isEnabled = true;
            Debug.Log("[TouchInputProvider] Touch input provider enabled");
        }
        
        /// <summary>
        /// Disable the input provider.
        /// </summary>
        public void Disable()
        {
            _isEnabled = false;
            ResetInput();
            Debug.Log("[TouchInputProvider] Touch input provider disabled");
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
        /// Process touch input from the screen.
        /// </summary>
        private void ProcessTouchInput()
        {
            // Process all touches
            for (int i = 0; i < UnityEngine.Input.touchCount; i++)
            {
                Touch touch = UnityEngine.Input.GetTouch(i);
                
                // Process touch based on phase
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        HandleTouchBegan(touch);
                        break;
                    
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        HandleTouchMoved(touch);
                        break;
                    
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        HandleTouchEnded(touch);
                        break;
                }
            }
            
            // If no touches and joystick was active, reset movement
            if (UnityEngine.Input.touchCount == 0 && _joystickTouchId != -1)
            {
                _joystickTouchId = -1;
                SetMovementInput(Vector2.zero);
            }
        }
        
        /// <summary>
        /// Handle touch began phase.
        /// </summary>
        /// <param name="touch">The touch data</param>
        private void HandleTouchBegan(Touch touch)
        {
            // Check if touch is in joystick area
            if (_joystickArea != null && RectTransformUtility.RectangleContainsScreenPoint(_joystickArea, touch.position))
            {
                // Start joystick tracking
                _joystickTouchId = touch.fingerId;
                _joystickCenter = touch.position;
            }
            // Check if touch is in interaction button area
            else if (IsInInteractionArea(touch.position))
            {
                _isInteractionActive = true;
                OnInteractionInput?.Invoke();
            }
            // Check if touch is in special ability button area
            else if (IsInSpecialAbilityArea(touch.position))
            {
                _isSpecialAbilityActive = true;
                OnSpecialAbilityInput?.Invoke();
            }
            // Check if touch is in pause button area
            else if (IsInPauseArea(touch.position))
            {
                OnPauseInput?.Invoke();
            }
        }
        
        /// <summary>
        /// Handle touch moved or stationary phase.
        /// </summary>
        /// <param name="touch">The touch data</param>
        private void HandleTouchMoved(Touch touch)
        {
            // If this is the joystick touch, update movement
            if (touch.fingerId == _joystickTouchId)
            {
                // Calculate joystick direction and magnitude
                Vector2 direction = touch.position - _joystickCenter;
                
                // Clamp to joystick radius
                if (direction.magnitude > _joystickRadius)
                {
                    direction = direction.normalized * _joystickRadius;
                }
                
                // Convert to -1 to 1 range
                Vector2 normalizedDirection = direction / _joystickRadius;
                
                // Update movement input
                SetMovementInput(normalizedDirection);
            }
        }
        
        /// <summary>
        /// Handle touch ended or canceled phase.
        /// </summary>
        /// <param name="touch">The touch data</param>
        private void HandleTouchEnded(Touch touch)
        {
            // If this is the joystick touch, reset movement
            if (touch.fingerId == _joystickTouchId)
            {
                _joystickTouchId = -1;
                SetMovementInput(Vector2.zero);
            }
            // If this is the interaction button touch, reset interaction
            else if (_isInteractionActive && IsInInteractionArea(touch.position))
            {
                _isInteractionActive = false;
            }
            // If this is the special ability button touch, reset special ability
            else if (_isSpecialAbilityActive && IsInSpecialAbilityArea(touch.position))
            {
                _isSpecialAbilityActive = false;
            }
        }
        
        /// <summary>
        /// Set the movement input and trigger the event.
        /// </summary>
        /// <param name="input">The new movement input</param>
        private void SetMovementInput(Vector2 input)
        {
            // Only update and trigger event if input has changed
            if (_movementInput != input)
            {
                _movementInput = input;
                OnMovementInput?.Invoke(_movementInput);
            }
        }
        
        /// <summary>
        /// Reset all input to default values.
        /// </summary>
        private void ResetInput()
        {
            _movementInput = Vector2.zero;
            _isInteractionActive = false;
            _isSpecialAbilityActive = false;
            _joystickTouchId = -1;
        }
        
        /// <summary>
        /// Check if a position is in the interaction button area.
        /// </summary>
        /// <param name="position">The position to check</param>
        /// <returns>True if the position is in the interaction button area</returns>
        private bool IsInInteractionArea(Vector2 position)
        {
            // In a real implementation, this would check against the actual UI element
            // For now, we'll use a placeholder
            
            // TODO: Implement actual interaction button area check
            
            return false;
        }
        
        /// <summary>
        /// Check if a position is in the special ability button area.
        /// </summary>
        /// <param name="position">The position to check</param>
        /// <returns>True if the position is in the special ability button area</returns>
        private bool IsInSpecialAbilityArea(Vector2 position)
        {
            // In a real implementation, this would check against the actual UI element
            // For now, we'll use a placeholder
            
            // TODO: Implement actual special ability button area check
            
            return false;
        }
        
        /// <summary>
        /// Check if a position is in the pause button area.
        /// </summary>
        /// <param name="position">The position to check</param>
        /// <returns>True if the position is in the pause button area</returns>
        private bool IsInPauseArea(Vector2 position)
        {
            // In a real implementation, this would check against the actual UI element
            // For now, we'll use a placeholder
            
            // TODO: Implement actual pause button area check
            
            return false;
        }
    }
}
