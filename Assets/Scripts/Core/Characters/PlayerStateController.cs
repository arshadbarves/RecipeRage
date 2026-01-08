using System;
using UnityEngine;
using Modules.Logging;

namespace Core.Characters
{
    /// <summary>
    /// Manages player movement state machine.
    /// Single Responsibility: State transitions and state logic.
    /// </summary>
    public class PlayerStateController
    {
        private PlayerMovementState _currentState = PlayerMovementState.Idle;
        private PlayerMovementState _previousState = PlayerMovementState.Idle;
        
        /// <summary>
        /// Event triggered when state changes.
        /// </summary>
        public event Action<PlayerMovementState, PlayerMovementState> OnStateChanged;
        
        /// <summary>
        /// Get current state.
        /// </summary>
        public PlayerMovementState CurrentState => _currentState;
        
        /// <summary>
        /// Update state machine based on input and conditions.
        /// </summary>
        public void UpdateState(Vector2 input, bool isHoldingObject)
        {
            PlayerMovementState newState = _currentState;
            
            switch (_currentState)
            {
                case PlayerMovementState.Idle:
                    if (input.sqrMagnitude > 0.01f)
                    {
                        newState = isHoldingObject ? PlayerMovementState.Carrying : PlayerMovementState.Moving;
                    }
                    break;
                    
                case PlayerMovementState.Moving:
                    if (input.sqrMagnitude < 0.01f)
                    {
                        newState = PlayerMovementState.Idle;
                    }
                    else if (isHoldingObject)
                    {
                        newState = PlayerMovementState.Carrying;
                    }
                    break;
                    
                case PlayerMovementState.Carrying:
                    if (input.sqrMagnitude < 0.01f)
                    {
                        newState = PlayerMovementState.Idle;
                    }
                    else if (!isHoldingObject)
                    {
                        newState = PlayerMovementState.Moving;
                    }
                    break;
                    
                case PlayerMovementState.Interacting:
                case PlayerMovementState.UsingAbility:
                case PlayerMovementState.Stunned:
                    // These states are set externally
                    break;
            }
            
            if (newState != _currentState)
            {
                ChangeState(newState);
            }
        }
        
        /// <summary>
        /// Change state manually (for external control).
        /// </summary>
        public void SetState(PlayerMovementState newState)
        {
            if (newState != _currentState)
            {
                ChangeState(newState);
            }
        }
        
        /// <summary>
        /// Internal state change with event notification.
        /// </summary>
        private void ChangeState(PlayerMovementState newState)
        {
            _previousState = _currentState;
            _currentState = newState;
            
            OnStateChanged?.Invoke(_previousState, _currentState);
            
            GameLogger.Log($"State: {_previousState} → {_currentState}");
        }
        
        /// <summary>
        /// Check if player can move in current state.
        /// </summary>
        public bool CanMove()
        {
            return _currentState != PlayerMovementState.Interacting &&
                   _currentState != PlayerMovementState.Stunned;
        }
        
        /// <summary>
        /// Check if player is currently moving.
        /// </summary>
        public bool IsMoving()
        {
            return _currentState == PlayerMovementState.Moving ||
                   _currentState == PlayerMovementState.Carrying;
        }
    }
}
