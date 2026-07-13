using System;
using System.Collections.Generic;

namespace Playcenter.GameFlow
{
    /// <summary>
    /// Production product FSM. Owns legal transitions; delegates work to ports.
    /// Zero game-specific types — adapters live in the game assembly.
    /// </summary>
    public sealed class AppFlowController : IAppFlow
    {
        private readonly FlowContext _context = new FlowContext();
        private readonly ISplashPort _splash;
        private readonly IBootPort _boot;
        private readonly IHomePort _home;
        private readonly IMatchmakingPort _matchmaking;
        private readonly IMatchIntroPort _matchIntro;
        private readonly ICountdownPort _countdown;
        private readonly IMatchRuntimePort _matchRuntime;
        private readonly IResultsPort _results;
        private readonly IPopupPolicyPort _popupPolicy;
        private readonly IFlowAnalyticsPort _analytics;

        private FlowPhaseId _current = FlowPhaseId.None;
        private FlowPhaseId _sideReturnPhase = FlowPhaseId.Home;
        private PlayRequest _pendingPlayRequest;

        private static readonly HashSet<(FlowPhaseId From, FlowPhaseId To)> Legal =
            new HashSet<(FlowPhaseId, FlowPhaseId)>
            {
                (FlowPhaseId.None, FlowPhaseId.StudioSplash),
                (FlowPhaseId.StudioSplash, FlowPhaseId.Boot),
                (FlowPhaseId.Boot, FlowPhaseId.Home),
                (FlowPhaseId.Boot, FlowPhaseId.ForceUpdate),
                (FlowPhaseId.Boot, FlowPhaseId.Maintenance),
                (FlowPhaseId.Boot, FlowPhaseId.NoConnection),
                (FlowPhaseId.Boot, FlowPhaseId.Login),
                (FlowPhaseId.Boot, FlowPhaseId.Tutorial),
                (FlowPhaseId.Boot, FlowPhaseId.AccountUpgrade),
                (FlowPhaseId.Home, FlowPhaseId.Matchmaking),
                (FlowPhaseId.Matchmaking, FlowPhaseId.MatchIntro),
                (FlowPhaseId.Matchmaking, FlowPhaseId.Home),
                // Migration path: some games still jump MM → Match until Intro ships
                (FlowPhaseId.Matchmaking, FlowPhaseId.Match),
                (FlowPhaseId.MatchIntro, FlowPhaseId.Countdown),
                (FlowPhaseId.MatchIntro, FlowPhaseId.Home),
                (FlowPhaseId.Countdown, FlowPhaseId.Match),
                (FlowPhaseId.Countdown, FlowPhaseId.Home),
                (FlowPhaseId.Match, FlowPhaseId.Results),
                (FlowPhaseId.Match, FlowPhaseId.Home),
                (FlowPhaseId.Results, FlowPhaseId.Matchmaking),
                (FlowPhaseId.Results, FlowPhaseId.Home),
                // Side phases → Home
                (FlowPhaseId.ForceUpdate, FlowPhaseId.Home),
                (FlowPhaseId.Maintenance, FlowPhaseId.Home),
                (FlowPhaseId.NoConnection, FlowPhaseId.Home),
                (FlowPhaseId.Login, FlowPhaseId.Home),
                (FlowPhaseId.Tutorial, FlowPhaseId.Home),
                (FlowPhaseId.AccountUpgrade, FlowPhaseId.Home),
                // Fail-closed from anywhere to Home is handled explicitly in ReturnHome
            };

        public AppFlowController(
            ISplashPort splash = null,
            IBootPort boot = null,
            IHomePort home = null,
            IMatchmakingPort matchmaking = null,
            IMatchIntroPort matchIntro = null,
            ICountdownPort countdown = null,
            IMatchRuntimePort matchRuntime = null,
            IResultsPort results = null,
            IPopupPolicyPort popupPolicy = null,
            IFlowAnalyticsPort analytics = null)
        {
            _splash = splash;
            _boot = boot;
            _home = home;
            _matchmaking = matchmaking;
            _matchIntro = matchIntro;
            _countdown = countdown;
            _matchRuntime = matchRuntime;
            _results = results;
            _popupPolicy = popupPolicy ?? new SoftPopupPolicy();
            _analytics = analytics;
        }

