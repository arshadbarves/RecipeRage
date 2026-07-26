namespace Playcenter.GameFlow
{
    /// <summary>Studio splash presentation + minimum dwell.</summary>
    public interface ISplashPort
    {
        void EnterSplash(FlowContext context);
        void ExitSplash();
    }

    /// <summary>Silent auth, config, profile load, progress UI.</summary>
    public interface IBootPort
    {
        void EnterBoot(FlowContext context);
        void ExitBoot();
    }

    /// <summary>Home hub: currencies, mode chip, PLAY surface.</summary>
    public interface IHomePort
    {
        void EnterHome(FlowContext context);
        void ExitHome();
    }

    /// <summary>Queue UI + matchmaking service. Must call NotifyMatchResolved / cancel paths.</summary>
    public interface IMatchmakingPort
    {
        void EnterMatchmaking(FlowContext context, PlayRequest request);
        void ExitMatchmaking();
        void Cancel();
    }

    /// <summary>Found / map card / load progress before countdown.</summary>
    public interface IMatchIntroPort
    {
        void EnterMatchIntro(FlowContext context, MatchResolvedInfo info);
        void ExitMatchIntro();
    }

    /// <summary>3-2-1-GO with input lock.</summary>
    public interface ICountdownPort
    {
        void EnterCountdown(FlowContext context);
        void ExitCountdown();
    }

    /// <summary>Live match plugin (kitchen combat, HUD). StartRound only after countdown.</summary>
    public interface IMatchRuntimePort
    {
        void EnterMatch(FlowContext context);
        void StartRound(FlowContext context);
        void ExitMatch();
    }

    /// <summary>Results: outcome, rewards surface, Play Again / Home.</summary>
    public interface IResultsPort
    {
        void EnterResults(FlowContext context, MatchResultInfo result);
        void ExitResults();
    }

    /// <summary>Soft offer / popup policy (never block first PLAY).</summary>
    public interface IPopupPolicyPort
    {
        bool CanShowSoftPopup(FlowContext context);
        void OnHomeEntered(FlowContext context);
    }

    /// <summary>Optional analytics sink for phase transitions.</summary>
    public interface IFlowAnalyticsPort
    {
        void TrackPhaseChanged(FlowPhaseId from, FlowPhaseId to, FlowContext context);
        void TrackPlayRequested(PlayRequest request, FlowContext context);
        void TrackMatchResolved(MatchResolvedInfo info, FlowContext context);
        void TrackMatchCompleted(MatchResultInfo result, FlowContext context);
    }

    /// <summary>Side phases: Login, Maintenance, ForceUpdate, Tutorial, etc.</summary>
    public interface ISidePhasePort
    {
        void EnterSidePhase(FlowPhaseId phase, FlowContext context);
        void ExitSidePhase(FlowPhaseId phase);
    }
}
