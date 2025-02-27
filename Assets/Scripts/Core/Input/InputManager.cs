using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace RecipeRage.Core.Input
{
    /// <summary>
    /// Manages input handling using the new Input System
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        #region Events
        public event Action<Vector2> OnMovementInput;
        public event Action OnInteractionStarted;
        public event Action OnInteractionCanceled;
        public event Action OnPauseTriggered;
        #endregion

        #region Private Fields
        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _interactAction;
        private InputAction _pauseAction;
        private bool _isInitialized;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            if (_playerInput != null && _playerInput.actions != null)
            {
                InitializeActions();
            }
        }

        private void Start()
        {
            // Fallback initialization in case actions weren't ready in Awake
            if (!_isInitialized && _playerInput != null && _playerInput.actions != null)
            {
                InitializeActions();
            }
        }

        private void OnEnable()
        {
            if (_isInitialized)
            {
                EnableActions();
            }
        }

        private void OnDisable()
        {
            if (_isInitialized)
            {
                DisableActions();
            }
        }

        private void OnDestroy()
        {
            if (_isInitialized)
            {
                UnsubscribeFromActions();
            }
        }
        #endregion

        #region Input Handlers
        private void OnMove(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            OnMovementInput?.Invoke(input);
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            OnInteractionStarted?.Invoke();
        }

        private void OnInteractCancel(InputAction.CallbackContext context)
        {
            OnInteractionCanceled?.Invoke();
        }

        private void OnPause(InputAction.CallbackContext context)
        {
            OnPauseTriggered?.Invoke();
        }
        #endregion

        #region Private Methods
        private void InitializeActions()
        {
            try
            {
                _moveAction = _playerInput.actions["Move"];
                _interactAction = _playerInput.actions["Interact"];
                _pauseAction = _playerInput.actions["Pause"];

                _moveAction.performed += OnMove;
                _moveAction.canceled += OnMove;
                _interactAction.started += OnInteract;
                _interactAction.canceled += OnInteractCancel;
                _pauseAction.performed += OnPause;

                _isInitialized = true;
                EnableActions();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize input actions: {e.Message}");
            }
        }

        private void EnableActions()
        {
            _moveAction?.Enable();
            _interactAction?.Enable();
            _pauseAction?.Enable();
        }

        private void DisableActions()
        {
            _moveAction?.Disable();
            _interactAction?.Disable();
            _pauseAction?.Disable();
        }

        private void UnsubscribeFromActions()
        {
            if (_moveAction != null)
            {
                _moveAction.performed -= OnMove;
                _moveAction.canceled -= OnMove;
            }
            if (_interactAction != null)
            {
                _interactAction.started -= OnInteract;
                _interactAction.canceled -= OnInteractCancel;
            }
            if (_pauseAction != null)
            {
                _pauseAction.performed -= OnPause;
            }
        }
        #endregion
    }
} 