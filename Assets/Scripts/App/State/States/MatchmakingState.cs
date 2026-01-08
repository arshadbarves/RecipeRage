using Modules.Shared.Interfaces;
using Modules.Networking;
using Modules.Networking.Common;
using Modules.UI;
using UnityEngine;
using VContainer;
using Cysharp.Threading.Tasks;
using Modules.Logging;
using Modules.UI;

namespace App.State.States
{
    /// <summary>
    /// State for matchmaking - handles searching for players and bot filling on timeout
    /// Owns all matchmaking logic and timeout detection
    /// </summary>
    public class MatchmakingState : BaseState
    {
        private readonly IUIService _uiService;
        private readonly SessionManager _sessionManager;
        private readonly IGameStateManager _stateManager;
        private readonly Core.Maintenance.IMaintenanceService _maintenanceService;

        private INetworkingServices _networkingServices;
        private bool _isMatchmakingInProgress;

        // Timeout tracking
        private float _searchStartTime;
        private float _searchTime;
        private const float SearchTimeout = 6f;
        private bool _hasFilledWithBots;

        // Matchmaking parameters
        private GameMode _gameMode = GameMode.Classic;
        private int _teamSize = 4;

        /// <summary>
        /// Constructor with dependencies
        /// </summary>
        public MatchmakingState(
            IUIService uiService,
            SessionManager sessionManager,
            IGameStateManager stateManager,
            Core.Maintenance.IMaintenanceService maintenanceService)
        {
            _uiService = uiService;
            _sessionManager = sessionManager;
            _stateManager = stateManager;
            _maintenanceService = maintenanceService;
        }

        public override void Enter()
        {
            base.Enter();

            // Check maintenance before starting matchmaking
            CheckMaintenanceAndStartAsync().Forget();
        }

        private async UniTaskVoid CheckMaintenanceAndStartAsync()
        {
            if (_maintenanceService != null)
            {
                bool isInMaintenance = await _maintenanceService.CheckMaintenanceStatusAsync();
                if (isInMaintenance)
                {
                    LogMessage("Matchmaking blocked - server is in maintenance mode");
                    ReturnToMainMenu();
                    return;
                }
            }

            StartMatchmaking();
        }

        private void StartMatchmaking()
        {
            var sessionContainer = _sessionManager?.SessionContainer;
            if (sessionContainer != null)
            {
                _networkingServices = sessionContainer.Resolve<INetworkingServices>();
            }

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
            if (_uiService != null)
            {
                // Hide main menu
                _uiService.HideScreen(UIScreenType.MainMenu, true);

                // Show matchmaking screen
                _uiService.ShowScreen(UIScreenType.Matchmaking, true, false);
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
            _uiService?.HideScreen(UIScreenType.Matchmaking, true);

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
            _stateManager?.ChangeState<GameplayState>();
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
            // Access NetworkingServices via Session
            var sessionContainer = _sessionManager?.SessionContainer;
            if (sessionContainer != null)
            {
                var networking = sessionContainer.Resolve<INetworkingServices>();
                var lobbyManager = networking?.LobbyManager;

                // Leave match lobby if we're in one
                if (lobbyManager != null && lobbyManager.IsInMatchLobby)
                {
                    LogMessage("Leaving match lobby due to matchmaking failure/cancellation");
                    lobbyManager.LeaveMatchLobby();
                }
            }
        }

        private void ReturnToMainMenu()
        {
            _stateManager?.ChangeState<MainMenuState>();
        }

        #endregion
    }
}