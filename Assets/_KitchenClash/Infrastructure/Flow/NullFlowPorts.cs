using Playcenter.GameFlow;

namespace KitchenClash.Infrastructure.Flow
{
    /// <summary>
    /// No-op ports for phases not yet adapted. Keeps AppFlowController constructible.
    /// </summary>
    public sealed class NullSplashPort : ISplashPort
    {
        public void EnterSplash(FlowContext context) { }
        public void ExitSplash() { }
    }

    public sealed class NullBootPort : IBootPort
    {
        public void EnterBoot(FlowContext context) { }
        public void ExitBoot() { }
    }

    public sealed class NullMatchIntroPort : IMatchIntroPort
    {
        public void EnterMatchIntro(FlowContext context, MatchResolvedInfo info) { }
        public void ExitMatchIntro() { }
    }

    public sealed class NullCountdownPort : ICountdownPort
    {
        public void EnterCountdown(FlowContext context) { }
        public void ExitCountdown() { }
    }

    public sealed class NullFlowAnalyticsPort : IFlowAnalyticsPort
    {
        public void TrackPhaseChanged(FlowPhaseId from, FlowPhaseId to, FlowContext context) { }
        public void TrackPlayRequested(PlayRequest request, FlowContext context) { }
        public void TrackMatchResolved(MatchResolvedInfo info, FlowContext context) { }
        public void TrackMatchCompleted(MatchResultInfo result, FlowContext context) { }
    }
}
