using Core.Logging;
using Core.State;
using Gameplay;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Helper component to start game rounds via NetworkGameStateManager.
    /// Handles in-game round flow (Preparation → Playing → Results).
    /// Different from Core.Networking.Services.GameStarter which handles lobby-to-game transition.
    /// </summary>
    public class RoundStarter : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _roundDuration = 180f; // 3 minutes
        [SerializeField] private bool _autoStartOnHost = false;

        private NetworkGameStateManager _networkGameStateManager;
        private RoundTimer _roundTimer;
        private bool _gameStarted = false;

        /// <summary>
        /// Initialize the game starter.
        /// </summary>
        private void Start()
        {
            _networkGameStateManager = FindFirstObjectByType<NetworkGameStateManager>();
            _roundTimer = FindFirstObjectByType<RoundTimer>();

            if (_networkGameStateManager == null)
            {
                GameLogger.LogError("NetworkGameStateManager not found in scene!");
            }

            if (_roundTimer == null)
            {
                GameLogger.LogWarning("RoundTimer not found in scene. Timer will not work.");
            }

            if (_autoStartOnHost && NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
            {
                Invoke(nameof(StartRound), 2f); // Delay to let everything initialize
            }
        }

        /// <summary>
        /// Start the round (call this from a button or after players are ready).
        /// </summary>
        public void StartRound()
        {
            if (_gameStarted)
            {
                GameLogger.LogWarning("Round already started");
                return;
            }

            if (NetworkManager.Singleton == null)
            {
                GameLogger.LogError("NetworkManager not found. Cannot start round.");
                return;
            }

            if (!NetworkManager.Singleton.IsHost)
            {
                GameLogger.LogWarning("Only the host can start the round");
                return;
            }

            if (_networkGameStateManager == null)
            {
                GameLogger.LogError("NetworkGameStateManager not found. Cannot start round.");
                return;
            }

            _gameStarted = true;

            _networkGameStateManager.RequestStartGameServerRpc();

            GameLogger.Log("Round start requested");
        }

        /// <summary>
        /// Start the round timer (called automatically by NetworkGameStateManager when Playing phase starts).
        /// You can also call this manually if needed.
        /// </summary>
        public void StartRoundTimer()
        {
            if (_roundTimer == null)
            {
                GameLogger.LogWarning("RoundTimer not found. Cannot start timer.");
                return;
            }

            if (!NetworkManager.Singleton.IsServer)
            {
                GameLogger.LogWarning("Only the server can start the timer");
                return;
            }

            _roundTimer.StartTimerServerRpc(_roundDuration);
            GameLogger.Log($"Round timer started for {_roundDuration} seconds");
        }

        /// <summary>
        /// End the round early (optional).
        /// </summary>
        public void EndRound()
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                GameLogger.LogWarning("Only the host can end the round");
                return;
            }

            if (_networkGameStateManager == null)
            {
                return;
            }

            _networkGameStateManager.ChangePhase(GamePhase.Results, 30f);

            GameLogger.Log("Round ended");
        }

        /// <summary>
        /// Reset for a new round.
        /// </summary>
        public void ResetRound()
        {
            _gameStarted = false;
        }
    }
}
