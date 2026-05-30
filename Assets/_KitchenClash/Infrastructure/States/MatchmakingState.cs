using KitchenClash.Application;
using System;
using KitchenClash.Application.Services;
using KitchenClash.Infrastructure.DI;
using KitchenClash.Application.State;
using Cysharp.Threading.Tasks;
using UnityEngine;
using KitchenClash.Domain;

namespace KitchenClash.Infrastructure.States
{
    public class MatchmakingState : BaseState
    {
        private readonly IUIService _uiService;
        private readonly ISessionContext _sessionContext;
        private readonly IGameStateManager _stateManager;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IMatchmakingService _matchmakingService;
        private readonly IConfigService _configService;
        private readonly IEventBus _eventBus;

        private bool _isMatchmakingInProgress;
        private float _searchStartTime;
        private float _searchTime;
        private float _searchTimeout;
        private bool _hasFilledWithBots;

        private string _gameModeId = "quick_2v2";
        private int _teamSize = 2;

        public MatchmakingState(
            IUIService uiService,
            ISessionContext sessionContext,
            IGameStateManager stateManager,
            IMaintenanceService maintenanceService,
            IMatchmakingService matchmakingService,
            IConfigService configService,
            IEventBus eventBus)
        {
            _uiService = uiService;
            _sessionContext = sessionContext;
            _stateManager = stateManager;
            _maintenanceService = maintenanceService;
            _matchmakingService = matchmakingService;
            _configService = configService;
            _eventBus = eventBus;
        }

        /// <summary>
        /// Set the queue parameters before entering this state.
        /// </summary>
        public void SetQueueParameters(string gameModeId, int teamSize)
        {
            _gameModeId = gameModeId;
            _teamSize = teamSize;
        }

        public override void Enter()
        {
            base.Enter();

            _eventBus?.Publish(new MusicEvent(MusicTrack.Matchmaking));

            _searchTimeout = _configService != null
                ? _configService.Get("matchmaking_timeout_sec", 30f)
                : 30f;

            // Subscribe to matchmaking events for state transitions
            if (_matchmakingService != null)
            {
                _matchmakingService.OnMatchFound += OnMatchFound;
                _matchmakingService.OnMatchmakingCancelled += OnMatchmakingCancelled;
                _matchmakingService.OnMatchmakingFailed += OnMatchmakingFailed;
            }

            // Show the matchmaking screen
            var screenType = Type.GetType("KitchenClash.Presentation.Screens.MatchmakingScreen, KitchenClash.Presentation");
            if (screenType != null)
            {
                _uiService?.Show(screenType);
            }

            CheckMaintenanceAndStartAsync().Forget();
        }

        private async UniTask CheckMaintenanceAndStartAsync()
        {
            try
            {
                if (_maintenanceService != null)
                {
                    bool isInMaintenance = await _maintenanceService.CheckMaintenanceStatusAsync();
                    if (!IsStateActive)
                    {
                        return;
                    }

                    if (isInMaintenance)
                    {
                        LogMessage("Matchmaking blocked - server is in maintenance mode");
                        _stateManager?.ChangeState<MainMenuState>();
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
                _stateManager?.ChangeState<MainMenuState>();
            }
        }

        private void StartMatchmaking()
        {
            _isMatchmakingInProgress = true;
            _searchStartTime = Time.time;
            _searchTime = 0f;
            _hasFilledWithBots = false;

            LogMessage($"Starting matchmaking: {_gameModeId}, Team Size: {_teamSize}, Timeout: {_searchTimeout}s");
            _matchmakingService?.FindMatch(_gameModeId, _teamSize);
        }

        public override void Exit()
        {
            base.Exit();

            // Unsubscribe from events
            if (_matchmakingService != null)
            {
                _matchmakingService.OnMatchFound -= OnMatchFound;
                _matchmakingService.OnMatchmakingCancelled -= OnMatchmakingCancelled;
                _matchmakingService.OnMatchmakingFailed -= OnMatchmakingFailed;
            }

            if (_isMatchmakingInProgress && _matchmakingService != null)
            {
                _matchmakingService.CancelMatchmaking();
            }
            _isMatchmakingInProgress = false;
        }

        public override void Update()
        {
            if (!_isMatchmakingInProgress)
            {
                return;
            }

            _searchTime = Time.time - _searchStartTime;

            // On timeout, fill remaining slots with bots and proceed
            if (_searchTime >= _searchTimeout && !_hasFilledWithBots)
            {
                LogMessage($"Matchmaking timeout after {_searchTime:F1}s - filling with bots");
                _hasFilledWithBots = true;
                _matchmakingService?.FillMatchWithBots();
            }
        }

        public override void FixedUpdate() { }

        #region Event Handlers

        private void OnMatchFound(LobbyInfo lobbyInfo)
        {
            LogMessage($"Match found: {lobbyInfo?.LobbyId}");
            _isMatchmakingInProgress = false;
            _stateManager?.ChangeState<GameplayState>();
        }

        private void OnMatchmakingCancelled()
        {
            LogMessage("Matchmaking cancelled by user");
            _isMatchmakingInProgress = false;
            _stateManager?.ChangeState<MainMenuState>();
        }

        private void OnMatchmakingFailed(string reason)
        {
            LogError($"Matchmaking failed: {reason}");
            _isMatchmakingInProgress = false;
            _stateManager?.ChangeState<MainMenuState>();
        }

        #endregion
    }
}