        public FlowPhaseId Current => _current;
        public FlowContext Context => _context;

        public event Action<FlowPhaseId, FlowPhaseId> PhaseChanged;

        public void StartColdBoot()
        {
            TransitionTo(FlowPhaseId.StudioSplash);
        }

        public void RequestPlay(PlayRequest request = null)
        {
            if (_current != FlowPhaseId.Home && _current != FlowPhaseId.Results)
            {
                // Only Home (and Play Again from Results via RequestPlayAgain) start queue.
                if (_current != FlowPhaseId.Home)
                {
                    return;
                }
            }

            _pendingPlayRequest = RememberedQueuePolicy.Resolve(request, _context);
            _analytics?.TrackPlayRequested(_pendingPlayRequest, _context);
            TransitionTo(FlowPhaseId.Matchmaking);
        }

        public void CancelMatchmaking()
        {
            if (_current != FlowPhaseId.Matchmaking)
            {
                return;
            }

            _matchmaking?.Cancel();
            TransitionTo(FlowPhaseId.Home);
        }

        public void NotifyMatchResolved(MatchResolvedInfo info)
        {
            if (_current != FlowPhaseId.Matchmaking)
            {
                return;
            }

            _context.LastMatchResolved = info;
            if (info != null && !string.IsNullOrEmpty(info.ModeId))
            {
                _context.LastModeId = info.ModeId;
            }

            if (info != null && info.TeamSize > 0)
            {
                _context.LastTeamSize = info.TeamSize;
            }

            _analytics?.TrackMatchResolved(info, _context);

            // Prefer intro when port is wired; otherwise go straight to Match (migration).
            if (_matchIntro != null)
            {
                TransitionTo(FlowPhaseId.MatchIntro);
            }
            else
            {
                TransitionTo(FlowPhaseId.Match);
            }
        }

        public void NotifyMatchIntroReady()
        {
            if (_current != FlowPhaseId.MatchIntro)
            {
                return;
            }

            if (_countdown != null)
            {
                TransitionTo(FlowPhaseId.Countdown);
            }
            else
            {
                TransitionTo(FlowPhaseId.Match);
            }
        }

        public void NotifyCountdownComplete()
        {
            if (_current != FlowPhaseId.Countdown)
            {
                return;
            }

            TransitionTo(FlowPhaseId.Match);
            _matchRuntime?.StartRound(_context);
        }

        public void NotifyMatchCompleted(MatchResultInfo result)
        {
            if (_current != FlowPhaseId.Match && _current != FlowPhaseId.Results)
            {
                // Allow late notify only from Match.
                if (_current != FlowPhaseId.Match)
                {
                    return;
                }
            }

            _context.LastMatchResult = result;
            _context.HasCompletedFirstPlay = true;
            _analytics?.TrackMatchCompleted(result, _context);
            TransitionTo(FlowPhaseId.Results);
        }

        public void NotifySplashComplete()
        {
            if (_current != FlowPhaseId.StudioSplash)
            {
                return;
            }

            TransitionTo(FlowPhaseId.Boot);
        }

        public void NotifyBootComplete()
        {
            if (_current != FlowPhaseId.Boot)
            {
                return;
            }

            TransitionTo(FlowPhaseId.Home);
        }

        public void RequestPlayAgain()
        {
            if (_current != FlowPhaseId.Results)
            {
                return;
            }

            _pendingPlayRequest = _context.BuildRememberedPlayRequest();
            _analytics?.TrackPlayRequested(_pendingPlayRequest, _context);
            TransitionTo(FlowPhaseId.Matchmaking);
        }

        public void ReturnHome()
        {
            if (_current == FlowPhaseId.Home)
            {
                return;
            }

            // Fail-closed: always legal to go Home from product perspective.
            ForceTransitionTo(FlowPhaseId.Home);
        }

        public void EnterSidePhase(FlowPhaseId sidePhase)
        {
            if (!IsSidePhase(sidePhase))
            {
                return;
            }

            _sideReturnPhase = _current == FlowPhaseId.None ? FlowPhaseId.Home : _current;
            ForceTransitionTo(sidePhase);
        }

