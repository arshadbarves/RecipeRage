using System.Collections.Generic;
using UnityEngine;
using Modules.Logging;

namespace Gameplay.Characters
{
    /// <summary>
    /// Handles client-side prediction and server reconciliation.
    /// Single Responsibility: Network prediction logic.
    /// </summary>
    public class PlayerNetworkController
    {
        private readonly bool _enablePrediction;
        private readonly int _maxHistorySize;
        private readonly float _reconciliationThreshold;
        
        private uint _inputSequence = 0;
        private uint _lastAcknowledgedSequence = 0;
        
        private readonly Queue<PlayerInputData> _inputHistory = new Queue<PlayerInputData>();
        private readonly Queue<PlayerStateData> _stateHistory = new Queue<PlayerStateData>();
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public PlayerNetworkController(
            bool enablePrediction,
            int maxHistorySize,
            float reconciliationThreshold)
        {
            _enablePrediction = enablePrediction;
            _maxHistorySize = maxHistorySize;
            _reconciliationThreshold = reconciliationThreshold;
        }
        
        /// <summary>
        /// Check if prediction is enabled.
        /// </summary>
        public bool IsPredictionEnabled => _enablePrediction;
        
        /// <summary>
        /// Create input data for current frame.
        /// </summary>
        public PlayerInputData CreateInputData(Vector2 movement)
        {
            return new PlayerInputData
            {
                Movement = movement,
                InteractPressed = false,
                AbilityPressed = false,
                Timestamp = Time.time,
                SequenceNumber = _inputSequence++
            };
        }
        
        /// <summary>
        /// Store input and state for reconciliation.
        /// </summary>
        public void StoreHistory(PlayerInputData input, PlayerStateData state)
        {
            _inputHistory.Enqueue(input);
            _stateHistory.Enqueue(state);
            
            // Limit history size
            while (_inputHistory.Count > _maxHistorySize)
            {
                _inputHistory.Dequeue();
                _stateHistory.Dequeue();
            }
        }
        
        /// <summary>
        /// Create state data from current transform and rigidbody.
        /// </summary>
        public PlayerStateData CreateStateData(Transform transform, Rigidbody rigidbody, uint sequenceNumber)
        {
            return new PlayerStateData
            {
                Position = transform.position,
                Rotation = transform.rotation,
                Velocity = rigidbody.linearVelocity,
                Timestamp = Time.time,
                SequenceNumber = sequenceNumber
            };
        }
        
        /// <summary>
        /// Reconcile client prediction with server state.
        /// </summary>
        public bool ReconcileState(
            PlayerStateData serverState,
            Transform transform,
            Rigidbody rigidbody,
            System.Action<PlayerInputData> replayInputCallback)
        {
            _lastAcknowledgedSequence = serverState.SequenceNumber;
            
            // Find predicted state for this sequence
            PlayerStateData? predictedState = FindPredictedState(serverState.SequenceNumber);
            
            if (!predictedState.HasValue)
            {
                // State not found, accept server state
                ApplyServerState(serverState, transform, rigidbody);
                return true;
            }
            
            // Calculate prediction error
            float positionError = Vector3.Distance(predictedState.Value.Position, serverState.Position);
            
            // Reconcile if error is significant
            if (positionError > _reconciliationThreshold)
            {
                GameLogger.Log($"Reconciling: error = {positionError:F3}m");
                
                // Snap to server position
                ApplyServerState(serverState, transform, rigidbody);
                
                // Replay inputs after this sequence
                ReplayInputs(serverState.SequenceNumber, replayInputCallback);
                
                // Clean up history
                CleanupHistory(serverState.SequenceNumber);
                
                return true;
            }
            
            // Clean up history
            CleanupHistory(serverState.SequenceNumber);
            
            return false;
        }
        
        /// <summary>
        /// Find predicted state by sequence number.
        /// </summary>
        private PlayerStateData? FindPredictedState(uint sequenceNumber)
        {
            foreach (var state in _stateHistory)
            {
                if (state.SequenceNumber == sequenceNumber)
                {
                    return state;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Apply server state to transform and rigidbody.
        /// </summary>
        private void ApplyServerState(PlayerStateData state, Transform transform, Rigidbody rigidbody)
        {
            transform.position = state.Position;
            transform.rotation = state.Rotation;
            rigidbody.linearVelocity = state.Velocity;
        }
        
        /// <summary>
        /// Replay inputs after reconciliation point.
        /// </summary>
        private void ReplayInputs(uint fromSequence, System.Action<PlayerInputData> replayCallback)
        {
            foreach (var input in _inputHistory)
            {
                if (input.SequenceNumber > fromSequence)
                {
                    replayCallback?.Invoke(input);
                }
            }
        }
        
        /// <summary>
        /// Clean up old history.
        /// </summary>
        private void CleanupHistory(uint acknowledgedSequence)
        {
            while (_inputHistory.Count > 0 && _inputHistory.Peek().SequenceNumber <= acknowledgedSequence)
            {
                _inputHistory.Dequeue();
            }
            
            while (_stateHistory.Count > 0 && _stateHistory.Peek().SequenceNumber <= acknowledgedSequence)
            {
                _stateHistory.Dequeue();
            }
        }
        
        /// <summary>
        /// Get debug info.
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Sequence: {_inputSequence}\n" +
                   $"History: {_inputHistory.Count}/{_maxHistorySize}\n" +
                   $"Last Ack: {_lastAcknowledgedSequence}";
        }
    }
}
