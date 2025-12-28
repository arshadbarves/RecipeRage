using NUnit.Framework;
using Core.State.States;
using Core.Bootstrap;
using Core.Events;
using Core.Logging;
using Core.Maintenance;
using Core.RemoteConfig;
using Core.Update;
using Core.Authentication;
using UI;
using UI.Screens;
using UI.Core;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;
using Core.State;
using UnityEngine;

namespace Tests.Editor.States
{
    public class BootstrapStateTests
    {
        private BootstrapState _bootstrapState;
        private MockUIService _mockUIService;
        private MockNTPTimeService _mockNTP;
        private MockRemoteConfigService _mockRemoteConfig;
        private MockAuthenticationService _mockAuth;
        private MockMaintenanceService _mockMaintenance;
        private MockGameStateManager _mockStateManager;
        private MockEventBus _mockEventBus;

        [SetUp]
        public void Setup()
        {
            _mockUIService = new MockUIService();
            _mockNTP = new MockNTPTimeService();
            _mockRemoteConfig = new MockRemoteConfigService();
            _mockAuth = new MockAuthenticationService();
            _mockMaintenance = new MockMaintenanceService();
            _mockStateManager = new MockGameStateManager();
            _mockEventBus = new MockEventBus();

            // We pass null for ServiceContainer as we don't expect it to be used in the tested flow
            // except for creating the next state, which we verify via MockStateManager
            _bootstrapState = new BootstrapState(
                _mockUIService,
                _mockNTP,
                _mockRemoteConfig,
                _mockAuth,
                _mockMaintenance,
                _mockStateManager,
                _mockEventBus,
                null
            );
        }

        [Test]
        public void Enter_StartsInitializationSequence()
        {
            // Act
            _bootstrapState.Enter();

            // Assert
            // 1. Shows Splash
            Assert.IsTrue(_mockUIService.ShownScreens.Contains(UIScreenType.Splash));

            // Note: Since the method is async void, we can't easily await it in a synchronous test.
            // However, we can verify the initial synchronous calls.
            // The Splash screen logic awaits a delay, so the loading screen might not be shown immediately in this tick.
        }
    }

    // ============================================
    // MOCKS
    // ============================================

        public class MockUIService : IUIService
        {
            public List<UIScreenType> ShownScreens = new List<UIScreenType>();
            public List<UIScreenType> HiddenScreens = new List<UIScreenType>();
            public ConfigHealthStatus HealthStatus => ConfigHealthStatus.Healthy;
            public DateTime LastUpdateTime => DateTime.MinValue;
            public bool IsInitialized => true;
    
            public event Action<IConfigModel> OnConfigUpdated;
            public event Action<Type, IConfigModel> OnSpecificConfigUpdated;
            public event Action<ConfigHealthStatus> OnHealthStatusChanged;
            public event Action<UIScreenType> OnScreenShown;
            public event Action<UIScreenType> OnScreenHidden;
            public event Action OnAllScreensHidden;
    
            public void Initialize() { }
            public void Initialize(UnityEngine.UIElements.UIDocument uiDocument) { }
            public void InitializeScreens() { }
            public void Dispose() { }
    
            public void ShowScreen(UIScreenType screenType, bool animate = true, bool addToHistory = true)
            {
                ShownScreens.Add(screenType);
                OnScreenShown?.Invoke(screenType);
            }
    
            public void HideScreen(UIScreenType screenType, bool animate = true)
            {
                HiddenScreens.Add(screenType);
                OnScreenHidden?.Invoke(screenType);
            }
    
            public T GetScreen<T>() where T : BaseUIScreen
            {
                // For testing, we return a dummy screen or null. 
                // Since BootstrapState calls GetScreen<LoadingScreen>, we need to return something or handle null.
                // Mocking BaseUIScreen is hard because it's a class not interface. 
                // Ideally we'd wrap it or the state would rely on interface.
                // For now, returning null and ensuring the state handles nulls (using ?.)
                return null;
            }
    
            public BaseUIScreen GetScreen(UIScreenType screenType) => null;
            public T GetScreen<T>(UIScreenType screenType) where T : BaseUIScreen => null;
            
            public bool TryGetConfig<T>(out T config) where T : class, IConfigModel { config = null; return false; }
            public T GetConfig<T>() where T : class, IConfigModel => null;
            public UniTask<bool> RefreshConfig() => UniTask.FromResult(true);
            public UniTask<bool> RefreshConfig<T>() where T : class, IConfigModel => UniTask.FromResult(true);
            public UniTask<bool> InitializeAsync() => UniTask.FromResult(true); // Added to satisfy IInitializable/Service pattern if needed
            
