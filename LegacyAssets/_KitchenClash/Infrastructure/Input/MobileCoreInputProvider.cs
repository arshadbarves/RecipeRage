using System;
using KitchenClash.Application;
using Playcenter.MobileCore;
using UnityEngine;

namespace KitchenClash.Infrastructure.Input
{
    /// <summary>
    /// Sole IInputProvider for RecipeRage: polls the MobileCore dual-stick model
    /// (fed by the PlaycenterBootstrap touch provider) and raises the legacy
    /// provider events PlayerController consumes. Replaces TouchInputProvider +
    /// InputSystemProvider + InputReceiver + GameplayInputBridge statics.
    /// </summary>
    public sealed class MobileCoreInputProvider : IInputProvider
    {
        public event Action<Vector2> OnMovementInput;
        public event Action OnInteractionInput;
        public event Action OnSpecialAbilityInput;
        public event Action OnPauseInput;

        private readonly MobileCoreInputBridge _bridge;

        private bool _isEnabled;
        private Vector2 _movementInput;
        private InputButtons _previousButtons;

        public MobileCoreInputProvider(MobileCoreInputBridge bridge)
        {
            _bridge = bridge;
        }

        public void Initialize()
        {
            _isEnabled = true;
        }

        public void Update()
        {
            if (!_isEnabled)
            {
                return;
            }

            InputFrame frame = _bridge.LatestFrame;

            Vector2 move = new Vector2(frame.Move.X, frame.Move.Y);
            if (move != _movementInput)
            {
                _movementInput = move;
                OnMovementInput?.Invoke(_movementInput);
            }

            InputButtons pressed = frame.Buttons & ~_previousButtons;

            if ((pressed & InputButtons.Interact) != 0 || (pressed & InputButtons.AimReleased) != 0)
            {
                OnInteractionInput?.Invoke();
            }

            if ((pressed & InputButtons.Ability) != 0)
            {
                OnSpecialAbilityInput?.Invoke();
            }

            _previousButtons = frame.Buttons;
        }

        public void Enable()
        {
            _isEnabled = true;
        }

        public void Disable()
        {
            _isEnabled = false;
            _movementInput = Vector2.zero;
            _previousButtons = InputButtons.None;
        }

        public Vector2 GetMovementInput() => _movementInput;

        public bool IsInteractionActive() => (_bridge.LatestFrame.Buttons & InputButtons.Interact) != 0;

        public bool IsSpecialAbilityActive() => (_bridge.LatestFrame.Buttons & InputButtons.Ability) != 0;
    }
}
