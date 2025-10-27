using System;
using Core.Bootstrap;
using Core.Networking;
using Core.Networking.Common;
using UI;
using UnityEngine;

namespace Core.State.States
{
    /// <summary>
    /// State for matchmaking and finding players for a game.
    /// Updated to use new NetworkingServiceContainer architecture
    /// </summary>
    public class MatchmakingState : BaseState
    {
        /// <summary>
        /// Flag to track if matchmaking is in progress.
        /// </summary>
        private bool _isMatchmakingInProgress;

        /// <summary>
        /// Reference to the networking services
        /// </summary>
        private INetworkingServices _networkingServices;

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

            // Get reference to networking services
            _networkingServices = GameBootstrap.Services?.NetworkingServices;

            if (_networkingServices == null)
            {
                LogError("NetworkingServices not available");
                CompleteMatchmaking(false);
                return;
            }

            // Reset matchmaking state
            _isMatchmakingInProgress = true;

            // Start matchmaking process
            LogMessage("Starting matchmaking process");

            // Show game mode selection screen
            GameBootstrap.Services?.UIService.ShowScreen(UIScreenType.GameModeSelection, true, true);

            // Subscribe to matchmaking events
            _networkingServices.MatchmakingService.OnMatchFound += HandleMatchFound;
            _networkingServices.MatchmakingService.OnMatchmakingFailed += HandleMatchmakingFailed;
            _networkingServices.MatchmakingService.OnPlayersFound += HandlePlayersFound;

            // Start matchmaking (default to 4v4 Classic mode)
            _networkingServices.MatchmakingService.FindMatch(GameMode.Classic, teamSize: 4);
            
            LogMessage("Matchmaking started - searching for players...");
        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public override void Exit()
        {
            base.Exit();

            // Unsubscribe from matchmaking events
            if (_networkingServices != null)
            {
                _networkingServices.MatchmakingService.OnMatchFound -= HandleMatchFound;
                _networkingServices.MatchmakingService.OnMatchmakingFailed -= HandleMatchmakingFailed;
                _networkingServices.MatchmakingService.OnPlayersFound -= HandlePlayersFound;
            }

            // Cancel matchmaking if still in progress
            if (_isMatchmakingInProgress)
            {
                CancelMatchmaking();
            }

            // Hide game mode selection screen
            GameBootstrap.Services?.UIService.HideScreen(UIScreenType.GameModeSelection, true);
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

        // UI is now managed by the UIService

        /// <summary>
        /// Handle match found event
        /// </summary>
        private void HandleMatchFound(LobbyInfo lobbyInfo)
        {
            LogMessage($"Match found! Lobby: {lobbyInfo.LobbyId}, Players: {lobbyInfo.CurrentPlayers}/{lobbyInfo.MaxPlayers}");

            // Complete matchmaking successfully
            CompleteMatchmaking(true);

            // Transition to gameplay state
            var services = GameBootstrap.Services;
            if (services != null)
            {
                services.StateManager.ChangeState(new GameplayState());
            }
        }

        /// <summary>
        /// Handle matchmaking failed event
        /// </summary>
        private void HandleMatchmakingFailed(string reason)
        {
            LogError($"Matchmaking failed: {reason}");

            // Complete matchmaking with failure
            CompleteMatchmaking(false);

            // Return to main menu
            var services = GameBootstrap.Services;
            if (services != null)
            {
                services.StateManager.ChangeState(new MainMenuState());
            }
        }

        /// <summary>
        /// Handle players found update
        /// </summary>
        private void HandlePlayersFound(int current, int required)
        {
            LogMessage($"Players found: {current}/{required}");
            
            // Update UI with progress (if we have a matchmaking screen)
            // TODO: Update matchmaking UI with player count
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

            // Cancel matchmaking via service
            if (_networkingServices != null)
            {
                _networkingServices.MatchmakingService.CancelMatchmaking();
            }

            // Trigger the matchmaking complete event with failure
            OnMatchmakingComplete?.Invoke(false);
        }
    }
}