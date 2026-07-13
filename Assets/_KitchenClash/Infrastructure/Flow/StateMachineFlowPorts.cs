using System;
using System.Collections.Generic;
using System.Linq;
using KitchenClash.Application.Services;
using KitchenClash.Application.State;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.States;
using Playcenter.GameFlow;

namespace KitchenClash.Infrastructure.Flow
{
    /// <summary>
    /// Home hub adapter: loads menu scene via MainMenuState.
    /// HomeScreen is shown by MainMenuState after the scene is ready (assembly-safe Type.GetType).
    /// </summary>
    public sealed class HomeFlowPort : IHomePort
    {
        private readonly IGameStateManager _stateManager;

        public HomeFlowPort(IGameStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public void EnterHome(FlowContext context)
        {
            if (_stateManager?.CurrentState is not MainMenuState)
            {
                _stateManager?.ChangeState<MainMenuState>();
            }
        }

        public void ExitHome()
        {
            // MainMenuState / next phase owns screen hide when leaving the hub.
        }
    }

    /// <summary>
    /// Matchmaking adapter: drives existing MatchmakingState.
    /// Match found / cancel report back through IAppFlow from the state.
    /// </summary>
    public sealed class MatchmakingFlowPort : IMatchmakingPort
    {
        private const string MatchmakingScreenTypeName =
            "KitchenClash.Presentation.Screens.MatchmakingScreen, KitchenClash.Presentation";

        private readonly IGameStateManager _stateManager;
        private readonly IStateFactory _stateFactory;
        private readonly IUIService _uiService;

        public MatchmakingFlowPort(
            IGameStateManager stateManager,
            IStateFactory stateFactory,
            IUIService uiService = null)
        {
            _stateManager = stateManager;
            _stateFactory = stateFactory;
            _uiService = uiService;
        }

        public void EnterMatchmaking(FlowContext context, PlayRequest request)
        {
            var state = _stateFactory?.Create<MatchmakingState>();
            if (state != null && request != null)
            {
                if (!string.IsNullOrEmpty(request.ModeId) || request.TeamSize > 0)
                {
                    int teamSize = request.TeamSize > 0
                        ? request.TeamSize
                        : (context?.LastTeamSize > 0 ? context.LastTeamSize : 2);
                    string modeId = !string.IsNullOrEmpty(request.ModeId)
                        ? request.ModeId
                        : context?.LastModeId;
                    if (!string.IsNullOrEmpty(modeId))
                    {
                        state.SetQueueParameters(modeId, teamSize);
                    }
                }

                _stateManager?.ChangeState(state);
                return;
            }

            _stateManager?.ChangeState<MatchmakingState>();
        }

        public void ExitMatchmaking()
        {
            // Hide immediately on flow exit so intro/home never sit under the queue UI.
            // MatchmakingState.Exit also hides (idempotent).
            Type screenType = Type.GetType(MatchmakingScreenTypeName);
            if (screenType != null)
            {
                _uiService?.Hide(screenType);
            }
        }

        public void Cancel()
        {
            // MatchmakingState.Exit cancels in-flight search when we leave the state.
        }
    }

    /// <summary>
    /// Match runtime adapter: GameplayState loads map on Enter; StartRound after GO.
    /// EnterMatch is idempotent so intro can preload the map under the card.
    /// </summary>
    public sealed class MatchRuntimeFlowPort : IMatchRuntimePort
    {
        private readonly IGameStateManager _stateManager;
        private readonly IStateFactory _stateFactory;
        private bool _pendingStartRound;

        public MatchRuntimeFlowPort(IGameStateManager stateManager, IStateFactory stateFactory = null)
        {
            _stateManager = stateManager;
            _stateFactory = stateFactory;
        }

        public void EnterMatch(FlowContext context)
        {
            _ = context;
            if (_stateManager?.CurrentState is GameplayState existing)
            {
                if (_pendingStartRound)
                {
                    _pendingStartRound = false;
                    existing.RequestStartRound();
                }

                return;
            }

            // Prefer factory so we can request start on the same instance if needed.
            var state = _stateFactory?.Create<GameplayState>();
            if (state != null)
            {
                _stateManager?.ChangeState(state);
                if (_pendingStartRound)
                {
                    _pendingStartRound = false;
                    state.RequestStartRound();
                }

                return;
            }

            _stateManager?.ChangeState<GameplayState>();
            if (_pendingStartRound && _stateManager?.CurrentState is GameplayState created)
            {
                _pendingStartRound = false;
                created.RequestStartRound();
            }
        }

