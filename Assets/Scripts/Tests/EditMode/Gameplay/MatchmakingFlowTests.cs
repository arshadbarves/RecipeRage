using System;
using System.Reflection;
using Core.Networking;
using Core.Networking.Common;
using Core.Networking.Interfaces;
using Core.Networking.Models;
using Core.RemoteConfig;
using Core.Session;
using Core.UI.Interfaces;
using Cysharp.Threading.Tasks;
using Gameplay.App.State;
using Gameplay.App.State.States;
using Gameplay.Characters;
using Gameplay.Economy;
using Gameplay.GameModes;
using Gameplay.Match;
using Gameplay.Persistence;
using Gameplay.Skins;
using Gameplay.UI.Features.Matchmaking;
using NUnit.Framework;
using UnityEngine;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    public class MatchmakingFlowTests
    {
        [Test]
        public void Enter_UsesQueueSelectedThreeVThreeSizeForMatchmaking()
        {
            GameMode selectedMode = CreateGameMode("team_battle", 2);

            try
            {
                FakeUIService uiService = new();
                FakeMatchmakingService matchmakingService = new();
                MatchmakingState state = new(
                    uiService,
                    new FakeSessionContext(matchmakingService, new FakeGameModeService(selectedMode)),
                    new FakeGameStateManager(),
                    new FakeMaintenanceService(false),
                    new MatchService(new DictionaryConfigService()));

                state.Enter();

                Assert.AreEqual("team_battle", matchmakingService.LastGameModeId);
                Assert.AreEqual(3, matchmakingService.LastTeamSize);
                Assert.AreEqual(typeof(MatchmakingView), uiService.LastRootScreenType);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(selectedMode);
            }
        }

        [Test]
        public void Enter_FallsBackToSelectedModeSizeWhenQueueIsMissing()
        {
            GameMode selectedMode = CreateGameMode("custom_arcade", 4);

            try
            {
                FakeMatchmakingService matchmakingService = new();
                MatchmakingState state = new(
                    new FakeUIService(),
                    new FakeSessionContext(matchmakingService, new FakeGameModeService(selectedMode)),
                    new FakeGameStateManager(),
                    new FakeMaintenanceService(false),
                    new MatchService(new DictionaryConfigService()));

                state.Enter();

                Assert.AreEqual("custom_arcade", matchmakingService.LastGameModeId);
                Assert.AreEqual(4, matchmakingService.LastTeamSize);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(selectedMode);
            }
        }

        [Test]
        public void BuildMatchLobbyConfig_UsesQueueSizedCapacityAndAttributes()
        {
            MethodInfo method = typeof(global::Core.Networking.Services.MatchmakingService).GetMethod(
                "BuildMatchLobbyConfig",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(method);

            LobbyConfig config = method.Invoke(null, new object[] { "team_battle", 3 }) as LobbyConfig;

            Assert.IsNotNull(config);
            Assert.AreEqual(LobbyType.Match, config.Type);
            Assert.AreEqual("team_battle", config.GameModeId);
            Assert.AreEqual(3, config.TeamSize);
            Assert.AreEqual(6, config.MaxPlayers);
            Assert.IsFalse(config.IsPrivate);
            Assert.AreEqual("Match", config.CustomAttributes["Type"]);
            Assert.AreEqual("team_battle", config.CustomAttributes["GameMode"]);
            Assert.AreEqual("3", config.CustomAttributes["TeamSize"]);
            Assert.AreEqual("Filling", config.CustomAttributes["Status"]);
        }

        private static GameMode CreateGameMode(string id, int playersPerTeam)
        {
            GameMode mode = ScriptableObject.CreateInstance<GameMode>();
            SetPrivateField(mode, "_id", id);
            SetPrivateField(mode, "_playersPerTeam", playersPerTeam);
            SetPrivateField(mode, "_teamCount", 2);
            return mode;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing field '{fieldName}' on {target.GetType().Name}");
            field.SetValue(target, value);
        }

        private sealed class DictionaryConfigService : IConfigService
        {
            public int GetInt(string key, int defaultValue) => defaultValue;
            public float GetFloat(string key, float defaultValue) => defaultValue;
            public bool GetBool(string key, bool defaultValue) => defaultValue;
            public string GetString(string key, string defaultValue) => defaultValue;
        }

        private sealed class FakeMaintenanceService : IMaintenanceService
        {
            private readonly bool _isInMaintenance;

            public FakeMaintenanceService(bool isInMaintenance)
            {
                _isInMaintenance = isInMaintenance;
            }

            public UniTask<bool> CheckMaintenanceStatusAsync() => UniTask.FromResult(_isInMaintenance);
            public void ShowServerDownMaintenance(string error) { }
        }

        private sealed class FakeGameModeService : IGameModeService
        {
            public FakeGameModeService(GameMode selectedGameMode)
            {
                SelectedGameMode = selectedGameMode;
            }

            public GameMode SelectedGameMode { get; }
            public event Action<GameMode> OnGameModeChanged;
            public GameMode[] GetAvailableGameModes() => new[] { SelectedGameMode };
            public GameMode GetGameMode(string id) => SelectedGameMode != null && SelectedGameMode.Id == id ? SelectedGameMode : null;
            public bool SelectGameMode(string id) => SelectedGameMode != null && SelectedGameMode.Id == id;
            public bool SelectGameMode(GameMode mode) => SelectedGameMode == mode;
            public UniTask<bool> LoadMapAsync(string sceneName) => UniTask.FromResult(true);
            public UniTask UnloadCurrentMapAsync() => UniTask.CompletedTask;
        }

        private sealed class FakeSessionContext : ISessionContext
        {
            public FakeSessionContext(IMatchmakingService matchmakingService, IGameModeService gameModeService)
            {
                MatchmakingService = matchmakingService;
                GameModeService = gameModeService;
            }

            public bool IsSessionActive => true;
            public ILobbyManager LobbyManager => null;
            public IGameStarter GameStarter => null;
            public IFriendsService FriendsService => null;
            public IMatchmakingService MatchmakingService { get; }
            public IGameModeService GameModeService { get; }
            public ICharacterService CharacterService => null;
            public ISkinsService SkinsService => null;
            public EconomyService EconomyService => null;
            public PlayerDataService PlayerDataService => null;
            public T Resolve<T>() where T : class => null;
        }

        private sealed class FakeMatchmakingService : IMatchmakingService
        {
            public event Action OnMatchmakingStarted;
            public event Action OnMatchmakingCancelled;
            public event Action<string> OnMatchmakingFailed;
            public event Action<int, int> OnPlayersFound;
            public event Action<LobbyInfo> OnMatchFound;

            public bool IsSearching { get; private set; }
            public int PlayersFound { get; private set; }
            public int RequiredPlayers { get; private set; }
            public string LastGameModeId { get; private set; }
            public int LastTeamSize { get; private set; }

            public void Initialize() { }

            public void FindMatch(string gameModeId, int teamSize)
            {
                LastGameModeId = gameModeId;
                LastTeamSize = teamSize;
                RequiredPlayers = teamSize * 2;
                IsSearching = true;
                OnMatchmakingStarted?.Invoke();
            }

            public void CancelMatchmaking()
            {
                IsSearching = false;
                OnMatchmakingCancelled?.Invoke();
            }

            public void SearchForMatchLobbies(string gameModeId, int teamSize, int neededPlayers) { }
            public void CreateAndWaitForPlayers(string gameModeId, int teamSize) { }
            public void FillMatchWithBots() { }
            public System.Collections.Generic.List<BotPlayer> GetActiveBots() => new();
        }

        private sealed class FakeGameStateManager : IGameStateManager
        {
            public IState CurrentState => null;
            public IState PreviousState => null;
            public event Action<IState, IState> OnStateChanged;
            public void Initialize(IState initialState) { }
            public void ChangeState(IState newState) { }
            public void ChangeState<T>() where T : IState { }
            public void Update(float deltaTime) { }
            public void FixedUpdate(float fixedDeltaTime) { }
        }

        private sealed class FakeUIService : IUIService
        {
            public bool IsInitialized => true;
            public Type LastRootScreenType { get; private set; }

            public event Action<Type> OnScreenShown;
            public event Action<Type> OnScreenHidden;
            public event Action OnAllScreensHidden;

            public void SetRootScreen<T>(bool animate = true) where T : class => LastRootScreenType = typeof(T);
            public void SetRootScreen(Type screenType, bool animate = true) => LastRootScreenType = screenType;
            public void PushScreen<T>(bool animate = true) where T : class { }
            public void PushScreen(Type screenType, bool animate = true) { }
            public void ShowSystem<T>(bool animate = true) where T : class { }
            public void ShowSystem(Type screenType, bool animate = true) { }
            public void HideSystem<T>(bool animate = true) where T : class { }
            public void HideSystem(Type screenType, bool animate = true) { }
            public void ShowOverlay<T>(bool animate = true) where T : class { }
            public void ShowOverlay(Type screenType, bool animate = true) { }
            public void HideOverlay<T>(bool animate = true) where T : class { }
            public void HideOverlay(Type screenType, bool animate = true) { }
            public void PushModal<T>(bool animate = true) where T : class { }
            public void PushModal(Type screenType, bool animate = true) { }
            public void PushPopup<T>(bool animate = true) where T : class { }
            public void PushPopup(Type screenType, bool animate = true) { }
            public void ShowHud<T>(bool animate = true) where T : class { }
            public void ShowHud(Type screenType, bool animate = true) { }
            public void HideHud<T>(bool animate = true) where T : class { }
            public void HideHud(Type screenType, bool animate = true) { }
            public bool Back(bool animate = true) => false;
            public UniTask ShowToast(string message, NotificationType type = NotificationType.Info, float duration = 3f) => UniTask.CompletedTask;
            public UniTask ShowToast(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f) => UniTask.CompletedTask;
            public void Show<T>(bool animate = true, bool addToHistory = true) where T : class { }
            public void Show(Type screenType, bool animate = true, bool addToHistory = true) { }
            public void Hide<T>(bool animate = true) where T : class { }
            public void Hide(Type screenType, bool animate = true) { }
            public void HideAllPopups(bool animate = true) { }
            public void HideAllModals(bool animate = true) { }
            public void HideAllGameScreens(bool animate = true) { }
            public void HideAllScreens(bool animate = false) { }
            public T GetScreen<T>() where T : class => null;
            public bool IsScreenVisible<T>() where T : class => false;
            public bool IsScreenVisible(Type screenType) => false;
            public bool GoBack(bool animate = true) => false;
            public void ClearHistory() { }
            public UniTask ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = 3f) => UniTask.CompletedTask;
            public UniTask ShowNotification(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f) => UniTask.CompletedTask;
            public void Update(float deltaTime) { }
        }
    }
}
