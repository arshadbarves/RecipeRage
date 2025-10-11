using System;
using Core.Networking;
using Core.Networking.Common;
using Core.Networking.EOS;
using UI.UISystem;
using UnityEngine;

namespace Core.State.States
{
    /// <summary>
    /// State for matchmaking and finding players for a game.
    /// </summary>
    public class MatchmakingState : BaseState
    {

        /// <summary>
        /// Flag to track if matchmaking is in progress.
        /// </summary>
        private bool _isMatchmakingInProgress;

        // No need to store references to UI elements anymore as they're managed by the UIManager

        /// <summary>
        /// Reference to the network manager.
        /// </summary>
        private RecipeRageNetworkManager _networkManager;

        /// <summary>
        /// Reference to the lobby manager.
        /// </summary>
        private RecipeRageLobbyManager _lobbyManager;

        /// <summary>
        /// Event triggered when matchmaking is complete.
        /// </summary>
        public event Action<bool> OnMatchmakingComplete;

        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public override void Enter()
        {
            base.Enter();

            // Get reference to the network manager and lobby manager
            _networkManager = RecipeRageNetworkManager.Instance;
            _lobbyManager = _networkManager?.LobbyManager;

            // Reset matchmaking state
            _isMatchmakingInProgress = true;

            // Start matchmaking process
            LogMessage("Starting matchmaking process");

            // Show game mode selection screen
            UIManager.Instance.ShowScreen(UIScreenType.GameModeSelection, true, true);

            // Subscribe to lobby events
            if (_lobbyManager != null)
            {
                _lobbyManager.OnLobbyUpdated += HandleLobbyUpdated;
                _networkManager.OnGameStarted += HandleGameStarted;

                // Create a new lobby by default
                string sessionName = $"RecipeRage_{DateTime.Now.Ticks}";
                _networkManager.CreateGame(sessionName, GameMode.Classic, "Kitchen", 4, false);
            }
            else
            {
                LogError("RecipeRageNetworkManager or LobbyManager instance not found");
                CompleteMatchmaking(false);
            }
        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public override void Exit()
        {
            base.Exit();

            // Unsubscribe from lobby events
            if (_lobbyManager != null)
            {
                _lobbyManager.OnLobbyUpdated -= HandleLobbyUpdated;
            }

            if (_networkManager != null)
            {
                _networkManager.OnGameStarted -= HandleGameStarted;
            }

            // Cancel matchmaking if still in progress
            if (_isMatchmakingInProgress)
            {
                CancelMatchmaking();
            }

            // Hide game mode selection screen
            UIManager.Instance.HideScreen(UIScreenType.GameModeSelection, true);
        }

        /// <summary>
        /// Called every frame to update the state.
        /// </summary>
        public override void Update()
        {
            // Matchmaking logic is now handled by the NetworkLobbyManager
        }

        /// <summary>
        /// Called at fixed intervals for physics updates.
        /// </summary>
        public override void FixedUpdate()
        {
            // Matchmaking doesn't need physics updates
        }

        // UI is now managed by the UIManager

        /// <summary>
        /// Handle lobby updated event.
        /// </summary>
        private void HandleLobbyUpdated()
        {
            LogMessage("Lobby updated");

            // If this is the first update, transition to the lobby screen
            if (_isMatchmakingInProgress)
            {
                // Show the lobby screen
                // Note: We're using the existing GameModeSelectionScreen for now
                // UIManager.Instance.ShowScreen<LobbyScreen>();

                // Mark matchmaking as complete
                CompleteMatchmaking(true);
            }
        }

        /// <summary>
        /// Handle game started event.
        /// </summary>
        private void HandleGameStarted()
        {
            LogMessage("Game started");

            // Complete matchmaking successfully if not already done
            if (_isMatchmakingInProgress)
            {
                CompleteMatchmaking(true);
            }

            // Transition to gameplay state
            GameStateManager.Instance.ChangeState(new GameplayState());
        }

        /// <summary>
        /// Called when matchmaking is complete.
        /// </summary>
        /// <param name="success"> Whether matchmaking was successful </param>
        private void CompleteMatchmaking(bool success)
        {
            if (!_isMatchmakingInProgress)
            {
                return;
            }

            _isMatchmakingInProgress = false;
            LogMessage($"Matchmaking complete. Success: {success}");

            // Trigger the matchmaking complete event
            OnMatchmakingComplete?.Invoke(success);
        }

        /// <summary>
        /// Cancels the matchmaking process.
        /// </summary>
        public void CancelMatchmaking()
        {
            if (!_isMatchmakingInProgress)
            {
                return;
            }

            _isMatchmakingInProgress = false;
            LogMessage("Matchmaking canceled");

            // Leave the game if we're in one
            if (_networkManager != null)
            {
                _networkManager.LeaveGame();
            }

            // Trigger the matchmaking complete event with failure
            OnMatchmakingComplete?.Invoke(false);
        }
    }
}