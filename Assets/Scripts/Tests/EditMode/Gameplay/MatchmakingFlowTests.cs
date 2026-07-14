using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KitchenClash.Application;
using KitchenClash.Application.Models;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.DI;
using KitchenClash.Infrastructure.Flow.Handlers;
using KitchenClash.Infrastructure.Network;
using KitchenClash.Infrastructure.Persistence;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Playcenter.GameFlow;
using RecipeRage.Tests.EditMode.Gameplay.Fakes;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    public class MatchmakingFlowTests
    {
        [Test]
        public void MatchmakingPhase_Enter_InvokesMatchmakingService()
        {
            FakeMatchmakingService matchmakingService = new();
            MatchmakingPhase phase = CreatePhase(matchmakingService, new FakeAppFlow());

            phase.Enter(new PlayRequest { ModeId = "quick_2v2", TeamSize = 2 }, null);

            Assert.AreEqual("quick_2v2", matchmakingService.LastGameModeId);
            Assert.AreEqual(2, matchmakingService.LastTeamSize);
        }

        [Test]
        public void MatchmakingPhase_Exit_CancelsMatchmaking()
        {
            FakeMatchmakingService matchmakingService = new();
            MatchmakingPhase phase = CreatePhase(matchmakingService, new FakeAppFlow());

            phase.Enter(new PlayRequest { ModeId = "quick_2v2", TeamSize = 2 }, null);
            Assert.IsTrue(matchmakingService.IsSearching);

            phase.Exit();
            Assert.IsFalse(matchmakingService.IsSearching);
        }

        [Test]
        public void MatchmakingPhase_OnMatchFound_NotifiesAppFlow()
        {
            FakeAppFlow flow = new();
            FakeMatchmakingService matchmakingService = new();
            MatchmakingPhase phase = CreatePhase(matchmakingService, flow);

            phase.Enter(new PlayRequest { ModeId = "quick_2v2", TeamSize = 2 }, null);

            LobbyInfo lobbyInfo = new()
            {
                LobbyId = "test-lobby-123",
                GameModeId = "quick_2v2",
                MapName = "TestMap",
                Players = new List<PlayerInfo>()
            };
            matchmakingService.RaiseMatchFound(lobbyInfo);

            Assert.AreEqual(1, flow.NotifyMatchResolvedCount, "NotifyMatchResolved should be called once");
        }

        [Test]
        public void MatchmakingPhase_OnMatchmakingCancelled_CallsCancelMatchmaking()
        {
            FakeAppFlow flow = new();
            FakeMatchmakingService matchmakingService = new();
            MatchmakingPhase phase = CreatePhase(matchmakingService, flow);

            phase.Enter(new PlayRequest { ModeId = "quick_2v2", TeamSize = 2 }, null);
            matchmakingService.RaiseCancelled();

            Assert.AreEqual(1, flow.CancelMatchmakingCount, "CancelMatchmaking should be called once");
        }

        [Test]
        public void MatchmakingPhase_OnMatchmakingFailed_CallsReturnHome()
        {
            FakeAppFlow flow = new();
            FakeMatchmakingService matchmakingService = new();
            MatchmakingPhase phase = CreatePhase(matchmakingService, flow);

            phase.Enter(new PlayRequest { ModeId = "quick_2v2", TeamSize = 2 }, null);
            matchmakingService.RaiseFailed("Connection lost");

            Assert.AreEqual(1, flow.ReturnHomeCount, "ReturnHome should be called once");
        }

        [Test]
        public void MatchEndEvaluator_EvaluateScoreLimit_ReturnsNoEnd_WhenBelowTarget()
        {
            MatchEndEvaluation result = MatchEndEvaluator.EvaluateScoreLimit(new[] { 800, 950 }, true, 1000);

            Assert.IsFalse(result.ShouldEnd);
        }

        [Test]
        public void MatchEndEvaluator_EvaluateFinalScores_ReturnsDraw_WhenEqual()
        {
            MatchEndEvaluation result = MatchEndEvaluator.EvaluateFinalScores(new[] { 1200, 1200 });

            Assert.IsTrue(result.ShouldEnd);
            Assert.IsTrue(result.IsDraw);
            Assert.AreEqual(-1, result.WinningTeamId);
        }

        private static MatchmakingPhase CreatePhase(IMatchmakingService matchmakingService, IAppFlow appFlow)
        {
            return new MatchmakingPhase(
                new FakeUIService(),
                new FakeSessionContext(matchmakingService, new FakeGameModeService()),
                new FakeMaintenanceService(false),
                new FakeConfigService(),
                new FakeEventBus(),
                appFlow,
                matchmakingService);
        }

        private sealed class FakeMaintenanceService : IMaintenanceService
        {
            private readonly bool _isInMaintenance;

            public FakeMaintenanceService(bool isInMaintenance)
            {
                _isInMaintenance = isInMaintenance;
            }

            public bool IsInMaintenance => _isInMaintenance;
            public string MaintenanceMessage => "";
            public DateTime? EstimatedEndTime => null;
            public Task<bool> CheckMaintenanceStatusAsync() => Task.FromResult(_isInMaintenance);
        }

        private sealed class FakeGameModeService : IGameModeService
        {
            public GameMode SelectedGameMode => null;
            public event Action<GameMode> OnGameModeChanged;
            public GameMode[] GetAvailableGameModes() => Array.Empty<GameMode>();
            public GameMode GetGameMode(string id) => null;
            public bool SelectGameMode(string id) => false;
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
            public List<BotPlayer> GetActiveBots() => new();

            public void RaiseMatchFound(LobbyInfo lobbyInfo)
            {
                IsSearching = false;
                OnMatchFound?.Invoke(lobbyInfo);
            }

            public void RaiseCancelled()
            {
                IsSearching = false;
                OnMatchmakingCancelled?.Invoke();
            }

            public void RaiseFailed(string reason)
            {
                IsSearching = false;
                OnMatchmakingFailed?.Invoke(reason);
            }
        }

        private sealed class FakeUIService : IUIService
        {
            public bool IsInitialized => true;

            public event Action<Type> OnScreenShown;
            public event Action<Type> OnScreenHidden;
            public event Action OnAllScreensHidden;

            public void SetRootScreen<T>(bool animate = true) where T : class { }
            public void SetRootScreen(Type screenType, bool animate = true) { }
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
            public void SetCurrentScope(VContainer.IObjectResolver scope) { }
        }

        private sealed class FakeConfigService : IConfigService
        {
            public T Get<T>(string key, T fallback) => fallback;
            public Task FetchAsync() => Task.CompletedTask;
        }

        private sealed class FakeEventBus : IEventBus
        {
            public void Publish<T>(T evt) where T : class { }
            public void Subscribe<T>(Action<T> handler) where T : class { }
            public void Unsubscribe<T>(Action<T> handler) where T : class { }
            public void ClearAllSubscriptions() { }
        }
    }
}
