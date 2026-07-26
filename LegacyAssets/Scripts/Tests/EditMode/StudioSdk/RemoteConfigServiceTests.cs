using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Playcenter.Services;

namespace RecipeRage.Tests.EditMode.StudioSdk
{
    public sealed class RemoteConfigServiceTests
    {
        private sealed class FakeModel : IConfigModel
        {
            public int Value = 1;
            public bool IsValid() => true;
        }

        private sealed class FakeProvider : IConfigProvider
        {
            public bool Available = true;
            public bool InitResult = true;
            public Dictionary<string, IConfigModel> All = new();
            public string ProviderName => "Fake";
            public bool IsAvailable() => Available;
            public Task<bool> Initialize() => Task.FromResult(InitResult);
            public Task<T> FetchConfig<T>(string key) where T : IConfigModel =>
                Task.FromResult(All.TryGetValue(key, out var m) ? (T)m : default);
            public Task<Dictionary<string, IConfigModel>> FetchAllConfigs() => Task.FromResult(All);
        }

        [Test]
        public async Task Initialize_ProviderHealthy_StatusHealthy()
        {
            var svc = new RemoteConfigService(new FakeProvider());
            await svc.Initialize();
            Assert.AreEqual(ConfigHealthStatus.Healthy, svc.HealthStatus);
        }

        [Test]
        public async Task Initialize_ProviderUnavailable_StatusDegraded()
        {
            var svc = new RemoteConfigService(new FakeProvider { InitResult = false, Available = false });
            await svc.Initialize();
            Assert.AreEqual(ConfigHealthStatus.Degraded, svc.HealthStatus);
        }

        [Test]
        public async Task Refresh_CachesModel_AndRaisesOnConfigUpdated()
        {
            var provider = new FakeProvider();
            provider.All["FakeModel"] = new FakeModel { Value = 42 };
            var svc = new RemoteConfigService(provider);
            await svc.Initialize();
            IConfigModel raised = null;
            svc.OnConfigUpdated += m => raised = m;
            await svc.RefreshConfig();
            Assert.IsTrue(svc.TryGetConfig<FakeModel>(out var cfg));
            Assert.AreEqual(42, cfg.Value);
            Assert.IsNotNull(raised);
        }

        [Test]
        public void HealthChange_RaisesOnHealthChanged()
        {
            var svc = new RemoteConfigService(new FakeProvider { InitResult = false, Available = false });
            ConfigHealthStatus? raised = null;
            svc.OnHealthChanged += s => raised = s;
            svc.Initialize().Wait();
            Assert.AreEqual(ConfigHealthStatus.Degraded, raised);
        }

        [Test]
        public void Get_UnknownRawKey_ReturnsFallback()
        {
            var svc = new RemoteConfigService(new FallbackConfigProvider());
            svc.Initialize().Wait();
            Assert.AreEqual(7, svc.Get("missing", 7));
        }
    }
}
