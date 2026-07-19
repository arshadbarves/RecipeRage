using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using KitchenClash.Application;
using KitchenClash.Application.Config;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Flow.Handlers;
using NUnit.Framework;
using Playcenter.GameFlow;
using Playcenter.Services;
using Playcenter.Shell;
using RecipeRage.Tests.EditMode.Gameplay.Fakes;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    public sealed class BootSequenceConnectivityTests
    {
        // ── Test doubles ────────────────────────────────────────────────────────

        private sealed class FakeConnectivity : IConnectivityService
        {
            public bool IsOnline { get; set; } = true;
            public ConnectivityState CurrentState =>
                IsOnline ? ConnectivityState.Online : ConnectivityState.OfflineMenu;
            public event Action<bool> OnConnectivityChanged { add { } remove { } }
            public event Action<bool> OnConnectionStatusChanged { add { } remove { } }
            public event Action<ConnectivityState> OnStateChanged { add { } remove { } }
            public void NotifyMatchStarted() { }
            public void NotifyMatchEnded() { }
            public void NotifyHostDropped() { }
        }

        private sealed class CountingNtp : INTPTimeService
        {
            public int SyncCalls { get; private set; }
            public bool IsSynced => true;
            public DateTime LastSyncTime => DateTime.UtcNow;
            public Task<bool> SyncTime() { SyncCalls++; return Task.FromResult(true); }
            public DateTime GetServerTime() => DateTime.UtcNow;
            public TimeSpan GetTimeOffset() => TimeSpan.Zero;
        }

        private sealed class StubRemoteConfig : IRemoteConfigService
        {
            public ConfigHealthStatus HealthStatus => ConfigHealthStatus.Healthy;
            public DateTime LastUpdateTime => DateTime.UtcNow;
            public Task<bool> Initialize() => Task.FromResult(true);
            public Task<bool> RefreshConfig() => Task.FromResult(true);
            public Task<bool> RefreshConfig<T>() where T : class, IConfigModel => Task.FromResult(true);
            public T GetConfig<T>() where T : class, IConfigModel => default;
            public bool TryGetConfig<T>(out T config) where T : class, IConfigModel { config = default; return false; }
        }

        private sealed class StubAuth : IAuthService
        {
            // Empty ProductUserId → boot exits to Login without needing SessionLoader.
            public string ProductUserId => string.Empty;
            public bool IsGuest => true;
            public Task<AuthResult> LoginAsGuestAsync() => Task.FromResult(new AuthResult(false));
            public Task<AuthResult> LoginWithGoogleAsync() => Task.FromResult(new AuthResult(false));
            public Task<AuthResult> LoginWithFacebookAsync() => Task.FromResult(new AuthResult(false));
            public Task<AuthResult> LoginWithAppleAsync() => Task.FromResult(new AuthResult(false));
            public Task LinkToGoogleAsync() => Task.CompletedTask;
            public Task LogoutAsync() => Task.CompletedTask;
        }

        private sealed class StubAuthAuthenticated : IAuthService
        {
            public string ProductUserId => "test-puid";
            public bool IsGuest => false;
            public Task<AuthResult> LoginAsGuestAsync() => Task.FromResult(new AuthResult(false));
            public Task<AuthResult> LoginWithGoogleAsync() => Task.FromResult(new AuthResult(false));
            public Task<AuthResult> LoginWithFacebookAsync() => Task.FromResult(new AuthResult(false));
            public Task<AuthResult> LoginWithAppleAsync() => Task.FromResult(new AuthResult(false));
            public Task LinkToGoogleAsync() => Task.CompletedTask;
            public Task LogoutAsync() => Task.CompletedTask;
        }

        private sealed class StubSessionLifecycle : ISessionLifecycle
        {
            public bool IsSessionActive => true;
            public void CreateSession() { }
            public void DestroySession() { }
        }

        private sealed class StubSessionContext : ISessionContext
        {
            public bool IsSessionActive => true;
            public IGameModeService GameModeService => null;
            public ICharacterService CharacterService => null;
            public ISkinsService SkinsService => null;
            public IGameStarter GameStarter => null;
            public IEconomyService EconomyService => null;
            public IPlayerDataService PlayerDataService => null;
            public IFriendsService FriendsService => null;
            public ILobbyManager LobbyManager => null;
            public IMatchmakingService MatchmakingService => null;
            public T Resolve<T>() where T : class => null;
            public void Inject(object target) { }
        }

        private sealed class StubMaintenance : IMaintenanceService
        {
            public bool IsInMaintenance => false;
            public string MaintenanceMessage => string.Empty;
            public DateTime? EstimatedEndTime => null;
            public Task<bool> CheckMaintenanceStatusAsync() => Task.FromResult(false);
        }

        private sealed class StubEventBus : IEventBus
        {
            public void Publish<T>(T evt) where T : class { }
            public void Subscribe<T>(Action<T> handler) where T : class { }
            public void Unsubscribe<T>(Action<T> handler) where T : class { }
            public void ClearAllSubscriptions() { }
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private static BootSequence CreateBoot(
            IConnectivityService connectivity,
            INTPTimeService ntp,
            IAppFlow appFlow,
            IAnalyticsService analytics = null)
        {
            return new BootSequence(
                connectivity,
                ntp,
                new StubRemoteConfig(),
                new StubAuth(),
                new StubMaintenance(),
                new StubEventBus(),
                appFlow,
                sessionLoader: null,
                analytics);
        }

        private static BootSequence CreateBootAuthenticated(
            IConnectivityService connectivity,
            INTPTimeService ntp,
            IAppFlow appFlow,
            IAnalyticsService analytics = null)
        {
            var sessionLoader = new SessionLoader(new StubSessionLifecycle(), new StubSessionContext());
            return new BootSequence(
                connectivity,
                ntp,
                new StubRemoteConfig(),
                new StubAuthAuthenticated(),
                new StubMaintenance(),
                new StubEventBus(),
                appFlow,
                sessionLoader,
                analytics);
        }

        // ── Tests ────────────────────────────────────────────────────────────────

        [Test]
        public async Task RunAsync_WhenOffline_EntersNoConnection_AndSkipsNtp()
        {
            var connectivity = new FakeConnectivity { IsOnline = false };
            var ntp = new CountingNtp();
            var appFlow = new FakeAppFlow();

            BootSequence boot = CreateBoot(connectivity, ntp, appFlow);
            await boot.RunAsync(CancellationToken.None).AsTask();

            Assert.AreEqual(FlowPhaseId.NoConnection, appFlow.LastSidePhase,
                "Offline boot must enter NoConnection side phase");
            Assert.AreEqual(0, ntp.SyncCalls,
                "NTP must not be called when offline");
        }

        [Test]
        public async Task RunAsync_WhenOnline_CallsNtpBeforeProceedingToLogin()
        {
            var connectivity = new FakeConnectivity { IsOnline = true };
            var ntp = new CountingNtp();
            var appFlow = new FakeAppFlow();

            BootSequence boot = CreateBoot(connectivity, ntp, appFlow);
            await boot.RunAsync(CancellationToken.None).AsTask();

            Assert.AreEqual(1, ntp.SyncCalls,
                "NTP must be called on the online path");
            Assert.AreEqual(FlowPhaseId.Login, appFlow.LastSidePhase,
                "Online boot with no auth must proceed to Login");
        }

        [Test]
        public async Task RunAsync_WhenOffline_DoesNotNotifyBootComplete()
        {
            var connectivity = new FakeConnectivity { IsOnline = false };
            var appFlow = new FakeAppFlow();

            BootSequence boot = CreateBoot(connectivity, new CountingNtp(), appFlow);
            await boot.RunAsync(CancellationToken.None).AsTask();

            Assert.AreEqual(0, appFlow.NotifyBootCompleteCount,
                "Boot must not complete when device is offline");
        }

        [Test]
        public async Task RunAsync_WhenOnlineAuthenticated_AndCurrentIsBoot_NotifiesBootComplete()
        {
            var connectivity = new FakeConnectivity { IsOnline = true };
            var appFlow = new FakeAppFlow { Current = FlowPhaseId.Boot };

            BootSequence boot = CreateBootAuthenticated(connectivity, new CountingNtp(), appFlow);
            await boot.RunAsync(CancellationToken.None).AsTask();

            Assert.AreEqual(1, appFlow.NotifyBootCompleteCount,
                "Authenticated cold boot from Boot phase must call NotifyBootComplete");
            Assert.AreEqual(0, appFlow.CompleteSidePhaseCount,
                "Authenticated cold boot from Boot phase must not call CompleteSidePhase");
        }

        [Test]
        public async Task RunAsync_WhenOnlineAuthenticated_AndCurrentIsNoConnection_CompletesSidePhase()
        {
            var connectivity = new FakeConnectivity { IsOnline = true };
            var appFlow = new FakeAppFlow { Current = FlowPhaseId.NoConnection };

            BootSequence boot = CreateBootAuthenticated(connectivity, new CountingNtp(), appFlow);
            await boot.RunAsync(CancellationToken.None).AsTask();

            Assert.AreEqual(1, appFlow.CompleteSidePhaseCount,
                "Retry from NoConnection must call CompleteSidePhase so flow returns to Home");
            Assert.AreEqual(0, appFlow.NotifyBootCompleteCount,
                "Retry from NoConnection must not call NotifyBootComplete");
        }

        [Test]
        public async Task RunAsync_WhenOffline_EmitsBootGateOfflineAnalytics()
        {
            var connectivity = new FakeConnectivity { IsOnline = false };
            var appFlow = new FakeAppFlow();
            var analytics = new SpyAnalytics();

            BootSequence boot = CreateBoot(connectivity, new CountingNtp(), appFlow, analytics);
            await boot.RunAsync(CancellationToken.None).AsTask();

            Assert.Contains(AnalyticsEvents.BootGateOffline, analytics.Events);
        }

        [Test]
        public async Task RunAsync_WhenOnlineAuthenticated_EmitsLoginSuccessAnalytics()
        {
            var connectivity = new FakeConnectivity { IsOnline = true };
            var appFlow = new FakeAppFlow { Current = FlowPhaseId.Boot };
            var analytics = new SpyAnalytics();

            BootSequence boot = CreateBootAuthenticated(connectivity, new CountingNtp(), appFlow, analytics);
            await boot.RunAsync(CancellationToken.None).AsTask();

            Assert.Contains(AnalyticsEvents.LoginSuccess, analytics.Events);
        }
    }
}
