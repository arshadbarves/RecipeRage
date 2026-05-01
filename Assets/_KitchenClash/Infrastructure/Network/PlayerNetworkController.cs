using System;
using System.Collections.Generic;
using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Network
{
    public class PlayerNetworkController
    {
        private readonly bool _enablePrediction;
        private readonly int _maxHistorySize;
        private readonly float _reconciliationThreshold;

        private uint _inputSequence = 0;
        private uint _lastAcknowledgedSequence = 0;

        private readonly Queue<PlayerInputData> _inputHistory = new Queue<PlayerInputData>();
        private readonly Queue<PlayerStateData> _stateHistory = new Queue<PlayerStateData>();

        public bool IsPredictionEnabled => _enablePrediction;

        public PlayerNetworkController(bool enablePrediction, int maxHistorySize, float reconciliationThreshold)
        {
            _enablePrediction = enablePrediction;
            _maxHistorySize = maxHistorySize;
            _reconciliationThreshold = reconciliationThreshold;
        }

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

        public void StoreHistory(PlayerInputData input, PlayerStateData state)
        {
            _inputHistory.Enqueue(input);
            _stateHistory.Enqueue(state);
            while (_inputHistory.Count > _maxHistorySize)
            {
                _inputHistory.Dequeue();
                _stateHistory.Dequeue();
            }
        }

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

        public void ReconcileState(PlayerStateData serverState, Transform transform, Rigidbody rigidbody, Action<PlayerInputData> replayAction)
        {
            if (!_enablePrediction) return;

            float delta = Vector3.Distance(transform.position, serverState.Position);
            if (delta > _reconciliationThreshold)
            {
                transform.position = serverState.Position;
                transform.rotation = serverState.Rotation;
                rigidbody.linearVelocity = serverState.Velocity;

                _lastAcknowledgedSequence = serverState.SequenceNumber;

                // Replay unacknowledged inputs
                foreach (var input in _inputHistory)
                {
                    if (input.SequenceNumber > _lastAcknowledgedSequence)
                    {
                        replayAction?.Invoke(input);
                    }
                }
            }
        }

        public string GetDebugInfo()
        {
            return $"Sequence: {_inputSequence}\nHistory: {_inputHistory.Count}/{_maxHistorySize}\nLast Ack: {_lastAcknowledgedSequence}";
        }
    }
}
