using System;

namespace Playcenter.GameFlow
{
    /// <summary>
    /// Public product navigation API. UI and features request intents here;
    /// they must not drive game-specific state machines directly.
    /// </summary>
    public interface IAppFlow
    {
        FlowPhaseId Current { get; }
        FlowContext Context { get; }

        /// <summary>Home PLAY (or equivalent). Always-resolve matchmaking policy applies.</summary>
        void RequestPlay(PlayRequest request = null);

        void CancelMatchmaking();

        /// <summary>Matchmaking port reports a resolved lobby (humans and/or bots).</summary>
        void NotifyMatchResolved(MatchResolvedInfo info);

        /// <summary>Match intro finished loading map / scene.</summary>
        void NotifyMatchIntroReady();

        /// <summary>Countdown finished (GO).</summary>
        void NotifyCountdownComplete();

        /// <summary>Match runtime reports end of match.</summary>
        void NotifyMatchCompleted(MatchResultInfo result);

        /// <summary>Results → re-queue same mode.</summary>
        void RequestPlayAgain();

        /// <summary>Any phase → Home (fail-closed safe). Also used by PlaycenterClient game-entry handoff.</summary>
        void ReturnHome();

        /// <summary>Optional gate: force update / maintenance / login / tutorial.</summary>
        void EnterSidePhase(FlowPhaseId sidePhase);

        /// <summary>Side phase finished; resume toward Home (or continue boot).</summary>
        void CompleteSidePhase();

        /// <summary>
        /// Soft offers (rate, IAP, ads) must never block first PLAY.
        /// UI/features query this before showing non-critical popups on Home.
        /// </summary>
        bool CanShowSoftPopup();

        event Action<FlowPhaseId, FlowPhaseId> PhaseChanged;
    }
}

