using NUnit.Framework;
using Core.Authentication;
using Core.Events;
using Core.Maintenance;
using Core.SaveSystem;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Tests.Editor.Authentication
{
    public class AuthenticationServiceTests
    {
        private AuthenticationService _authService;
        private MockSaveService _mockSaveService;
        private MockEventBus _mockEventBus;
        private MockMaintenanceService _mockMaintenanceService;
        private MockEOSWrapper _mockEOSWrapper;
        private bool _sessionCreated;

        [SetUp]
        public void Setup()
        {
            _mockSaveService = new MockSaveService();
            _mockEventBus = new MockEventBus();
            _mockMaintenanceService = new MockMaintenanceService();
            _mockEOSWrapper = new MockEOSWrapper();
            _sessionCreated = false;

            _authService = new AuthenticationService(
                _mockSaveService,
                _mockEventBus,
                _mockMaintenanceService,
                _mockEOSWrapper,
                () => _sessionCreated = true
            );
        }

        [Test]
        public async void InitializeAsync_WhenEOSNotReady_FailsGracefully()
        {
            _mockEOSWrapper.IsReady = false;
            _mockSaveService.Settings.LastLoginMethod = "DeviceID";

            bool result = await _authService.InitializeAsync();

            Assert.IsFalse(result);
            Assert.IsFalse(_sessionCreated);
        }

        [Test]
        public async void LoginAsGuestAsync_WhenEOSNotReady_ReturnsFalse_And_PublishesError()
        {
            _mockEOSWrapper.IsReady = false;

            bool result = await _authService.LoginAsGuestAsync();

            Assert.IsFalse(result);
            Assert.IsNotNull(_mockEventBus.LastPublishedEvent);
            Assert.IsInstanceOf<LoginFailedEvent>(_mockEventBus.LastPublishedEvent);
            Assert.AreEqual("EOS SDK not initialized", ((LoginFailedEvent)_mockEventBus.LastPublishedEvent).Error);
        }
    }

    // ============================================
    // MOCKS
    // ============================================

    public class MockSaveService : ISaveService
    {
        public GameSettingsData Settings = new GameSettingsData();
        public bool IsUserLoggedInVal = false;

        public void Initialize() { }
        public GameSettingsData GetSettings() => Settings;
        public void SaveSettings(GameSettingsData settings) { Settings = settings; }
        public void UpdateSettings(Action<GameSettingsData> updateAction) { updateAction(Settings); }
        public void OnUserLoggedIn() { IsUserLoggedInVal = true; }
        public void OnUserLoggedOut() { IsUserLoggedInVal = false; }
        
        public PlayerProgressData GetProgress() => new PlayerProgressData();
        public void SaveProgress(PlayerProgressData progress) { }
        public void UpdateProgress(Action<PlayerProgressData> updateAction) { }
        
        public PlayerStatsData GetStats() => new PlayerStatsData();
        public void SaveStats(PlayerStatsData stats) { }
        public void UpdateStats(Action<PlayerStatsData> updateAction) { }
    }

    public class MockEventBus : IEventBus
    {
        public object LastPublishedEvent;

        public void Initialize() { }
        public void Publish<T>(T eventMessage) { LastPublishedEvent = eventMessage; }
        public void Subscribe<T>(Action<T> action) { }
        public void Unsubscribe<T>(Action<T> action) { }
        public void ClearAllSubscriptions() { }
    }

    public class MockMaintenanceService : IMaintenanceService
    {
        public void Initialize() { }
        public void ShowServerDownMaintenance(string message) { }
    }

    public class MockEOSWrapper : IEOSWrapper
    {
        public bool IsReady { get; set; } = true;
        
        public Epic.OnlineServices.ProductUserId GetProductUserId() => null;
        public Epic.OnlineServices.Connect.ConnectInterface GetEOSConnectInterface() => null;
        public void StartConnectLoginWithOptions(Epic.OnlineServices.ExternalCredentialType credentialsType, string credentialsId, string displayName, System.Action<Epic.OnlineServices.Connect.LoginCallbackInfo> callback) { }
        public void ClearConnectId(Epic.OnlineServices.ProductUserId localUserId) { }
    }
}