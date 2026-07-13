using System;
using Playcenter.GameFlow;

namespace KitchenClash.Infrastructure.Flow
{
    /// <summary>
    /// Lazy IAppFlow forwarder so ports can be constructed while AppFlowController is still being built.
    /// </summary>
    public sealed class AppFlowProxy : IAppFlow
    {
        private readonly Func<IAppFlow> _resolve;

        public AppFlowProxy(Func<IAppFlow> resolve)
        {
            _resolve = resolve ?? throw new ArgumentNullException(nameof(resolve));
        }

        private IAppFlow Target => _resolve()
            ?? throw new InvalidOperationException("IAppFlow is not ready yet.");

        public FlowPhaseId Current => Target.Current;
        public FlowContext Context => Target.Context;

        public event Action<FlowPhaseId, FlowPhaseId> PhaseChanged
        {
            add => Target.PhaseChanged += value;
            remove => Target.PhaseChanged -= value;
        }

        public void StartColdBoot() => Target.StartColdBoot();
        public void RequestPlay(PlayRequest request = null) => Target.RequestPlay(request);
        public void CancelMatchmaking() => Target.CancelMatchmaking();
        public void NotifyMatchResolved(MatchResolvedInfo info) => Target.NotifyMatchResolved(info);
        public void NotifyMatchIntroReady() => Target.NotifyMatchIntroReady();
        public void NotifyCountdownComplete() => Target.NotifyCountdownComplete();
        public void NotifyMatchCompleted(MatchResultInfo result) => Target.NotifyMatchCompleted(result);
        public void RequestPlayAgain() => Target.RequestPlayAgain();
        public void ReturnHome() => Target.ReturnHome();
        public void EnterSidePhase(FlowPhaseId sidePhase) => Target.EnterSidePhase(sidePhase);
        public void CompleteSidePhase() => Target.CompleteSidePhase();
        public bool CanShowSoftPopup() => Target.CanShowSoftPopup();
    }
}
