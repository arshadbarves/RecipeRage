using System;
using Gameplay.App.State.States;
using Gameplay.Bootstrap;
using Core.Logging;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Gameplay.App.State
{
    /// <summary>
    /// Manages game state synchronization across the network.
    /// Integrates with existing GameStateManager.
    /// </summary>
    public class NetworkGameStateManager : NetworkBehaviour
    {
        /// <summary>
        /// The current game phase.
        /// </summary>
        private NetworkVariable<GamePhase> _currentPhase = new NetworkVariable<GamePhase>(GamePhase.Waiting);

        /// <summary>
        /// The time when the current phase started.
        /// </summary>
        private NetworkVariable<float> _phaseStartTime = new NetworkVariable<float>(0f);

        /// <summary>
        /// The duration of the current phase.
        /// </summary>
        private NetworkVariable<float> _phaseDuration = new NetworkVariable<float>(0f);

        [Inject]
        private IGameStateManager _gameStateManager;

        /// <summary>
        /// Event triggered when the game phase changes.
        /// </summary>
        public event Action<GamePhase> OnPhaseChanged;

        /// <summary>
        /// Get the current phase.
        /// </summary>
        public GamePhase CurrentPhase => _currentPhase.Value;

        /// <summary>
        /// Get the time remaining in the current phase.
        /// </summary>
        public float TimeRemaining
        {
            get
            {
                if (_phaseDuration.Value <= 0)
                {
                    return 0f;
                }

                float elapsed = Time.time - _phaseStartTime.Value;
                return Mathf.Max(0f, _phaseDuration.Value - elapsed);
            }
        }

        /// <summary>
        /// Initialize the network game state manager.
        /// </summary>
        private void Awake()
        {
            var scope = LifetimeScope.Find<GameLifetimeScope>();
            if (scope != null)
            {
                scope.Container.Inject(this);
            }
        }

        /// <summary>
        /// Set up network variable callbacks.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _currentPhase.OnValueChanged += OnCurrentPhaseChanged;
        }

        /// <summary>
        /// Clean up network variable callbacks.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            _currentPhase.OnValueChanged -= OnCurrentPhaseChanged;
        }

        /// <summary>
        /// Request to start the game.
        /// </summary>
        /// <param name="rpcParams">RPC parameters</param>
        [ServerRpc(RequireOwnership = false)]
        public void RequestStartGameServerRpc(ServerRpcParams rpcParams = default)
        {
            // Only allow host to start game
            if (rpcParams.Receive.SenderClientId != NetworkManager.Singleton.LocalClientId)
            {
                GameLogger.LogWarning("Only host can start the game");
                return;
            }

            // Start preparation phase
            ChangePhase(GamePhase.Preparation, 10f);
        }

        /// <summary>
        /// Change the game phase (server only).
        /// </summary>
        /// <param name="newPhase">The new phase</param>
        /// <param name="duration">The duration of the phase</param>
        public void ChangePhase(GamePhase newPhase, float duration)
        {
            if (!IsServer)
            {
                GameLogger.LogWarning("Only the server can change game phase");
                return;
            }

            _currentPhase.Value = newPhase;
            _phaseStartTime.Value = Time.time;
            _phaseDuration.Value = duration;

            // Notify clients
            ChangePhaseClientRpc(newPhase, duration);

            // Integrate with existing state manager
            if (_gameStateManager != null)
            {
                switch (newPhase)
                {
                    case GamePhase.Waiting:
                        _gameStateManager.ChangeState<LobbyState>();
                        break;
                    case GamePhase.Preparation:
                        // Stay in lobby or transition to game
                        break;
                    case GamePhase.Playing:
                        _gameStateManager.ChangeState<GameplayState>();
                        break;
                    case GamePhase.Results:
                        _gameStateManager.ChangeState<GameOverState>();
                        break;
                }
            }

            GameLogger.Log($"Changed phase to {newPhase} for {duration} seconds");
        }

        /// <summary>
        /// Update the game state.
        /// </summary>
        private void Update()
        {
            if (!IsServer)
            {
                return;
            }

            // Check for phase transitions
            if (_phaseDuration.Value > 0 && TimeRemaining <= 0)
            {
                HandlePhaseTimeout();
            }
        }

        /// <summary>
        /// Handle phase timeout.
        /// </summary>
        private void HandlePhaseTimeout()
        {
            switch (_currentPhase.Value)
            {
                case GamePhase.Preparation:
                    // Start playing phase (e.g., 180 seconds = 3 minutes)
                    ChangePhase(GamePhase.Playing, 180f);
                    break;

                case GamePhase.Playing:
                    // End game and show results
                    ChangePhase(GamePhase.Results, 30f);
                    break;

                case GamePhase.Results:
                    // Return to waiting
                    ChangePhase(GamePhase.Waiting, 0f);
                    break;
            }
        }

        /// <summary>
        /// Handle current phase changes.
        /// </summary>
        private void OnCurrentPhaseChanged(GamePhase previousValue, GamePhase newValue)
        {
            OnPhaseChanged?.Invoke(newValue);

            GameLogger.Log($"Phase changed from {previousValue} to {newValue}");
        }

        /// <summary>
        /// Notify clients of phase change.
        /// </summary>
        [ClientRpc]
        private void ChangePhaseClientRpc(GamePhase newPhase, float duration)
        {
            // Clients receive the phase change notification
            // The NetworkVariable will already be updated
            GameLogger.Log($"Client received phase change to {newPhase}");
        }

        /// <summary>
        /// Show countdown on all clients.
        /// </summary>
        /// <param name="seconds">The number of seconds to count down</param>
        [ClientRpc]
        public void ShowCountdownClientRpc(int seconds)
        {
            // UI can subscribe to this to show countdown
            GameLogger.Log($"Countdown: {seconds}");
        }
    }

    /// <summary>
    /// Game phases.
    /// </summary>
    public enum GamePhase
    {
        Waiting,      // Waiting for players
        Preparation,  // 10 second countdown
        Playing,      // Active gameplay
        Results       // Show scores
    }
}