        public void CompleteSidePhase()
        {
            if (!IsSidePhase(_current))
            {
                return;
            }

            var target = _sideReturnPhase == FlowPhaseId.None ? FlowPhaseId.Home : _sideReturnPhase;
            if (target == FlowPhaseId.Boot || target == FlowPhaseId.StudioSplash)
            {
                target = FlowPhaseId.Home;
            }

            ForceTransitionTo(target);
        }

        public bool CanShowSoftPopup()
        {
            return _popupPolicy != null && _popupPolicy.CanShowSoftPopup(_context);
        }

        private void TransitionTo(FlowPhaseId next)
        {
            if (_current == next)
            {
                return;
            }

            if (_current != FlowPhaseId.None && !Legal.Contains((_current, next)))
            {
                // Illegal product jump → fail-closed Home
                if (next != FlowPhaseId.Home)
                {
                    ForceTransitionTo(FlowPhaseId.Home);
                    return;
                }
            }

            ForceTransitionTo(next);
        }

        private void ForceTransitionTo(FlowPhaseId next)
        {
            var previous = _current;
            ExitPhase(previous);
            _current = next;
            EnterPhase(next);
            PhaseChanged?.Invoke(previous, next);
            _analytics?.TrackPhaseChanged(previous, next, _context);
        }

        private void EnterPhase(FlowPhaseId phase)
        {
            switch (phase)
            {
                case FlowPhaseId.StudioSplash:
                    _splash?.EnterSplash(_context);
                    // Auto-advance when no splash port (tests); real games call into Boot from adapter.
                    if (_splash == null)
                    {
                        TransitionTo(FlowPhaseId.Boot);
                    }
                    break;
                case FlowPhaseId.Boot:
                    _boot?.EnterBoot(_context);
                    if (_boot == null)
                    {
                        TransitionTo(FlowPhaseId.Home);
                    }
                    break;
                case FlowPhaseId.Home:
                    _home?.EnterHome(_context);
                    _popupPolicy?.OnHomeEntered(_context);
                    break;
                case FlowPhaseId.Matchmaking:
                    _matchmaking?.EnterMatchmaking(_context, _pendingPlayRequest ?? _context.BuildRememberedPlayRequest());
                    break;
                case FlowPhaseId.MatchIntro:
                    _matchIntro?.EnterMatchIntro(_context, _context.LastMatchResolved);
                    // Brawl-style: preload map under the intro card so GO is instant.
                    _matchRuntime?.EnterMatch(_context);
                    break;
                case FlowPhaseId.Countdown:
                    _countdown?.EnterCountdown(_context);
                    // Keep map loading if intro was skipped somehow.
                    _matchRuntime?.EnterMatch(_context);
                    break;
                case FlowPhaseId.Match:
                    _matchRuntime?.EnterMatch(_context);
                    // Migration: no countdown port → start round immediately.
                    // With countdown, StartRound is invoked from NotifyCountdownComplete.
                    if (_countdown == null)
                    {
                        _matchRuntime?.StartRound(_context);
                    }
                    break;
                case FlowPhaseId.Results:
                    _results?.EnterResults(_context, _context.LastMatchResult);
                    break;
            }
        }

        private void ExitPhase(FlowPhaseId phase)
        {
            switch (phase)
            {
                case FlowPhaseId.StudioSplash:
                    _splash?.ExitSplash();
                    break;
                case FlowPhaseId.Boot:
                    _boot?.ExitBoot();
                    break;
                case FlowPhaseId.Home:
                    _home?.ExitHome();
                    break;
                case FlowPhaseId.Matchmaking:
                    _matchmaking?.ExitMatchmaking();
                    break;
                case FlowPhaseId.MatchIntro:
                    _matchIntro?.ExitMatchIntro();
                    break;
                case FlowPhaseId.Countdown:
                    _countdown?.ExitCountdown();
                    break;
                case FlowPhaseId.Match:
                    _matchRuntime?.ExitMatch();
                    break;
                case FlowPhaseId.Results:
                    _results?.ExitResults();
                    break;
            }
        }

        private static bool IsSidePhase(FlowPhaseId phase)
        {
            return phase == FlowPhaseId.ForceUpdate
                || phase == FlowPhaseId.Maintenance
                || phase == FlowPhaseId.NoConnection
                || phase == FlowPhaseId.Login
                || phase == FlowPhaseId.Tutorial
                || phase == FlowPhaseId.AccountUpgrade;
        }
    }
}
