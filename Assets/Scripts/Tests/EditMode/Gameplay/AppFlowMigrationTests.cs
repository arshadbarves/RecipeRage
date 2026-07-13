using NUnit.Framework;
using Playcenter.GameFlow;
using KitchenClash.Presentation.ViewModels;
using KitchenClash.Infrastructure.DI;
using KitchenClash.Application.Services;
using KitchenClash.Domain;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    /// <summary>
    /// Tests verifying Presentation layer migrated from IGameStateManager to IAppFlow intents.
    /// </summary>
    public class AppFlowMigrationTests
    {
        private sealed class FakeAppFlow : IAppFlow
        {
            public int RequestPlayCount;
            public PlayRequest LastPlayRequest;
            public int ReturnHomeCount;
            public int NotifyMatchCompletedCount;
            public MatchResultInfo LastMatchResult;

            public FlowPhaseId Current => FlowPhaseId.Home;
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

            public void StartColdBoot() { }
            public void CancelMatchmaking() { }
            public void NotifyMatchResolved(MatchResolvedInfo info) { }
            public void NotifyMatchIntroReady() { }
            public void NotifyCountdownComplete() { }
            public void NotifySplashComplete() { }
            public void NotifyBootComplete() { }
            public void RequestPlayAgain() { }
            public void EnterSidePhase(FlowPhaseId sidePhase) { }
            public void CompleteSidePhase() { }
            public bool CanShowSoftPopup() => false;

            public event System.Action<FlowPhaseId, FlowPhaseId> PhaseChanged
            {
                add { }
                remove { }
            }
        }

        private sealed class FakeGameModeService : IGameModeService
        {
            public GameMode SelectedGameMode { get; set; }
            public event System.Action<GameMode> OnGameModeChanged
            {
                add { }
                remove { }
            }
        }

        private sealed class FakeSessionContext : ISessionContext
        {
            public IGameModeService GameModeService { get; set; }
            public ILobbyManager LobbyManager => null;
            public IPlayerManager PlayerManager => null;
            public IMatchmakingService MatchmakingService => null;
            public ITeamManager TeamManager => null;
            public IGameStarter GameStarter => null;
        }

        [Test]
        public void LobbyViewModel_Play_CallsRequestPlay_WithModeId()
        {
            // Arrange
            var flow = new FakeAppFlow();
            var gameModeService = new FakeGameModeService
            {
                SelectedGameMode = new GameMode
                {
                    Id = "quick_2v2",
                    DisplayName = "Quick 2v2"
                }
            };
            var sessionContext = new FakeSessionContext
            {
                GameModeService = gameModeService
            };
            var vm = new LobbyViewModel(sessionContext, flow);

            // Act
            vm.Play();

            // Assert
            Assert.AreEqual(1, flow.RequestPlayCount, "RequestPlay should be called once");
            Assert.IsNotNull(flow.LastPlayRequest, "PlayRequest should not be null");
            Assert.AreEqual("quick_2v2", flow.LastPlayRequest.ModeId, "ModeId should match selected mode");
            Assert.AreEqual(2, flow.LastPlayRequest.TeamSize, "TeamSize should default to 2");
        }

        [Test]
        public void LobbyViewModel_Play_WithNullGameMode_CallsRequestPlay_WithDefaultTeamSize()
        {
            // Arrange
            var flow = new FakeAppFlow();
            var sessionContext = new FakeSessionContext
            {
                GameModeService = null
            };
            var vm = new LobbyViewModel(sessionContext, flow);

            // Act
            vm.Play();

            // Assert
            Assert.AreEqual(1, flow.RequestPlayCount, "RequestPlay should be called once");
            Assert.IsNotNull(flow.LastPlayRequest, "PlayRequest should not be null");
            Assert.AreEqual(2, flow.LastPlayRequest.TeamSize, "TeamSize should default to 2");
        }
    }
}
