using System;
using KitchenClash.Application;
using KitchenClash.Application.Input;
using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Input
{
    /// <summary>
    /// Mobile touch input provider. Movement is polled from dual-stick InputReceiver
    /// when present; action buttons arrive via <see cref="GameplayInputBridge"/>.
    /// </summary>
    public sealed class TouchInputProvider : IInputProvider
    {
        public event Action<Vector2> OnMovementInput;
        public event Action OnInteractionInput;
        public event Action OnSpecialAbilityInput;
        public event Action OnAttackInput;
        public event Action OnPauseInput;

        private bool _isEnabled;
        private Vector2 _movementInput;
        private InputReceiver _receiver;

        public void Initialize()
        {
            _receiver = UnityEngine.Object.FindFirstObjectByType<InputReceiver>();
            GameplayInputBridge.AttackPressed += HandleBridgeAttack;
            GameplayInputBridge.InteractPressed += HandleBridgeInteract;
            GameplayInputBridge.SpecialPressed += HandleBridgeSpecial;
            _isEnabled = true;
        }

        public void Update()
        {
            if (!_isEnabled)
            {
                return;
            }

            if (_receiver == null)
            {
                _receiver = UnityEngine.Object.FindFirstObjectByType<InputReceiver>();
            }

            Vector2 move = _receiver != null ? _receiver.MoveInput : Vector2.zero;
            if (move != _movementInput)
            {
                _movementInput = move;
                OnMovementInput?.Invoke(_movementInput);
            }
        }

        public void Enable()
        {
            _isEnabled = true;
        }

        public void Disable()
        {
            _isEnabled = false;
            _movementInput = Vector2.zero;
            GameplayInputBridge.AttackPressed -= HandleBridgeAttack;
            GameplayInputBridge.InteractPressed -= HandleBridgeInteract;
            GameplayInputBridge.SpecialPressed -= HandleBridgeSpecial;
        }

        public Vector2 GetMovementInput() => _movementInput;

        public bool IsInteractionActive() => false;

        public bool IsSpecialAbilityActive() => false;

        private void HandleBridgeAttack()
        {
            if (_isEnabled)
            {
                OnAttackInput?.Invoke();
            }
        }

        private void HandleBridgeInteract()
        {
            if (_isEnabled)
            {
                OnInteractionInput?.Invoke();
            }
        }

        private void HandleBridgeSpecial()
        {
            if (_isEnabled)
            {
                OnSpecialAbilityInput?.Invoke();
            }
        }
    }
}
