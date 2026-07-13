using NUnit.Framework;
using Playcenter.GameFlow;
using KitchenClash.Presentation.ViewModels;
using KitchenClash.Infrastructure.DI;
using KitchenClash.Application.Services;
using KitchenClash.Application;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Persistence;
using RecipeRage.Tests.EditMode.Gameplay.Fakes;
using Cysharp.Threading.Tasks;
using GameMode = KitchenClash.Application.Models.GameMode;
using UnityEngine;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    /// <summary>
    /// Tests verifying Presentation layer migrated from IGameStateManager to IAppFlow intents.
    /// </summary>
    public class AppFlowMigrationTests
    {
        private sealed class FakeGameModeService : IGameModeService
        {
            public GameMode SelectedGameMode { get; set; }

            public GameMode[] GetAvailableGameModes() => null;
            public GameMode GetGameMode(string id) => null;
            public bool SelectGameMode(string id) => true;
            public UniTask<bool> LoadMapAsync(string sceneName) => UniTask.FromResult(true);
            public UniTask UnloadCurrentMapAsync() => UniTask.CompletedTask;

            public event System.Action<GameMode> OnGameModeChanged
            {
                add { }
                remove { }
            }
        }

        private sealed class FakeSessionContext : ISessionContext
        {
            public bool IsSessionActive => true;
            public IGameModeService GameModeService { get; set; }
            public ICharacterService CharacterService => null;
            public ISkinsService SkinsService => null;
            public IGameStarter GameStarter => null;
            public EconomyService EconomyService => null;
            public PlayerDataService PlayerDataService => null;
            public IFriendsService FriendsService => null;
            public ILobbyManager LobbyManager => null;
            public IMatchmakingService MatchmakingService => null;

            public T Resolve<T>() where T : class => null;
        }

        [Test]
        public void LobbyViewModel_Play_CallsRequestPlay_WithModeId()
        {
            // Arrange
            var flow = new FakeAppFlow();
            var gameMode = ScriptableObject.CreateInstance<GameMode>();
            typeof(GameMode).GetField("_id", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(gameMode, "quick_2v2");
            typeof(GameMode).GetField("_displayName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(gameMode, "Quick 2v2");

            var gameModeService = new FakeGameModeService
            {
                SelectedGameMode = gameMode
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
