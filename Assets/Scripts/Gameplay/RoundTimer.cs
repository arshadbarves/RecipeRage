using System;
using Core.Logging;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Synchronized round timer for gameplay.
    /// Follows Single Responsibility Principle - handles only time tracking.
    /// </summary>
    public class RoundTimer : NetworkBehaviour
    {
        /// <summary>
        /// The time remaining in the round.
        /// </summary>
        private NetworkVariable<float> _timeRemaining = new NetworkVariable<float>(0f);

        /// <summary>
        /// Whether the timer is running.
        /// </summary>
        private NetworkVariable<bool> _isRunning = new NetworkVariable<bool>(false);

        /// <summary>
        /// The duration of the round.
        /// </summary>
        private float _duration;

        /// <summary>
        /// Event triggered when the timer updates.
        /// </summary>
        public event Action<float> OnTimeUpdated;

        /// <summary>
        /// Event triggered when the timer expires.
        /// </summary>
        public event Action OnTimerExpired;

        /// <summary>
        /// Get the time remaining.
        /// </summary>
        public float TimeRemaining => _timeRemaining.Value;

        /// <summary>
        /// Get whether the timer is running.
        /// </summary>
        public bool IsRunning => _isRunning.Value;

        /// <summary>
        /// Set up network variable callbacks.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _timeRemaining.OnValueChanged += OnTimeRemainingChanged;
            _isRunning.OnValueChanged += OnIsRunningChanged;
        }

        /// <summary>
        /// Clean up network variable callbacks.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            _timeRemaining.OnValueChanged -= OnTimeRemainingChanged;
            _isRunning.OnValueChanged -= OnIsRunningChanged;
        }

        /// <summary>
        /// Update the timer.
        /// </summary>
        private void Update()
        {
            // Only update on server
            if (!IsServer || !_isRunning.Value)
            {
                return;
            }

            // Decrease time
            _timeRemaining.Value -= Time.deltaTime;

            // Check if timer expired
            if (_timeRemaining.Value <= 0)
            {
                _timeRemaining.Value = 0;
                _isRunning.Value = false;

                // Notify clients
                TimerExpiredClientRpc();
            }
        }

        /// <summary>
        /// Start the timer.
        /// </summary>
        /// <param name="duration">The duration in seconds</param>
        /// <param name="rpcParams">RPC parameters</param>
        [ServerRpc(RequireOwnership = false)]
        public void StartTimerServerRpc(float duration, ServerRpcParams rpcParams = default)
        {
            _duration = duration;
            _timeRemaining.Value = duration;
            _isRunning.Value = true;

            GameLogger.Log($"Started timer for {duration} seconds");
        }

        /// <summary>
        /// Pause the timer.
        /// </summary>
        /// <param name="rpcParams">RPC parameters</param>
        [ServerRpc(RequireOwnership = false)]
        public void PauseTimerServerRpc(ServerRpcParams rpcParams = default)
        {
            _isRunning.Value = false;

            GameLogger.Log("Paused timer");
        }

        /// <summary>
        /// Resume the timer.
        /// </summary>
        /// <param name="rpcParams">RPC parameters</param>
        [ServerRpc(RequireOwnership = false)]
        public void ResumeTimerServerRpc(ServerRpcParams rpcParams = default)
        {
            if (_timeRemaining.Value > 0)
            {
                _isRunning.Value = true;
                GameLogger.Log("Resumed timer");
            }
        }

        /// <summary>
        /// Stop the timer.
        /// </summary>
        /// <param name="rpcParams">RPC parameters</param>
        [ServerRpc(RequireOwnership = false)]
        public void StopTimerServerRpc(ServerRpcParams rpcParams = default)
        {
            _timeRemaining.Value = 0;
            _isRunning.Value = false;

            GameLogger.Log("Stopped timer");
        }

        /// <summary>
        /// Add time to the timer.
        /// </summary>
        /// <param name="seconds">The number of seconds to add</param>
        public void AddTime(float seconds)
        {
            if (!IsServer)
            {
                GameLogger.LogWarning("Only the server can add time");
                return;
            }

            _timeRemaining.Value += seconds;
            GameLogger.Log($"Added {seconds} seconds to timer");
        }

        /// <summary>
        /// Sync time to all clients.
        /// </summary>
        /// <param name="timeRemaining">The time remaining</param>
        [ClientRpc]
        public void SyncTimeClientRpc(float timeRemaining)
        {
            // Force sync time on all clients
            // Useful for correcting drift
            if (!IsServer)
            {
                _timeRemaining.Value = timeRemaining;
            }
        }

        /// <summary>
        /// Handle time remaining changes.
        /// </summary>
        private void OnTimeRemainingChanged(float previousValue, float newValue)
        {
            OnTimeUpdated?.Invoke(newValue);
        }

        /// <summary>
        /// Handle running state changes.
        /// </summary>
        private void OnIsRunningChanged(bool previousValue, bool newValue)
        {
            if (!newValue && previousValue && _timeRemaining.Value <= 0)
            {
                OnTimerExpired?.Invoke();
            }
        }

        /// <summary>
        /// Notify clients that the timer expired.
        /// </summary>
        [ClientRpc]
        private void TimerExpiredClientRpc()
        {
            OnTimerExpired?.Invoke();
            GameLogger.Log("Timer expired");
        }
    }
}
