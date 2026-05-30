using System;
using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay
{
    public class PlayerStateController
    {
        private PlayerMovementState _currentState = PlayerMovementState.Idle;
        private PlayerMovementState _previousState = PlayerMovementState.Idle;

        public event Action<PlayerMovementState, PlayerMovementState> OnStateChanged;
        public PlayerMovementState CurrentState => _currentState;

        public void UpdateState(Vector2 input, bool isHoldingObject)
        {
            PlayerMovementState newState = _currentState;

            switch (_currentState)
            {
                case PlayerMovementState.Idle:
                    if (input.sqrMagnitude > 0.01f)
                        newState = isHoldingObject ? PlayerMovementState.Carrying : PlayerMovementState.Moving;
                    break;
                case PlayerMovementState.Moving:
                    if (input.sqrMagnitude < 0.01f) newState = PlayerMovementState.Idle;
                    else if (isHoldingObject) newState = PlayerMovementState.Carrying;
                    break;
                case PlayerMovementState.Carrying:
                    if (input.sqrMagnitude < 0.01f) newState = PlayerMovementState.Idle;
                    else if (!isHoldingObject) newState = PlayerMovementState.Moving;
                    break;
            }

            if (newState != _currentState) SetState(newState);
        }

        public void SetState(PlayerMovementState newState)
        {
            if (newState == _currentState) return;
            _previousState = _currentState;
            _currentState = newState;
            OnStateChanged?.Invoke(_previousState, _currentState);
        }

        public bool CanMove() => _currentState != PlayerMovementState.Interacting && _currentState != PlayerMovementState.Stunned;
        public bool IsMoving() => _currentState == PlayerMovementState.Moving || _currentState == PlayerMovementState.Carrying;
    }
}
