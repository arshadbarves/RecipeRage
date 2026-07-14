using System;
using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Flow;
using Playcenter.GameFlow;
using UnityEngine;
using VContainer.Unity;
using Cysharp.Threading.Tasks;

namespace KitchenClash.Infrastructure.Flow.Handlers
{
    /// <summary>
    /// Matchmaking queue: maintenance gate, FindMatch, timeout bot fill (ITickable).
    /// </summary>
    public sealed class MatchmakingPhase : ITickable
    {
        private const string MatchmakingScreenTypeName =
            "KitchenClash.Presentation.Screens.MatchmakingScreen, KitchenClash.Presentation";

        private readonly IUIService _uiService;
        private readonly ISessionContext _sessionContext;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IMatchmakingService _matchmakingService;
        private readonly IConfigService _configService;
        private readonly IEventBus _eventBus;
        private readonly IAppFlow _appFlow;

        private bool _active;
        private bool _isMatchmakingInProgress;
        private float _searchStartTime;
        private float _searchTime;
        private float _searchTimeout;
        private bool _hasFilledWithBots;
        private string _gameModeId = "quick_2v2";
        private int _teamSize = 2;

        /// <summary>
        /// Prefer injected service; fall back to session-scoped matchmaking when Root cannot resolve it.
        /// </summary>
        private IMatchmakingService Matchmaking =>
            _matchmakingService ?? _sessionContext?.MatchmakingService;

        public MatchmakingPhase(
            IUIService uiService,
            ISessionContext sessionContext,
            IMaintenanceService maintenanceService,
            IConfigService configService,
            IEventBus eventBus,
            IAppFlow appFlow,
            IMatchmakingService matchmakingService = null)
        {
            _uiService = uiService;
            _sessionContext = sessionContext;
            _maintenanceService = maintenanceService;
            _matchmakingService = matchmakingService;
            _configService = configService;
            _eventBus = eventBus;
            _appFlow = appFlow;
        }

        public void Enter(PlayRequest request, FlowContext context)
        {
            Exit();

            if (request != null)
            {
                if (!string.IsNullOrEmpty(request.ModeId))
                {
                    _gameModeId = request.ModeId;
                }
                else if (!string.IsNullOrEmpty(context?.LastModeId))
                {
                    _gameModeId = context.LastModeId;
                }

                _teamSize = request.TeamSize > 0
                    ? request.TeamSize
                    : (context?.LastTeamSize > 0 ? context.LastTeamSize : 2);
            }

            _active = true;
            _eventBus?.Publish(new MusicEvent(MusicTrack.Matchmaking));

            _searchTimeout = _configService != null
                ? _configService.Get("matchmaking_timeout_sec", 30f)
                : 30f;

            IMatchmakingService matchmaking = Matchmaking;
            if (matchmaking != null)
            {
                matchmaking.OnMatchFound += OnMatchFound;
                matchmaking.OnMatchmakingCancelled += OnMatchmakingCancelled;
                matchmaking.OnMatchmakingFailed += OnMatchmakingFailed;
            }

            Type screenType = Type.GetType(MatchmakingScreenTypeName);
            if (screenType != null)
            {
                _uiService?.Show(screenType);
            }

            CheckMaintenanceAndStartAsync().Forget();
        }

        public void Exit()
        {
            _active = false;

            IMatchmakingService matchmaking = Matchmaking;
            if (matchmaking != null)
            {
                matchmaking.OnMatchFound -= OnMatchFound;
                matchmaking.OnMatchmakingCancelled -= OnMatchmakingCancelled;
                matchmaking.OnMatchmakingFailed -= OnMatchmakingFailed;
            }

            if (_isMatchmakingInProgress && matchmaking != null)
            {
                matchmaking.CancelMatchmaking();
            }

            _isMatchmakingInProgress = false;
        }

        public void Tick()
        {
            if (!_active || !_isMatchmakingInProgress)
            {
                return;
            }

            _searchTime = Time.time - _searchStartTime;

            if (_searchTime >= _searchTimeout && !_hasFilledWithBots)
            {
                GameLogger.Log($"[MatchmakingPhase] Timeout after {_searchTime:F1}s - filling with bots");
                _hasFilledWithBots = true;
                Matchmaking?.FillMatchWithBots();
            }
        }

        private async UniTask CheckMaintenanceAndStartAsync()
        {
            try
            {
                if (_maintenanceService != null)
                {
                    bool isInMaintenance = await _maintenanceService.CheckMaintenanceStatusAsync();
                    if (!_active)
                    {
                        return;
                    }

                    if (isInMaintenance)
                    {
                        GameLogger.Log("[MatchmakingPhase] Matchmaking blocked - server is in maintenance mode");
                        _appFlow?.ReturnHome();
                        return;
                    }
                }

                StartMatchmaking();
            }
            catch (OperationCanceledException)
            {
                GameLogger.Log("[MatchmakingPhase] Matchmaking startup cancelled");
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"[MatchmakingPhase] Failed to start matchmaking: {ex.Message}");
                _appFlow?.ReturnHome();
            }
        }

        private void StartMatchmaking()
        {
            if (!_active)
            {
                return;
            }

            _isMatchmakingInProgress = true;
            _searchStartTime = Time.time;
            _searchTime = 0f;
            _hasFilledWithBots = false;

            GameLogger.Log($"[MatchmakingPhase] Starting: {_gameModeId}, Team Size: {_teamSize}, Timeout: {_searchTimeout}s");
            Matchmaking?.FindMatch(_gameModeId, _teamSize);
        }

        private void OnMatchFound(LobbyInfo lobbyInfo)
        {
            GameLogger.Log($"[MatchmakingPhase] Match found: {lobbyInfo?.LobbyId}");
            _isMatchmakingInProgress = false;
            _appFlow?.NotifyMatchResolved(
                FlowMatchInfoFactory.FromLobby(lobbyInfo, _hasFilledWithBots, _teamSize));
        }

        private void OnMatchmakingCancelled()
        {
            GameLogger.Log("[MatchmakingPhase] Cancelled by user");
            _isMatchmakingInProgress = false;
            _appFlow?.CancelMatchmaking();
        }

        private void OnMatchmakingFailed(string reason)
        {
            GameLogger.LogError($"[MatchmakingPhase] Failed: {reason}");
            _isMatchmakingInProgress = false;
            _appFlow?.ReturnHome();
        }
    }
}
