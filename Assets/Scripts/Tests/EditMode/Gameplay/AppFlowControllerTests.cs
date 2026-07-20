using NUnit.Framework;
using Playcenter.GameFlow;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    public class AppFlowControllerTests
    {
        private sealed class RecordingHome : IHomePort
        {
            public int EnterCount;
            public void EnterHome(FlowContext context) => EnterCount++;
            public void ExitHome() { }
        }

        private sealed class RecordingMatchmaking : IMatchmakingPort
        {
            public int EnterCount;
            public PlayRequest LastRequest;
            public void EnterMatchmaking(FlowContext context, PlayRequest request)
            {
                EnterCount++;
                LastRequest = request;
            }
            public void ExitMatchmaking() { }
            public void Cancel() { }
        }

        private sealed class RecordingIntro : IMatchIntroPort
        {
            public int EnterCount;
            public void EnterMatchIntro(FlowContext context, MatchResolvedInfo info) => EnterCount++;
            public void ExitMatchIntro() { }
        }

        private sealed class RecordingCountdown : ICountdownPort
        {
            public int EnterCount;
            public void EnterCountdown(FlowContext context) => EnterCount++;
            public void ExitCountdown() { }
        }

        private sealed class RecordingMatch : IMatchRuntimePort
        {
            public int EnterCount;
            public int StartRoundCount;
            public void EnterMatch(FlowContext context) => EnterCount++;
            public void StartRound(FlowContext context) => StartRoundCount++;
            public void ExitMatch() { }
        }

        private sealed class RecordingResults : IResultsPort
        {
            public int EnterCount;
            public MatchResultInfo Last;
            public void EnterResults(FlowContext context, MatchResultInfo result)
            {
                EnterCount++;
                Last = result;
            }
            public void ExitResults() { }
        }


        [Test]
        public void RequestPlay_FromHome_EntersMatchmaking()
        {
            var home = new RecordingHome();
            var mm = new RecordingMatchmaking();
            var flow = new AppFlowController(home: home, matchmaking: mm);
            // SDK owns cold boot now — game entry calls ReturnHome() after OnPlaycenterReadyAsync.
            // From FlowPhaseId.None, ReturnHome() uses ForceTransitionTo which bypasses legal checks.
            flow.ReturnHome();
            Assert.AreEqual(FlowPhaseId.Home, flow.Current);
            Assert.AreEqual(1, home.EnterCount);

            flow.RequestPlay(new PlayRequest { ModeId = "quick_2v2", TeamSize = 2 });
            Assert.AreEqual(FlowPhaseId.Matchmaking, flow.Current);
            Assert.AreEqual(1, mm.EnterCount);
            Assert.AreEqual("quick_2v2", mm.LastRequest.ModeId);
        }

        [Test]
        public void FullHappyPath_IntroCountdown_StartRound_Results()
        {
            var home = new RecordingHome();
            var mm = new RecordingMatchmaking();
            var intro = new RecordingIntro();
            var countdown = new RecordingCountdown();
            var match = new RecordingMatch();
            var results = new RecordingResults();
            var flow = new AppFlowController(
                home: home,
                matchmaking: mm,
                matchIntro: intro,
                countdown: countdown,
                matchRuntime: match,
                results: results);

            flow.ReturnHome();
            flow.RequestPlay(new PlayRequest { ModeId = "quick_2v2", TeamSize = 2 });
            flow.NotifyMatchResolved(new MatchResolvedInfo
            {
                LobbyId = "L1",
                ModeId = "quick_2v2",
                TeamSize = 2,
                HumanCount = 1,
                BotCount = 3,
                FilledWithBots = true
            });
            Assert.AreEqual(FlowPhaseId.MatchIntro, flow.Current);
            Assert.AreEqual(1, intro.EnterCount);
            // EnterMatch called during intro (preload); StartRound must NOT be called yet
            Assert.GreaterOrEqual(match.EnterCount, 1, "Match should preload during intro");
            Assert.AreEqual(0, match.StartRoundCount, "StartRound must not fire during intro");

            flow.NotifyMatchIntroReady();
            Assert.AreEqual(FlowPhaseId.Countdown, flow.Current);
            Assert.AreEqual(1, countdown.EnterCount);
            // StartRound still deferred until countdown completes
            Assert.AreEqual(0, match.StartRoundCount, "StartRound must not fire during countdown");

            flow.NotifyCountdownComplete();
            Assert.AreEqual(FlowPhaseId.Match, flow.Current);
            // EnterMatch idempotent across intro/countdown/match phases, so >=1 is valid
            Assert.GreaterOrEqual(match.EnterCount, 1);
            // StartRound only fires AFTER countdown GO
            Assert.AreEqual(1, match.StartRoundCount, "StartRound must fire exactly once after GO");

            flow.NotifyMatchCompleted(new MatchResultInfo { Won = true, LocalTeamId = 0 });
            Assert.AreEqual(FlowPhaseId.Results, flow.Current);
            Assert.AreEqual(1, results.EnterCount);
            Assert.IsTrue(results.Last.Won);
        }

        [Test]
        public void RequestPlay_NotFromHome_IsIgnored()
        {
            var home = new RecordingHome();
            var mm = new RecordingMatchmaking();
            var flow = new AppFlowController(home: home, matchmaking: mm);
            flow.ReturnHome(); // SDK entry calls ReturnHome() after ready; current is now Home.
            Assert.AreEqual(FlowPhaseId.Home, flow.Current);

            flow.RequestPlay(new PlayRequest { ModeId = "quick_2v2", TeamSize = 2 });
            Assert.AreEqual(FlowPhaseId.Matchmaking, flow.Current);
            Assert.AreEqual(1, mm.EnterCount);

            // Second RequestPlay from Matchmaking should be ignored
            flow.RequestPlay(new PlayRequest { ModeId = "quick_3v3", TeamSize = 3 });
            Assert.AreEqual(1, mm.EnterCount); // Still 1, second call ignored
            Assert.AreEqual(FlowPhaseId.Matchmaking, flow.Current);
        }

        [Test]
        public void MigrationPath_NullCountdown_StartsRoundOnMatchEnter()
        {
            var home = new RecordingHome();
            var mm = new RecordingMatchmaking();
            var intro = new RecordingIntro();
            var match = new RecordingMatch();
            // No countdown port — migration path
            var flow = new AppFlowController(
                home: home,
                matchmaking: mm,
                matchIntro: intro,
                countdown: null,
                matchRuntime: match);

            flow.ReturnHome();
            flow.RequestPlay(new PlayRequest { ModeId = "quick_2v2", TeamSize = 2 });
            flow.NotifyMatchResolved(new MatchResolvedInfo
            {
                LobbyId = "L1",
                ModeId = "quick_2v2",
                TeamSize = 2
            });
            Assert.AreEqual(FlowPhaseId.MatchIntro, flow.Current);
            
            flow.NotifyMatchIntroReady();
            // Without countdown, goes straight to Match and StartRound fires immediately
            Assert.AreEqual(FlowPhaseId.Match, flow.Current);
            Assert.AreEqual(1, match.StartRoundCount, "Null countdown path should StartRound on Match enter");
        }

        private sealed class RecordingSidePhases : ISidePhasePort
        {
            public FlowPhaseId LastEnter = FlowPhaseId.None;
            public FlowPhaseId LastExit = FlowPhaseId.None;
            public int EnterCount;
            public int ExitCount;

            public void EnterSidePhase(FlowPhaseId phase, FlowContext context)
            {
                EnterCount++;
                LastEnter = phase;
            }

            public void ExitSidePhase(FlowPhaseId phase)
            {
                ExitCount++;
                LastExit = phase;
            }
        }

        [Test]
        public void EnterSidePhase_DispatchesSidePhasePort_AndCompleteReturnsHome()
        {
            var home = new RecordingHome();
            var sides = new RecordingSidePhases();
            var flow = new AppFlowController(home: home, sidePhases: sides);
            flow.ReturnHome();
            Assert.AreEqual(FlowPhaseId.Home, flow.Current);

            flow.EnterSidePhase(FlowPhaseId.Login);
            Assert.AreEqual(FlowPhaseId.Login, flow.Current);
            Assert.AreEqual(1, sides.EnterCount);
            Assert.AreEqual(FlowPhaseId.Login, sides.LastEnter);

            flow.CompleteSidePhase();
            Assert.AreEqual(FlowPhaseId.Home, flow.Current);
            Assert.AreEqual(1, sides.ExitCount);
            Assert.AreEqual(FlowPhaseId.Login, sides.LastExit);
            Assert.GreaterOrEqual(home.EnterCount, 2);
        }

        [Test]
        public void EnterSidePhase_Chained_PreservesReturnAndExitsPrevious()
        {
            var home = new RecordingHome();
            var sides = new RecordingSidePhases();
            var flow = new AppFlowController(home: home, sidePhases: sides);
            flow.ReturnHome();
            flow.EnterSidePhase(FlowPhaseId.Maintenance);
            Assert.AreEqual(FlowPhaseId.Maintenance, sides.LastEnter);

            flow.EnterSidePhase(FlowPhaseId.Login);
            Assert.AreEqual(FlowPhaseId.Login, flow.Current);
            Assert.AreEqual(FlowPhaseId.Maintenance, sides.LastExit);
            Assert.AreEqual(FlowPhaseId.Login, sides.LastEnter);

            flow.CompleteSidePhase();
            Assert.AreEqual(FlowPhaseId.Home, flow.Current);
        }
    }
}