        public void StartRound(FlowContext context)
        {
            _ = context;
            if (_stateManager?.CurrentState is GameplayState gameplay)
            {
                gameplay.RequestStartRound();
                return;
            }

            // Map not entered yet (or still transitioning) — queue until EnterMatch.
            _pendingStartRound = true;
        }

        public void ExitMatch()
        {
            _pendingStartRound = false;
            // Teardown owned by GameplayState.Exit / match scope dispose.
        }
    }

    /// <summary>
    /// Results adapter: enters GameOverState worker (music/SFX via MatchEndedEvent).
    /// Screen show uses Type.GetType — Infrastructure must not reference Presentation types.
    /// </summary>
    public sealed class ResultsFlowPort : IResultsPort
    {
        private const string ResultsScreenTypeName =
            "KitchenClash.Presentation.Screens.ResultsScreen, KitchenClash.Presentation";

        private readonly IGameStateManager _stateManager;
        private readonly IUIService _uiService;

        public ResultsFlowPort(IGameStateManager stateManager, IUIService uiService)
        {
            _stateManager = stateManager;
            _uiService = uiService;
        }

        public void EnterResults(FlowContext context, MatchResultInfo result)
        {
            if (_stateManager?.CurrentState is not GameOverState)
            {
                _stateManager?.ChangeState<GameOverState>();
            }

            Type resultsType = Type.GetType(ResultsScreenTypeName);
            if (resultsType != null)
            {
                _uiService?.Show(resultsType);
            }
        }

        public void ExitResults()
        {
            Type resultsType = Type.GetType(ResultsScreenTypeName);
            if (resultsType != null)
            {
                _uiService?.Hide(resultsType);
            }
        }
    }

    /// <summary>
    /// Optional analytics bridge to IAnalyticsService.
    /// </summary>
    public sealed class AnalyticsFlowPort : IFlowAnalyticsPort
    {
        private readonly IAnalyticsService _analytics;

        public AnalyticsFlowPort(IAnalyticsService analytics)
        {
            _analytics = analytics;
        }

        public void TrackPhaseChanged(FlowPhaseId from, FlowPhaseId to, FlowContext context)
        {
            _analytics?.LogEvent("flow_phase_changed", new Dictionary<string, object>
            {
                { "from", from.ToString() },
                { "to", to.ToString() }
            });
        }

        public void TrackPlayRequested(PlayRequest request, FlowContext context)
        {
            _analytics?.LogEvent("flow_play_requested", new Dictionary<string, object>
            {
                { "mode_id", request?.ModeId ?? string.Empty },
                { "team_size", request?.TeamSize ?? 0 }
            });
        }

        public void TrackMatchResolved(MatchResolvedInfo info, FlowContext context)
        {
            _analytics?.LogEvent("flow_match_resolved", new Dictionary<string, object>
            {
                { "mode_id", info?.ModeId ?? string.Empty },
                { "bots", info?.FilledWithBots ?? false }
            });
        }

        public void TrackMatchCompleted(MatchResultInfo result, FlowContext context)
        {
            _analytics?.LogEvent("flow_match_completed", new Dictionary<string, object>
            {
                { "won", result?.Won ?? false },
                { "draw", result?.IsDraw ?? false }
            });
        }
    }

    /// <summary>
    /// Helpers shared by states/view-models when building flow DTOs.
    /// </summary>
    public static class FlowMatchInfoFactory
    {
        public static MatchResolvedInfo FromLobby(LobbyInfo lobby, bool filledWithBots, int teamSize)
        {
            int botCount = 0;
            int humanCount = 0;
            if (lobby?.Players != null)
            {
                botCount = lobby.Players.Count(p => p != null && p.IsBot);
                humanCount = lobby.Players.Count(p => p != null && !p.IsBot);
            }

            return new MatchResolvedInfo
            {
                LobbyId = lobby?.LobbyId,
                ModeId = lobby?.GameModeId,
                MapId = lobby?.MapName,
                TeamSize = teamSize > 0 ? teamSize : 2,
                HumanCount = humanCount,
                BotCount = botCount,
                FilledWithBots = filledWithBots || botCount > 0
            };
        }
    }
}
