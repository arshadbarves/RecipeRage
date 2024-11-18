using System;
using System.Threading.Tasks;
using Core.TouchInputSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using Action = Unity.Plastic.Antlr3.Runtime.Misc.Action;

namespace GameSystem.Input
{
    public class InputSystem : IGameSystem
    {
        private TouchInputManager _inputManager;
        private PlayerInput _playerInput;
        private Action<InputAction.CallbackContext> _primaryAttackAction;

        private Action<InputAction.CallbackContext> _primaryInteractAction;
        private Action<InputAction.CallbackContext> _secondaryAttackAction;
        private Action<InputAction.CallbackContext> _secondaryInteractAction;

        public async Task InitializeAsync()
        {
            _playerInput = new PlayerInput();
            _playerInput.Enable();

            _primaryInteractAction = _ => OnPrimaryInteractAction?.Invoke();
            _secondaryInteractAction = _ => OnSecondaryInteractAction?.Invoke();
            _primaryAttackAction = _ => OnPrimaryAttackAction?.Invoke();
            _secondaryAttackAction = _ => OnSecondaryAttackAction?.Invoke();

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

        public event Action OnPrimaryInteractAction;
        public event Action OnSecondaryInteractAction;
        public event Action OnPrimaryAttackAction;
        public event Action OnSecondaryAttackAction;

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