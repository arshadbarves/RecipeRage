using System;
using Core.Networking;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Core.Networking.Common;
using Core.RemoteConfig;
using Core.UI.Interfaces;
using Core.Session;
using Core.Networking.Interfaces;
using Gameplay.Match;
using Gameplay.UI.Features.MainMenu;
using Gameplay.UI.Features.Matchmaking;

namespace Gameplay.App.State.States
{
    /// <summary>
    /// State for matchmaking - handles searching for players and bot filling on timeout
    /// Owns all matchmaking logic and timeout detection
    /// </summary>
    public class MatchmakingState : BaseState
    {
        private readonly IUIService _uiService;
        private readonly ISessionContext _sessionContext;
        private readonly IGameStateManager _stateManager;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IMatchService _matchService;

        private IMatchmakingService _matchmakingService;
        private bool _isMatchmakingInProgress;

        // Timeout tracking
        private float _searchStartTime;
        private float _searchTime;
        private const float SearchTimeout = 6f;
        private bool _hasFilledWithBots;

        // Matchmaking parameters
        private string _gameModeId = "classic";
        private int _teamSize = 2;

        /// <summary>
        /// Constructor with dependencies
        /// </summary>
        public MatchmakingState(
            IUIService uiService,
            ISessionContext sessionContext,
            IGameStateManager stateManager,
            IMaintenanceService maintenanceService,
            IMatchService matchService)
        {
            _uiService = uiService;
            _sessionContext = sessionContext;
            _stateManager = stateManager;
            _maintenanceService = maintenanceService;
            _matchService = matchService;
        }

        public override void Enter()
        {
            base.Enter();

            // Check maintenance before starting matchmaking
            CheckMaintenanceAndStartAsync().Forget();
        }

        private async UniTask CheckMaintenanceAndStartAsync()
        {
            try
            {
                if (_maintenanceService != null)
                {
                    bool isInMaintenance = await _maintenanceService.CheckMaintenanceStatusAsync();
                    if (!IsStateActive) return;
                    if (isInMaintenance)
                    {
                        LogMessage("Matchmaking blocked - server is in maintenance mode");
                        ReturnToMainMenu();
                        return;
                    }
                }

                StartMatchmaking();
            }
            catch (OperationCanceledException)
            {
                LogMessage("Matchmaking startup cancelled");
            }
            catch (Exception ex)
            {
                LogError($"Failed to start matchmaking: {ex.Message}");
                ReturnToMainMenu();
            }
        }

        private void StartMatchmaking()
        {
            _matchmakingService = _sessionContext.MatchmakingService;

            var selectedGameMode = _sessionContext.GameModeService?.SelectedGameMode;
            if (selectedGameMode != null)
            {
                if (_matchService != null && _matchService.TryGetQueue(selectedGameMode.Id, out MatchQueueDefinition queue))
                {
                    _gameModeId = queue.ModeId;
                    _teamSize = Mathf.Max(1, queue.PlayersPerTeam);
                    LogMessage($"Resolved queue '{queue.DisplayName}' ({queue.TeamCount} teams, {queue.DurationSeconds}s)");
                }
                else
                {
                    _gameModeId = selectedGameMode.Id;
                    _teamSize = Mathf.Max(1, selectedGameMode.PlayersPerTeam);
                }
            }

            if (_matchmakingService == null)
            {
                LogError("MatchmakingService not available");
                ReturnToMainMenu();
                return;
            }

            // Initialize state
            _isMatchmakingInProgress = true;
            _searchStartTime = Time.time;
            _searchTime = 0f;
            _hasFilledWithBots = false;

            LogMessage($"Starting matchmaking: {_gameModeId}, Team Size: {_teamSize}");

            // Show matchmaking UI
            if (_uiService != null)
            {
                _uiService.SetRootScreen<MatchmakingView>(true);
                LogMessage("Matchmaking screen shown");
            }

            // Subscribe to matchmaking events
            _matchmakingService.OnMatchFound += HandleMatchFound;
            _matchmakingService.OnMatchmakingFailed += HandleMatchmakingFailed;
            _matchmakingService.OnPlayersFound += HandlePlayersFound;
            _matchmakingService.OnMatchmakingCancelled += HandleMatchmakingCancelled;

            // Start matchmaking
            _matchmakingService.FindMatch(_gameModeId, _teamSize);

            LogMessage("Matchmaking started - searching for players...");
        }

        public override void Exit()
        {
            base.Exit();

            // Hide matchmaking UI
            _uiService?.Hide<MatchmakingView>(true);

            // Unsubscribe from events
            if (_matchmakingService != null)
            {
                _matchmakingService.OnMatchFound -= HandleMatchFound;
                _matchmakingService.OnMatchmakingFailed -= HandleMatchmakingFailed;
                _matchmakingService.OnPlayersFound -= HandlePlayersFound;
                _matchmakingService.OnMatchmakingCancelled -= HandleMatchmakingCancelled;
            }

            // Cancel matchmaking if still in progress
            if (_isMatchmakingInProgress && _matchmakingService != null)
            {
                _matchmakingService.CancelMatchmaking();
            }

            _isMatchmakingInProgress = false;
        }

        public override void Update()
        {
            if (!_isMatchmakingInProgress || _matchmakingService == null)
                return;

            // Update search time
            _searchTime = Time.time - _searchStartTime;

            // Check for timeout - trigger bot filling
            if (_searchTime >= SearchTimeout && !_hasFilledWithBots)
            {
                LogMessage($"Matchmaking timeout after {_searchTime:F1}s - filling with bots");
                _hasFilledWithBots = true;

                // Tell service to fill with bots
                _matchmakingService.FillMatchWithBots();
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
            // MatchmakingView will update its UI via this event
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
            var lobbyManager = _sessionContext.LobbyManager;
            if (lobbyManager != null && lobbyManager.IsInMatchLobby)
            {
                LogMessage("Leaving match lobby due to matchmaking failure/cancellation");
                lobbyManager.LeaveMatchLobby();
            }
        }

        private void ReturnToMainMenu()
        {
            _stateManager?.ChangeState<MainMenuState>();
        }

        #endregion
    }
}
