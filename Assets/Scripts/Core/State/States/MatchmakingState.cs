using System;
using Core.Bootstrap;
using Core.Networking;
using Core.Networking.Common;
using UI;
using UnityEngine;

namespace Core.State.States
{
    /// <summary>
    /// State for matchmaking - handles searching for players and bot filling on timeout
    /// Owns all matchmaking logic and timeout detection
    /// </summary>
    public class MatchmakingState : BaseState
    {
        private INetworkingServices _networkingServices;
        private bool _isMatchmakingInProgress;

        // Timeout tracking
        private float _searchStartTime;
        private float _searchTime;
        private const float SearchTimeout = 6f;
        private bool _hasFilledWithBots;

        // Matchmaking parameters
        private GameMode _gameMode;
        private int _teamSize;

        /// <summary>
        /// Constructor with matchmaking parameters
        /// </summary>
        public MatchmakingState(GameMode gameMode = GameMode.Classic, int teamSize = 4)
        {
            _gameMode = gameMode;
            _teamSize = teamSize;
        }

        public override void Enter()
        {
            base.Enter();

            _networkingServices = GameBootstrap.Services.Session.NetworkingServices;

            if (_networkingServices == null)
            {
                LogError("NetworkingServices not available");
                ReturnToMainMenu();
                return;
            }

            // Initialize state
            _isMatchmakingInProgress = true;
            _searchStartTime = Time.time;
            _searchTime = 0f;
            _hasFilledWithBots = false;

            LogMessage($"Starting matchmaking: {_gameMode}, Team Size: {_teamSize}");

            // Show matchmaking UI
            var uiService = GameBootstrap.Services?.UIService;
            if (uiService != null)
            {
                // Hide main menu
                uiService.HideScreen(UIScreenType.MainMenu, true);

                // Show matchmaking screen
                uiService.ShowScreen(UIScreenType.Matchmaking, true, false);
                LogMessage("Matchmaking screen shown");
            }

            // Subscribe to matchmaking events
            _networkingServices.MatchmakingService.OnMatchFound += HandleMatchFound;
            _networkingServices.MatchmakingService.OnMatchmakingFailed += HandleMatchmakingFailed;
            _networkingServices.MatchmakingService.OnPlayersFound += HandlePlayersFound;
            _networkingServices.MatchmakingService.OnMatchmakingCancelled += HandleMatchmakingCancelled;

            // Start matchmaking
            _networkingServices.MatchmakingService.FindMatch(_gameMode, _teamSize);

            LogMessage("Matchmaking started - searching for players...");
        }

        public override void Exit()
        {
            base.Exit();

            // Hide matchmaking UI
            var uiService = GameBootstrap.Services?.UIService;
            if (uiService != null)
            {
                uiService.HideScreen(UIScreenType.Matchmaking, true);
            }

            // Unsubscribe from events
            if (_networkingServices != null)
            {
                _networkingServices.MatchmakingService.OnMatchFound -= HandleMatchFound;
                _networkingServices.MatchmakingService.OnMatchmakingFailed -= HandleMatchmakingFailed;
                _networkingServices.MatchmakingService.OnPlayersFound -= HandlePlayersFound;
                _networkingServices.MatchmakingService.OnMatchmakingCancelled -= HandleMatchmakingCancelled;
            }

            // Cancel matchmaking if still in progress
            if (_isMatchmakingInProgress && _networkingServices != null)
            {
                _networkingServices.MatchmakingService.CancelMatchmaking();
            }

            _isMatchmakingInProgress = false;
        }

        public override void Update()
        {
            if (!_isMatchmakingInProgress || _networkingServices == null)
                return;

            // Update search time
            _searchTime = Time.time - _searchStartTime;

            // Check for timeout - trigger bot filling
            if (_searchTime >= SearchTimeout && !_hasFilledWithBots)
            {
                LogMessage($"Matchmaking timeout after {_searchTime:F1}s - filling with bots");
                _hasFilledWithBots = true;

                // Tell service to fill with bots
                _networkingServices.MatchmakingService.FillMatchWithBots();
            }
        }

        public override void FixedUpdate()
        {
            // No physics needed for matchmaking
        }

        #region Event Handlers

        private void HandleMatchFound(LobbyInfo lobbyInfo)
        {
            LogMessage($"Match found! Lobby: {lobbyInfo.LobbyId}, Players: {lobbyInfo.CurrentPlayers}/{lobbyInfo.MaxPlayers}");

            _isMatchmakingInProgress = false;

            // Transition to gameplay state
            var services = GameBootstrap.Services;
            if (services != null)
            {
                services.StateManager.ChangeState(new GameplayState());
            }
        }

        private void HandleMatchmakingFailed(string reason)
        {
            LogError($"Matchmaking failed: {reason}");

            _isMatchmakingInProgress = false;

            // Leave the lobby before returning to main menu
            CleanupLobby();

            // Return to main menu
            ReturnToMainMenu();
        }

        private void HandlePlayersFound(int current, int required)
        {
            LogMessage($"Players found: {current}/{required}");
            // MatchmakingScreen will update its UI via this event
        }

        private void HandleMatchmakingCancelled()
        {
            LogMessage("Matchmaking cancelled by user");

            _isMatchmakingInProgress = false;

            // Leave the lobby before returning to main menu
            CleanupLobby();

            // Return to main menu
            ReturnToMainMenu();
        }

        #endregion

        #region Helper Methods

        private void CleanupLobby()
        {
            var services = GameBootstrap.Services;
            // Access NetworkingServices via Session
            if (services?.Session?.NetworkingServices?.LobbyManager == null)
                return;

            var lobbyManager = services.Session.NetworkingServices.LobbyManager;

            // Leave match lobby if we're in one
            if (lobbyManager.IsInMatchLobby)
            {
                LogMessage("Leaving match lobby due to matchmaking failure/cancellation");
                lobbyManager.LeaveMatchLobby();
            }
        }

        private void ReturnToMainMenu()
        {
            var services = GameBootstrap.Services;
            if (services != null)
            {
                services.StateManager.ChangeState(new MainMenuState());
            }
        }

        #endregion
    }
}
