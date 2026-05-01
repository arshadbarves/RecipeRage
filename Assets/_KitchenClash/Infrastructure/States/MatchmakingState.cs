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

        private bool _isMatchmakingInProgress;
        private float _searchStartTime;
        private float _searchTime;
        private const float SearchTimeout = 6f;
        private bool _hasFilledWithBots;

        private string _gameModeId = "classic";
        private int _teamSize = 2;

        public MatchmakingState(
            IUIService uiService,
            ISessionContext sessionContext,
            IGameStateManager stateManager,
            IMaintenanceService maintenanceService,
            IMatchmakingService matchmakingService)
        {
            _uiService = uiService;
            _sessionContext = sessionContext;
            _stateManager = stateManager;
            _maintenanceService = maintenanceService;
            _matchmakingService = matchmakingService;
        }

        public override void Enter()
        {
            base.Enter();
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

            LogMessage($"Starting matchmaking: {_gameModeId}, Team Size: {_teamSize}");
            _matchmakingService?.FindMatch(_gameModeId, _teamSize);
        }

        public override void Exit()
        {
            base.Exit();
            if (_isMatchmakingInProgress && _matchmakingService != null)
            {
                _matchmakingService.CancelMatchmaking();
            }
            _isMatchmakingInProgress = false;
        }

        public override void Update()
        {
            if (!_isMatchmakingInProgress) return;

            _searchTime = Time.time - _searchStartTime;

            if (_searchTime >= SearchTimeout && !_hasFilledWithBots)
            {
                LogMessage($"Matchmaking timeout after {_searchTime:F1}s - filling with bots");
                _hasFilledWithBots = true;
                _matchmakingService?.FillMatchWithBots();
            }
        }

        public override void FixedUpdate() { }
    }
}
