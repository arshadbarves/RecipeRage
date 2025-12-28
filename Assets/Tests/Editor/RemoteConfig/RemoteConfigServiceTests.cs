using NUnit.Framework;
using Core.RemoteConfig;
using Core.RemoteConfig.Models;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Tests.Editor.RemoteConfig
{
    public class RemoteConfigServiceTests
    {
        private RemoteConfigService _service;
        private MockConfigProvider _mockProvider;

        [SetUp]
        public void Setup()
        {
            _mockProvider = new MockConfigProvider();
            _service = new RemoteConfigService(_mockProvider);
        }

        [Test]
        public async void Initialize_Success_SetsHealthToHealthy()
        {
            _mockProvider.ShouldSucceed = true;
            _mockProvider.MockConfigs["GameSettings"] = new GameSettingsConfig();

            bool result = await _service.Initialize();

            Assert.IsTrue(result);
            Assert.AreEqual(ConfigHealthStatus.Healthy, _service.HealthStatus);
        }

        [Test]
        public async void Initialize_ProviderFailure_SetsHealthToFailed()
        {
            _mockProvider.ShouldSucceed = false;

            bool result = await _service.Initialize();

            Assert.IsFalse(result);
            Assert.AreEqual(ConfigHealthStatus.Failed, _service.HealthStatus);
        }
    }

    public class MockConfigProvider : IConfigProvider
    {
        public string ProviderName => "Mock";
        public bool ShouldSucceed = true;
        public Dictionary<string, IConfigModel> MockConfigs = new Dictionary<string, IConfigModel>();

        public bool IsAvailable() => ShouldSucceed;

        public async UniTask<bool> Initialize()
        {
            return await UniTask.FromResult(ShouldSucceed);
        }

        public async UniTask<T> FetchConfig<T>(string key) where T : IConfigModel
        {
            if (MockConfigs.TryGetValue(key, out var config))
            {
                return await UniTask.FromResult((T)config);
            }
            return default;
        }

        public async UniTask<Dictionary<string, IConfigModel>> FetchAllConfigs()
        {
            return await UniTask.FromResult(MockConfigs);
        }
    }
}
