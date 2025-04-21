using System;
using Core.Patterns;
using UnityEngine;

namespace Core.Input
{
    /// <summary>
    /// Manages input from different input providers and provides a unified interface for game input.
    /// </summary>
    public class InputManager : MonoBehaviourSingleton<InputManager>
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

        [Header("Input Settings")]
        [SerializeField] private bool _useKeyboardInput = true;
        [SerializeField] private bool _useTouchInput = true;
        [SerializeField] private bool _useInputSystem = true;

        /// <summary>
        /// The active input provider.
        /// </summary>
        private IInputProvider _activeInputProvider;

        /// <summary>
        /// The keyboard input provider.
        /// </summary>
        private KeyboardInputProvider _keyboardInputProvider;

        /// <summary>
        /// The touch input provider.
        /// </summary>
        private TouchInputProvider _touchInputProvider;

        /// <summary>
        /// The input system provider.
        /// </summary>
        private InputSystemProvider _inputSystemProvider;

        /// <summary>
        /// Flag to track if the input manager is initialized.
        /// </summary>
        private bool _isInitialized = false;

        /// <summary>
        /// Initialize the input manager.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Initialize input providers
            InitializeInputProviders();
        }

        /// <summary>
        /// Update the active input provider.
        /// </summary>
        private void Update()
        {
            // Update the active input provider
            _activeInputProvider?.Update();
        }

        /// <summary>
        /// Clean up when the object is destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unsubscribe from input provider events
            UnsubscribeFromInputProviderEvents(_keyboardInputProvider);
            UnsubscribeFromInputProviderEvents(_touchInputProvider);
            UnsubscribeFromInputProviderEvents(_inputSystemProvider);
        }

        /// <summary>
        /// Initialize input providers.
        /// </summary>
        private void InitializeInputProviders()
        {
            if (_isInitialized)
            {
                return;
            }

            Debug.Log("[InputManager] Initializing input providers");

            // Create input providers
            _keyboardInputProvider = new KeyboardInputProvider();
            _touchInputProvider = new TouchInputProvider();
            _inputSystemProvider = new InputSystemProvider();

            // Initialize input providers
            _keyboardInputProvider.Initialize();
            _touchInputProvider.Initialize();
            _inputSystemProvider.Initialize();

            // Subscribe to input provider events
            SubscribeToInputProviderEvents(_keyboardInputProvider);
            SubscribeToInputProviderEvents(_touchInputProvider);
            SubscribeToInputProviderEvents(_inputSystemProvider);

            // Set the active input provider based on platform and settings
            SetActiveInputProvider();

            _isInitialized = true;
            Debug.Log("[InputManager] Input providers initialized");
        }

        /// <summary>
        /// Set the active input provider based on platform and settings.
        /// </summary>
        private void SetActiveInputProvider()
        {
            // Disable all input providers first
            _keyboardInputProvider?.Disable();
            _touchInputProvider?.Disable();
            _inputSystemProvider?.Disable();

            // Set the active input provider based on platform and settings
            if (_useInputSystem)
            {
                _activeInputProvider = _inputSystemProvider;
                _activeInputProvider.Enable();
                Debug.Log("[InputManager] Using input system provider");
            }
#if UNITY_EDITOR || UNITY_STANDALONE
            else if (_useKeyboardInput)
            {
                _activeInputProvider = _keyboardInputProvider;
                _activeInputProvider.Enable();
                Debug.Log("[InputManager] Using keyboard input provider");
            }
            else if (_useTouchInput)
            {
                _activeInputProvider = _touchInputProvider;
                _activeInputProvider.Enable();
                Debug.Log("[InputManager] Using touch input provider");
            }
#elif UNITY_IOS || UNITY_ANDROID
            else if (_useTouchInput)
            {
                _activeInputProvider = _touchInputProvider;
                _activeInputProvider.Enable();
                Debug.Log("[InputManager] Using touch input provider");
            }
            else if (_useKeyboardInput)
            {
                _activeInputProvider = _keyboardInputProvider;
                _activeInputProvider.Enable();
                Debug.Log("[InputManager] Using keyboard input provider");
            }
#endif

            if (_activeInputProvider == null)
            {
                Debug.LogWarning("[InputManager] No input provider enabled. Input will not work.");
            }
        }

        /// <summary>
        /// Subscribe to input provider events.
        /// </summary>
        /// <param name="inputProvider">The input provider to subscribe to</param>
        private void SubscribeToInputProviderEvents(IInputProvider inputProvider)
        {
            if (inputProvider == null)
            {
                return;
            }

            inputProvider.OnMovementInput += HandleMovementInput;
            inputProvider.OnInteractionInput += HandleInteractionInput;
            inputProvider.OnSpecialAbilityInput += HandleSpecialAbilityInput;
            inputProvider.OnPauseInput += HandlePauseInput;
        }

        /// <summary>
        /// Unsubscribe from input provider events.
        /// </summary>
        /// <param name="inputProvider">The input provider to unsubscribe from</param>
        private void UnsubscribeFromInputProviderEvents(IInputProvider inputProvider)
        {
            if (inputProvider == null)
            {
                return;
            }

            inputProvider.OnMovementInput -= HandleMovementInput;
            inputProvider.OnInteractionInput -= HandleInteractionInput;
            inputProvider.OnSpecialAbilityInput -= HandleSpecialAbilityInput;
            inputProvider.OnPauseInput -= HandlePauseInput;
        }

        /// <summary>
        /// Handle movement input from the input provider.
        /// </summary>
        /// <param name="movementInput">The movement input vector</param>
        private void HandleMovementInput(Vector2 movementInput)
        {
            // Forward the event
            OnMovementInput?.Invoke(movementInput);
        }

        /// <summary>
        /// Handle interaction input from the input provider.
        /// </summary>
        private void HandleInteractionInput()
        {
            // Forward the event
            OnInteractionInput?.Invoke();
        }

        /// <summary>
        /// Handle special ability input from the input provider.
        /// </summary>
        private void HandleSpecialAbilityInput()
        {
            // Forward the event
            OnSpecialAbilityInput?.Invoke();
        }

        /// <summary>
        /// Handle pause input from the input provider.
        /// </summary>
        private void HandlePauseInput()
        {
            // Forward the event
            OnPauseInput?.Invoke();
        }

        /// <summary>
        /// Get the current movement input.
        /// </summary>
        /// <returns>Movement input vector</returns>
        public Vector2 GetMovementInput()
        {
            return _activeInputProvider?.GetMovementInput() ?? Vector2.zero;
        }

        /// <summary>
        /// Check if interaction input is active.
        /// </summary>
        /// <returns>True if interaction input is active</returns>
        public bool IsInteractionActive()
        {
            return _activeInputProvider?.IsInteractionActive() ?? false;
        }

        /// <summary>
        /// Check if special ability input is active.
        /// </summary>
        /// <returns>True if special ability input is active</returns>
        public bool IsSpecialAbilityActive()
        {
            return _activeInputProvider?.IsSpecialAbilityActive() ?? false;
        }

        /// <summary>
        /// Enable input.
        /// </summary>
        public void EnableInput()
        {
            _activeInputProvider?.Enable();
        }

        /// <summary>
        /// Disable input.
        /// </summary>
        public void DisableInput()
        {
            _activeInputProvider?.Disable();
        }

        /// <summary>
        /// Switch to keyboard input.
        /// </summary>
        public void SwitchToKeyboardInput()
        {
            if (_keyboardInputProvider == null || !_useKeyboardInput)
            {
                return;
            }

            _activeInputProvider?.Disable();
            _activeInputProvider = _keyboardInputProvider;
            _activeInputProvider.Enable();

            Debug.Log("[InputManager] Switched to keyboard input provider");
        }

        /// <summary>
        /// Switch to touch input.
        /// </summary>
        public void SwitchToTouchInput()
        {
            if (_touchInputProvider == null || !_useTouchInput)
            {
                return;
            }

            _activeInputProvider?.Disable();
            _activeInputProvider = _touchInputProvider;
            _activeInputProvider.Enable();

            Debug.Log("[InputManager] Switched to touch input provider");
        }

        /// <summary>
        /// Switch to input system.
        /// </summary>
        public void SwitchToInputSystem()
        {
            if (_inputSystemProvider == null || !_useInputSystem)
            {
                return;
            }

            _activeInputProvider?.Disable();
            _activeInputProvider = _inputSystemProvider;
            _activeInputProvider.Enable();

            Debug.Log("[InputManager] Switched to input system provider");
        }
    }
}
