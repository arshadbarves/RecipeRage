using Playcenter.GameFlow;

namespace RecipeRage.Tests.EditMode.Gameplay.Fakes
{
    /// <summary>
    /// Shared test double for IAppFlow. Records call counts and last arguments for all state-changing methods.
    /// </summary>
    public sealed class FakeAppFlow : IAppFlow
    {
        public int RequestPlayCount { get; private set; }
        public PlayRequest LastPlayRequest { get; private set; }

        public int ReturnHomeCount { get; private set; }

        public int NotifyMatchCompletedCount { get; private set; }
        public MatchResultInfo LastMatchResult { get; private set; }

        public int NotifyMatchResolvedCount { get; private set; }
        public MatchResolvedInfo LastMatchResolved { get; private set; }

        public int CancelMatchmakingCount { get; private set; }

        public int NotifyMatchIntroReadyCount { get; private set; }
        public int NotifyCountdownCompleteCount { get; private set; }
        public int NotifySplashCompleteCount { get; private set; }
        public int NotifyBootCompleteCount { get; private set; }
        public int RequestPlayAgainCount { get; private set; }

        public int EnterSidePhaseCount { get; private set; }
        public FlowPhaseId LastSidePhase { get; private set; }

        public int CompleteSidePhaseCount { get; private set; }

        public FlowPhaseId Current { get; set; } = FlowPhaseId.Home;
        public FlowContext Context => null;

        public void RequestPlay(PlayRequest request = null)
        {
            RequestPlayCount++;
            LastPlayRequest = request;
        }

        public void ReturnHome()
        {
            ReturnHomeCount++;
        }

        public void NotifyMatchCompleted(MatchResultInfo result)
        {
            NotifyMatchCompletedCount++;
            LastMatchResult = result;
        }

        public void NotifyMatchResolved(MatchResolvedInfo info)
        {
            NotifyMatchResolvedCount++;
            LastMatchResolved = info;
        }

        public void CancelMatchmaking()
        {
            CancelMatchmakingCount++;
        }

        public void NotifyMatchIntroReady()
        {
            NotifyMatchIntroReadyCount++;
        }

        public void NotifyCountdownComplete()
        {
            NotifyCountdownCompleteCount++;
        }

        public void NotifySplashComplete()
        {
            NotifySplashCompleteCount++;
        }

        public void NotifyBootComplete()
        {
            NotifyBootCompleteCount++;
        }

        public void RequestPlayAgain()
        {
            RequestPlayAgainCount++;
        }

        public void EnterSidePhase(FlowPhaseId sidePhase)
        {
            EnterSidePhaseCount++;
            LastSidePhase = sidePhase;
        }

        public void CompleteSidePhase()
        {
            CompleteSidePhaseCount++;
        }

        public void StartColdBoot() { }

        public bool CanShowSoftPopup() => false;

        public event System.Action<FlowPhaseId, FlowPhaseId> PhaseChanged
        {
            add { }
            remove { }
        }

        public void Update(float deltaTime) { }
        public void FixedUpdate(float fixedDeltaTime) { }
    }
}