            // Other IUIService methods stubbed
            public void HideScreensOfType(UIScreenType screenType, bool animate = true) { }
            public void HideAllPopups(bool animate = true) { }
            public void HideAllModals(bool animate = true) { }
            public void HideAllGameScreens(bool animate = true) { }
            public void HideAllScreens(bool animate = false) { }
            public bool GoBack(bool animate = true) => false;
            public bool IsScreenVisible(UIScreenType screenType) => false;
            public IReadOnlyList<BaseUIScreen> GetVisibleScreens() => new List<BaseUIScreen>();
            public IReadOnlyList<BaseUIScreen> GetScreensByPriority() => new List<BaseUIScreen>();
            public void ClearHistory() { }
            public UIScreenCategory GetScreenCategory(UIScreenType screenType) => UIScreenCategory.Screen;
            public bool IsInteractionBlocked(UIScreenType screenType) => false;
            public UniTask ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = 3f) => UniTask.CompletedTask;
            public UniTask ShowNotification(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f) => UniTask.CompletedTask;
    
            public void Update(float deltaTime) { }
        }
    
        public class MockNTPTimeService : INTPTimeService
        {
            public bool IsSynced => true;
            public DateTime LastSyncTime => DateTime.UtcNow;
    
            public void Initialize() { }
            public UniTask<bool> SyncTime() => UniTask.FromResult(true);
            public DateTime GetServerTime() => DateTime.UtcNow;
            public TimeSpan GetTimeOffset() => TimeSpan.Zero;
        }
    
        public class MockRemoteConfigService : IRemoteConfigService
        {
            public ConfigHealthStatus HealthStatus => ConfigHealthStatus.Healthy;
            public DateTime LastUpdateTime => DateTime.UtcNow;
    
            public event Action<IConfigModel> OnConfigUpdated;
            public event Action<Type, IConfigModel> OnSpecificConfigUpdated;
            public event Action<ConfigHealthStatus> OnHealthStatusChanged;
    
            public void Initialize() { }
            UniTask<bool> IRemoteConfigService.Initialize() => UniTask.FromResult(true);
            public T GetConfig<T>() where T : class, IConfigModel => null;
            public bool TryGetConfig<T>(out T config) where T : class, IConfigModel { config = null; return false; }
            public UniTask<bool> RefreshConfig() => UniTask.FromResult(true);
            public UniTask<bool> RefreshConfig<T>() where T : class, IConfigModel => UniTask.FromResult(true);
        }
    
        public class MockAuthenticationService : IAuthenticationService
        {
            public bool IsLoggedIn => true;
            public string LastLoginMethod => "DeviceID";
    
            public void Initialize() { }
            public UniTask<bool> InitializeAsync() => UniTask.FromResult(true);
            public UniTask<bool> AttemptAutoLoginAsync() => UniTask.FromResult(true);
            public UniTask<bool> LoginAsGuestAsync() => UniTask.FromResult(true);
            public UniTask<bool> LoginWithFacebookAsync() => UniTask.FromResult(false);
            public UniTask LogoutAsync() => UniTask.CompletedTask;
        }
    
        public class MockMaintenanceService : IMaintenanceService
        {
            public void Initialize() { }
            public void ShowServerDownMaintenance(string message) { }
            public UniTask<bool> CheckMaintenanceStatusAsync() => UniTask.FromResult(false);
        }
    
        public class MockGameStateManager : IGameStateManager
        {
            public IState CurrentState { get; private set; }
            public IState PreviousState { get; private set; }
    
            public event Action<IState, IState> OnStateChanged;
    
            public void Initialize(IState initialState) { CurrentState = initialState; }
            public void Initialize() { }
            public void ChangeState(IState newState) { PreviousState = CurrentState; CurrentState = newState; OnStateChanged?.Invoke(PreviousState, CurrentState); }
            public void ChangeState<T>() where T : IState, new() { ChangeState(new T()); }
            public void Update(float deltaTime) { }
            public void FixedUpdate(float fixedDeltaTime) { }
        }
    
        public class MockEventBus : IEventBus
        {
            public void Initialize() { }
            public void Publish<T>(T eventMessage) where T : class { }
            public void Subscribe<T>(Action<T> action) where T : class { }
            public void Unsubscribe<T>(Action<T> action) where T : class { }
            public void ClearSubscriptions<T>() where T : class { }
            public void ClearAllSubscriptions() { }
        }}
