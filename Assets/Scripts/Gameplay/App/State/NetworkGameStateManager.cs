using System;
using Gameplay.Bootstrap;
using Core.Logging;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Gameplay.App.State
{
    /// <summary>
    /// Manages game phase synchronization across network.
    /// Controls preparation countdown and game duration.
    /// </summary>
    public class NetworkGameStateManager : NetworkBehaviour
    {
        /// <summary>
        /// Whether game is currently in preparation phase (countdown).
        /// </summary>
        private NetworkVariable<bool> _isInPreparation = new NetworkVariable<bool>(false);

        /// <summary>
        /// Whether game is currently in playing phase.
        /// </summary>
        private NetworkVariable<bool> _isInPlaying = new NetworkVariable<bool>(false);

        /// <summary>
        /// The time when current phase started.
        /// </summary>
        private NetworkVariable<float> _phaseStartTime = new NetworkVariable<float>(0f);

        /// <summary>
        /// The duration of current phase.
        /// </summary>
        private NetworkVariable<float> _phaseDuration = new NetworkVariable<float>(0f);

        [Inject]
        private IGameStateManager _gameStateManager;

        /// <summary>
        /// Event triggered when game phase changes.
        /// </summary>
        public event Action<bool> OnPreparationChanged;
        public event Action<bool> OnPlayingChanged;
        public event Action<GamePhase> OnPhaseChanged;

        /// <summary>
        /// Get whether game is in preparation phase.
        /// </summary>
        public bool IsInPreparation => _isInPreparation.Value;

        /// <summary>
        /// Get whether game is in playing phase.
        /// </summary>
        public bool IsInPlaying => _isInPlaying.Value;

        /// <summary>
        /// Get current game phase.
        /// </summary>
        public GamePhase Phase
        {
            get
            {
                if (_isInPreparation.Value)
                {
                    return GamePhase.Preparation;
                }
                else if (_isInPlaying.Value)
                {
                    return GamePhase.Playing;
                }
                else
                {
                    return GamePhase.Waiting;
                }
            }
        }

        /// <summary>
        /// Get time remaining in current phase.
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

            _isInPreparation.OnValueChanged += OnIsInPreparationChanged;
            _isInPlaying.OnValueChanged += OnIsInPlayingChanged;
        }

        /// <summary>
        /// Clean up network variable callbacks.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            _isInPreparation.OnValueChanged -= OnIsInPreparationChanged;
            _isInPlaying.OnValueChanged -= OnIsInPlayingChanged;
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
                GameLogger.LogWarning("Only host can start game");
                return;
            }

            // Start preparation phase (countdown)
            StartPreparationPhase(10f);
        }

        /// <summary>
        /// Start preparation phase (countdown).
        /// </summary>
        /// <param name="duration">Countdown duration in seconds</param>
        public void StartPreparationPhase(float duration)
        {
            if (!IsServer)
            {
                GameLogger.LogWarning("Only server can start preparation phase");
                return;
            }

            _isInPreparation.Value = true;
            _isInPlaying.Value = false;
            _phaseStartTime.Value = Time.time;
            _phaseDuration.Value = duration;

            ShowCountdownClientRpc(Mathf.RoundToInt(duration));

            GameLogger.Log($"Started preparation phase for {duration} seconds");
        }

        /// <summary>
        /// Start playing phase.
        /// </summary>
        /// <param name="duration">Game duration in seconds (default: 180s = 3 minutes)</param>
        public void StartPlayingPhase(float duration = 180f)
        {
            if (!IsServer)
            {
                GameLogger.LogWarning("Only server can start playing phase");
                return;
            }

            _isInPreparation.Value = false;
            _isInPlaying.Value = true;
            _phaseStartTime.Value = Time.time;
            _phaseDuration.Value = duration;

            GameLogger.Log($"Started playing phase for {duration} seconds");
        }

        /// <summary>
        /// End current game phase.
        /// </summary>
        public void EndGame()
        {
            if (!IsServer)
            {
                GameLogger.LogWarning("Only server can end game");
                return;
            }

            _isInPreparation.Value = false;
            _isInPlaying.Value = false;
            _phaseDuration.Value = 0f;

            GameLogger.Log("Ended game phase");
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

        private void HandlePhaseTimeout()
        {
            if (_isInPreparation.Value)
            {
                // Transition to playing
                StartPlayingPhase(180f);
            }
            else if (_isInPlaying.Value)
            {
                // End game
                EndGame();
            }
        }

        private void OnIsInPreparationChanged(bool previousValue, bool newValue)
        {
            OnPreparationChanged?.Invoke(newValue);
            OnPhaseChanged?.Invoke(Phase);
            GameLogger.Log($"Preparation phase: {newValue}");
        }

        private void OnIsInPlayingChanged(bool previousValue, bool newValue)
        {
            OnPlayingChanged?.Invoke(newValue);
            OnPhaseChanged?.Invoke(Phase);
            GameLogger.Log($"Playing phase: {newValue}");
        }

        /// <summary>
        /// Show countdown on all clients.
        /// </summary>
        /// <param name="seconds">The number of seconds to count down</param>
        [ClientRpc]
        private void ShowCountdownClientRpc(int seconds)
        {
            // UI can subscribe to this to show countdown
            GameLogger.Log($"Countdown: {seconds} seconds");
        }
    }
}
