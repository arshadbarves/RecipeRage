using System;
using Core.Logging;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Stations
{
    /// <summary>
    /// Manages station network state and access control.
    /// Follows Single Responsibility Principle - handles only station network state.
    /// </summary>
    public class StationNetworkController : NetworkBehaviour
    {
        [Header("Station Settings")]
        [SerializeField] private float _lockDuration = 0.5f; // Prevent rapid switching

        /// <summary>
        /// The ID of the player currently using the station.
        /// </summary>
        private NetworkVariable<ulong> _currentUserId = new NetworkVariable<ulong>(ulong.MaxValue);

        /// <summary>
        /// Whether the station is locked (in use).
        /// </summary>
        private NetworkVariable<bool> _isLocked = new NetworkVariable<bool>(false);

        /// <summary>
        /// The current state of the station.
        /// </summary>
        private NetworkVariable<StationState> _state = new NetworkVariable<StationState>(StationState.Idle);

        /// <summary>
        /// Time when the station was last locked.
        /// </summary>
        private float _lockTime;

        /// <summary>
        /// Event triggered when the station state changes.
        /// </summary>
        public event Action<StationState> OnStateChanged;

        /// <summary>
        /// Event triggered when a player starts using the station.
        /// </summary>
        public event Action<ulong> OnPlayerStartedUsing;

        /// <summary>
        /// Event triggered when a player stops using the station.
        /// </summary>
        public event Action<ulong> OnPlayerStoppedUsing;

        /// <summary>
        /// Get the current user ID.
        /// </summary>
        public ulong CurrentUserId => _currentUserId.Value;

        /// <summary>
        /// Get whether the station is locked.
        /// </summary>
        public bool IsLocked => _isLocked.Value;

        /// <summary>
        /// Get the current state.
        /// </summary>
        public StationState State => _state.Value;

        /// <summary>
        /// Set up network variable callbacks.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _currentUserId.OnValueChanged += OnCurrentUserIdChanged;
            _isLocked.OnValueChanged += OnIsLockedChanged;
            _state.OnValueChanged += OnStateValueChanged;
        }

        /// <summary>
        /// Clean up network variable callbacks.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            _currentUserId.OnValueChanged -= OnCurrentUserIdChanged;
            _isLocked.OnValueChanged -= OnIsLockedChanged;
            _state.OnValueChanged -= OnStateValueChanged;
        }

        /// <summary>
        /// Request to use the station.
        /// </summary>
        /// <param name="playerId">The player ID requesting to use the station</param>
        /// <param name="rpcParams">RPC parameters</param>
        [ServerRpc(RequireOwnership = false)]
        public void RequestUseStationServerRpc(ulong playerId, ServerRpcParams rpcParams = default)
        {
            // Validate sender
            if (rpcParams.Receive.SenderClientId != playerId)
            {
                GameLogger.LogWarning($"Player {playerId} tried to use station but sender was {rpcParams.Receive.SenderClientId}");
                return;
            }

            // Check if station is available
            if (_isLocked.Value && _currentUserId.Value != playerId)
            {
                // Check if lock has expired
                if (Time.time - _lockTime < _lockDuration)
                {
                    GameLogger.Log($"Station is locked by player {_currentUserId.Value}");
                    return;
                }
            }

            // Lock the station for this player
            _currentUserId.Value = playerId;
            _isLocked.Value = true;
            _lockTime = Time.time;

            GameLogger.Log($"Player {playerId} started using station");
        }

        /// <summary>
        /// Release the station.
        /// </summary>
        /// <param name="playerId">The player ID releasing the station</param>
        /// <param name="rpcParams">RPC parameters</param>
        [ServerRpc(RequireOwnership = false)]
        public void ReleaseStationServerRpc(ulong playerId, ServerRpcParams rpcParams = default)
        {
            // Validate sender
            if (rpcParams.Receive.SenderClientId != playerId)
            {
                GameLogger.LogWarning($"Player {playerId} tried to release station but sender was {rpcParams.Receive.SenderClientId}");
                return;
            }

            // Check if this player is using the station
            if (_currentUserId.Value != playerId)
            {
                GameLogger.LogWarning($"Player {playerId} tried to release station but it's being used by {_currentUserId.Value}");
                return;
            }

            // Release the station
            _currentUserId.Value = ulong.MaxValue;
            _isLocked.Value = false;

            GameLogger.Log($"Player {playerId} released station");
        }

        /// <summary>
        /// Set the station state (server only).
        /// </summary>
        /// <param name="newState">The new state</param>
        public void SetState(StationState newState)
        {
            if (!IsServer)
            {
                GameLogger.LogWarning("Only the server can set station state");
                return;
            }

            if (_state.Value != newState)
            {
                _state.Value = newState;
            }
        }

        /// <summary>
        /// Check if a player can use the station.
        /// </summary>
        /// <param name="playerId">The player ID to check</param>
        /// <returns>True if the player can use the station</returns>
        public bool CanPlayerUse(ulong playerId)
        {
            // Station is not locked, or locked by this player
            return !_isLocked.Value || _currentUserId.Value == playerId;
        }

        /// <summary>
        /// Handle current user ID changes.
        /// </summary>
        private void OnCurrentUserIdChanged(ulong previousValue, ulong newValue)
        {
            if (newValue != ulong.MaxValue)
            {
                OnPlayerStartedUsing?.Invoke(newValue);
            }
            else if (previousValue != ulong.MaxValue)
            {
                OnPlayerStoppedUsing?.Invoke(previousValue);
            }
        }

        /// <summary>
        /// Handle lock state changes.
        /// </summary>
        private void OnIsLockedChanged(bool previousValue, bool newValue)
        {
            // Visual feedback can be added here
        }

        /// <summary>
        /// Handle state changes.
        /// </summary>
        private void OnStateValueChanged(StationState previousValue, StationState newValue)
        {
            OnStateChanged?.Invoke(newValue);
        }

        /// <summary>
        /// Update station visuals on all clients.
        /// </summary>
        /// <param name="newState">The new state</param>
        [ClientRpc]
        public void UpdateStationVisualsClientRpc(StationState newState)
        {
            // Subclasses can override to update visuals
            OnStateChanged?.Invoke(newState);
        }
    }

    /// <summary>
    /// Station states.
    /// </summary>
    public enum StationState
    {
        Idle,
        InUse,
        Processing,
        Complete,
        Error
    }
}
