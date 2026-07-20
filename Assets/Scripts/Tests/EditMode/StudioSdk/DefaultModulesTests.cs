using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Playcenter.SDK;
using Playcenter.Shell;

namespace RecipeRage.Tests.EditMode.StudioSdk
{
    public sealed class DefaultModulesTests
    {
        [Test]
        public void DefaultModulePack_Create_ReturnsNineModulesInSpecOrder()
        {
            var modules = DefaultModulePack.Create();

            Assert.AreEqual(9, modules.Count);
            CollectionAssert.AreEqual(
                new[]
                {
                    "logging",
                    "connectivity",
                    "ntp",
                    "remote_config",
                    "force_update",
                    "maintenance",
                    "auth_warmup",
                    "analytics",
                    "shell_ready"
                },
                modules.Select(m => m.Id).ToArray());
            CollectionAssert.AreEqual(
                new[] { 0.05f, 0.15f, 0.10f, 0.15f, 0.10f, 0.10f, 0.15f, 0.10f, 0.10f },
                modules.Select(m => m.Weight).ToArray());
        }

        [Test]
        public void UseDefaultModules_AddsNineModules()
        {
            var options = new ClientOptions();
            options.UseDefaultModules();
            Assert.AreEqual(9, options.Modules.Count);
        }

        [Test]
        public async Task ConnectivityModule_WhenOffline_FailsOffline()
        {
            var reg = new ServiceRegistry();
            reg.AddSingleton<IConnectivityService>(new FakeConnectivity(false));
            var services = reg.Build();
            var mod = new ConnectivityModule();
            var ctx = new ModuleContext(services, new BootProgress(new[] { (mod.Id, mod.Weight) }));

            ModuleResult result = await mod.InitializeAsync(ctx, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(BootFailureCode.Offline, result.Failure.Code);
        }

        [Test]
        public async Task ConnectivityModule_WhenOnline_Succeeds()
        {
            var reg = new ServiceRegistry();
            reg.AddSingleton<IConnectivityService>(new FakeConnectivity(true));
            var services = reg.Build();
            var mod = new ConnectivityModule();
            var ctx = new ModuleContext(services, new BootProgress(new[] { (mod.Id, mod.Weight) }));

            ModuleResult result = await mod.InitializeAsync(ctx, CancellationToken.None);

            Assert.IsTrue(result.Success);
        }

        [Test]
        public async Task ConnectivityModule_WhenServiceMissing_FailsOffline()
        {
            var services = new ServiceRegistry().Build();
            var mod = new ConnectivityModule();
            var ctx = new ModuleContext(services, new BootProgress(new[] { (mod.Id, mod.Weight) }));

            ModuleResult result = await mod.InitializeAsync(ctx, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(BootFailureCode.Offline, result.Failure.Code);
        }

        [Test]
        public async Task ForceUpdateModule_WhenPolicyMissing_Succeeds()
        {
            var services = new ServiceRegistry().Build();
            var mod = new ForceUpdateModule();
            var ctx = new ModuleContext(services, new BootProgress(new[] { (mod.Id, mod.Weight) }));

            ModuleResult result = await mod.InitializeAsync(ctx, CancellationToken.None);

            Assert.IsTrue(result.Success);
        }

        [Test]
        public async Task ForceUpdateModule_WhenPolicyRequiresUpdate_FailsForceUpdate()
        {
            var reg = new ServiceRegistry();
            reg.AddSingleton<IForceUpdatePolicy>(new FakeForceUpdatePolicy(
                new ForceUpdateDecision(true, "Please update", "https://store.example")));
            var services = reg.Build();
            var mod = new ForceUpdateModule();
            var ctx = new ModuleContext(services, new BootProgress(new[] { (mod.Id, mod.Weight) }));

            ModuleResult result = await mod.InitializeAsync(ctx, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(BootFailureCode.ForceUpdate, result.Failure.Code);
            Assert.AreEqual("Please update", result.Failure.Message);
        }

        [Test]
        public async Task AuthWarmupModule_AlwaysSucceedsWithoutLogin()
        {
            var services = new ServiceRegistry().Build();
            var mod = new AuthWarmupModule();
            var ctx = new ModuleContext(services, new BootProgress(new[] { (mod.Id, mod.Weight) }));

            ModuleResult result = await mod.InitializeAsync(ctx, CancellationToken.None);

            Assert.IsTrue(result.Success);
        }

        sealed class FakeConnectivity : IConnectivityService
        {
            public FakeConnectivity(bool isOnline) => IsOnline = isOnline;

            public bool IsOnline { get; }
            public ConnectivityState CurrentState =>
                IsOnline ? ConnectivityState.Online : ConnectivityState.OfflineMenu;

            public event Action<bool> OnConnectivityChanged
            {
                add { }
                remove { }
            }

            public event Action<bool> OnConnectionStatusChanged
            {
                add { }
                remove { }
            }

            public event Action<ConnectivityState> OnStateChanged
            {
                add { }
                remove { }
            }

            public void NotifyMatchStarted() { }
            public void NotifyMatchEnded() { }
            public void NotifyHostDropped() { }
        }

        sealed class FakeForceUpdatePolicy : IForceUpdatePolicy
        {
            readonly ForceUpdateDecision _decision;

            public FakeForceUpdatePolicy(ForceUpdateDecision decision) => _decision = decision;

            public Task<ForceUpdateDecision> EvaluateAsync(CancellationToken ct) =>
                Task.FromResult(_decision);
        }
    }
}
