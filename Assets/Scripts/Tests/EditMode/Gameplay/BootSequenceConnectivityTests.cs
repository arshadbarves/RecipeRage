using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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
            public Task<AuthResult> LoginAsGuestAsync() => Task.FromResult(new AuthResult());
            public Task<AuthResult> LoginWithGoogleAsync() => Task.FromResult(new AuthResult());
            public Task<AuthResult> LoginWithFacebookAsync() => Task.FromResult(new AuthResult());
            public Task<AuthResult> LoginWithAppleAsync() => Task.FromResult(new AuthResult());
            public Task LinkToGoogleAsync() => Task.CompletedTask;
            public Task LogoutAsync() => Task.CompletedTask;
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
            IAppFlow appFlow)
        {
            return new BootSequence(
                connectivity,
                ntp,
                new StubRemoteConfig(),
                new StubAuth(),
                new StubMaintenance(),
                new StubEventBus(),
                appFlow,
                sessionLoader: null);
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
    }
}
