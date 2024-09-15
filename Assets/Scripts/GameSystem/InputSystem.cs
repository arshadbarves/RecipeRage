using System;
using System.Threading.Tasks;
using Core.TouchInputSystem;
using UnityEngine;

namespace GameSystem
{
    public class InputSystem : IGameSystem
    {
        private TouchInputManager _inputManager;
        private PlayerInput _playerInput;

        public event EventHandler OnPrimaryInteractAction;
        public event EventHandler OnSecondaryInteractAction;
        public event EventHandler OnPrimaryAttackAction;
        public event EventHandler OnSecondaryAttackAction;

        private Action<UnityEngine.InputSystem.InputAction.CallbackContext> _primaryInteractAction;
        private Action<UnityEngine.InputSystem.InputAction.CallbackContext> _secondaryInteractAction;
        private Action<UnityEngine.InputSystem.InputAction.CallbackContext> _primaryAttackAction;
        private Action<UnityEngine.InputSystem.InputAction.CallbackContext> _secondaryAttackAction;

        public async Task InitializeAsync()
        {
            _playerInput = new PlayerInput();
            _playerInput.Enable();

            _primaryInteractAction = ctx => OnPrimaryInteractAction?.Invoke(this, EventArgs.Empty);
            _secondaryInteractAction = ctx => OnSecondaryInteractAction?.Invoke(this, EventArgs.Empty);
            _primaryAttackAction = ctx => OnPrimaryAttackAction?.Invoke(this, EventArgs.Empty);
            _secondaryAttackAction = ctx => OnSecondaryAttackAction?.Invoke(this, EventArgs.Empty);

            _playerInput.Gameplay.PrimaryInteract.performed += _primaryInteractAction;
            _playerInput.Gameplay.SecondaryInteract.performed += _secondaryInteractAction;
            _playerInput.Gameplay.PrimaryAttack.performed += _primaryAttackAction;
            _playerInput.Gameplay.SecondaryAttack.performed += _secondaryAttackAction;

            await Task.CompletedTask;
        }

        public void Update() { }

        public async Task CleanupAsync()
        {
            _playerInput.Gameplay.PrimaryInteract.performed -= _primaryInteractAction;
            _playerInput.Gameplay.SecondaryInteract.performed -= _secondaryInteractAction;
            _playerInput.Gameplay.PrimaryAttack.performed -= _primaryAttackAction;
            _playerInput.Gameplay.SecondaryAttack.performed -= _secondaryAttackAction;
            _playerInput.Disable();

            await Task.CompletedTask;
        }

        public Vector2 GetMovementInput()
        {
#if UNITY_IOS || UNITY_ANDROID
            return _inputManager.GetJoystickValue("Movement");
#else
            return _playerInput.Gameplay.Move.ReadValue<Vector2>();
#endif
        }
    }
}