using System;
using KitchenClash.Domain;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// Synchronized round timer for gameplay.
    /// </summary>
    public class RoundTimer : NetworkBehaviour
    {
        private NetworkVariable<float> _timeRemaining = new NetworkVariable<float>(0f);
        private NetworkVariable<bool> _isRunning = new NetworkVariable<bool>(false);
        private float _duration;

        [Inject] private IEventBus _eventBus;
        [Inject] private IScoreService _scoreService;

        public event Action<float> OnTimeUpdated;
        public event Action OnTimerExpired;

        public float TimeRemaining => _timeRemaining.Value;
        public bool IsRunning => _isRunning.Value;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _timeRemaining.OnValueChanged += OnTimeRemainingChanged;
            _isRunning.OnValueChanged += OnIsRunningChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            _timeRemaining.OnValueChanged -= OnTimeRemainingChanged;
            _isRunning.OnValueChanged -= OnIsRunningChanged;
        }

        private void Update()
        {
            if (!IsServer || !_isRunning.Value)
            {
                return;
            }

            _timeRemaining.Value -= Time.deltaTime;

            _scoreService?.UpdateMatchTime(_timeRemaining.Value);

            if (_timeRemaining.Value <= 0)
            {
                _timeRemaining.Value = 0;
                _isRunning.Value = false;

                TimerExpiredClientRpc();
                TriggerGameOverShakeClientRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void StartTimerServerRpc(float duration, ServerRpcParams rpcParams = default)
        {
            StartTimer(duration);
        }

        [ServerRpc(RequireOwnership = false)]
        public void PauseTimerServerRpc(ServerRpcParams rpcParams = default)
        {
            PauseTimer();
        }

        [ServerRpc(RequireOwnership = false)]
        public void ResumeTimerServerRpc(ServerRpcParams rpcParams = default)
        {
            ResumeTimer();
        }

        [ServerRpc(RequireOwnership = false)]
        public void StopTimerServerRpc(ServerRpcParams rpcParams = default)
        {
            StopTimer();
        }

        public void StartTimer(float duration)
        {
            if (!CanMutateTimer())
            {
                return;
            }

            _duration = duration;
            _timeRemaining.Value = Mathf.Max(0f, duration);
            _isRunning.Value = duration > 0f;

            GameLogger.Log($"Started timer for {duration} seconds");
        }

        public void PauseTimer()
        {
            if (!CanMutateTimer())
            {
                return;
            }

            _isRunning.Value = false;
            GameLogger.Log("Paused timer");
        }

        public void ResumeTimer()
        {
            if (!CanMutateTimer())
            {
                return;
            }

            if (_timeRemaining.Value > 0)
            {
                _isRunning.Value = true;
                GameLogger.Log("Resumed timer");
            }
        }

        public void StopTimer()
        {
            if (!CanMutateTimer())
            {
                return;
            }

            _timeRemaining.Value = 0;
            _isRunning.Value = false;
            GameLogger.Log("Stopped timer");
        }

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

        [ClientRpc]
        public void SyncTimeClientRpc(float timeRemaining)
        {
            if (!IsServer)
            {
                _timeRemaining.Value = timeRemaining;
            }
        }

        private void OnTimeRemainingChanged(float previousValue, float newValue)
        {
            OnTimeUpdated?.Invoke(newValue);
        }

        private void OnIsRunningChanged(bool previousValue, bool newValue)
        {
            if (!newValue && previousValue && _timeRemaining.Value <= 0)
            {
                OnTimerExpired?.Invoke();
            }
        }

        private bool CanMutateTimer()
        {
            if (IsSpawned && !IsServer)
            {
                GameLogger.LogWarning("Only the server can mutate the round timer");
                return false;
            }

            return true;
        }

        [ClientRpc]
        private void TimerExpiredClientRpc()
        {
            OnTimerExpired?.Invoke();
            GameLogger.Log("Timer expired");
        }

        [ClientRpc]
        private void TriggerGameOverShakeClientRpc()
        {
            _eventBus?.Publish(new CameraShakeEvent(1.0f, 0.8f));
        }
    }
}
