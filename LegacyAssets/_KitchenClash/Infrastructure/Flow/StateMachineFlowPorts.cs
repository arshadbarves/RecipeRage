using System;
using System.Collections.Generic;
using System.Linq;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Flow.Handlers;
using Playcenter.GameFlow;
using Playcenter.Shell;
using Playcenter.Services;
using Playcenter.UI;

namespace KitchenClash.Infrastructure.Flow
{
    /// <summary>
    /// Home hub adapter: loads MainMenu scene via HomePhase.
    /// </summary>
    public sealed class HomeFlowPort : IHomePort
    {
        private readonly HomePhase _home;

        public HomeFlowPort(HomePhase home)
        {
            _home = home;
        }

        public void EnterHome(FlowContext context)
        {
            _ = context;
            _home?.Enter();
        }

        public void ExitHome()
        {
            _home?.Exit();
        }
    }

    /// <summary>
    /// Matchmaking adapter: drives MatchmakingPhase; hides queue UI on exit.
    /// </summary>
    public sealed class MatchmakingFlowPort : IMatchmakingPort
    {
        private const string MatchmakingScreenTypeName =
            "KitchenClash.Presentation.Screens.MatchmakingScreen, KitchenClash.Presentation";

        private readonly MatchmakingPhase _matchmaking;
        private readonly IUIService _uiService;

        public MatchmakingFlowPort(MatchmakingPhase matchmaking, IUIService uiService = null)
        {
            _matchmaking = matchmaking;
            _uiService = uiService;
        }

        public void EnterMatchmaking(FlowContext context, PlayRequest request)
        {
            _matchmaking?.Enter(request, context);
        }

        public void ExitMatchmaking()
        {
            _matchmaking?.Exit();

            Type screenType = Type.GetType(MatchmakingScreenTypeName);
            if (screenType != null)
            {
                _uiService?.Hide(screenType);
            }
        }

        public void Cancel()
        {
            // ExitMatchmaking cancels in-flight search via MatchmakingPhase.Exit.
        }
    }

    /// <summary>
    /// Match runtime adapter: MatchRuntimePhase loads map; StartRound gated until load complete.
    /// EnterMatch is idempotent so intro can preload the map under the card.
    /// </summary>
    public sealed class MatchRuntimeFlowPort : IMatchRuntimePort
    {
        private readonly MatchRuntimePhase _matchRuntime;
        private bool _pendingStartRound;

        public MatchRuntimeFlowPort(MatchRuntimePhase matchRuntime)
        {
            _matchRuntime = matchRuntime;
        }

        public void EnterMatch(FlowContext context)
        {
            _ = context;
            _matchRuntime?.Enter();

            if (_pendingStartRound)
            {
                _pendingStartRound = false;
                _matchRuntime?.RequestStartRound();
            }
        }

        public void StartRound(FlowContext context)
        {
            _ = context;
            if (_matchRuntime != null && _matchRuntime.IsEntered)
            {
                _matchRuntime.RequestStartRound();
                return;
            }

            _pendingStartRound = true;
        }

        public void ExitMatch()
        {
            _pendingStartRound = false;
            _matchRuntime?.Exit();
        }
    }

    /// <summary>
    /// Results adapter: ResultsPhase (audio/reward) + ResultsScreen show/hide.
    /// </summary>
    public sealed class ResultsFlowPort : IResultsPort
    {
        private const string ResultsScreenTypeName =
            "KitchenClash.Presentation.Screens.ResultsScreen, KitchenClash.Presentation";

        private readonly ResultsPhase _results;
        private readonly IUIService _uiService;

        public ResultsFlowPort(ResultsPhase results, IUIService uiService)
        {
            _results = results;
            _uiService = uiService;
        }

        public void EnterResults(FlowContext context, MatchResultInfo result)
        {
            MatchResultInfo resolved = result ?? context?.LastMatchResult;
            _results?.Enter(resolved);

            Type resultsType = Type.GetType(ResultsScreenTypeName);
            if (resultsType != null)
            {
                _uiService?.Show(resultsType);
            }
        }

        public void ExitResults()
        {
            _results?.Exit();

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
            GameLogger.Log($"[AppFlow] {from} → {to}");
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
    /// Helpers shared by handlers/view-models when building flow DTOs.
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
