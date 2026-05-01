using System;
using System.Threading.Tasks;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    public class ConfigServiceTests
    {
        [Test]
        public void Get_ReturnsFallbackValue()
        {
            ConfigServiceAdapter service = new(new FakeRemoteConfigService());

            Assert.AreEqual(180, service.Get("missing.int", 180));
            Assert.AreEqual(1.25f, service.Get("missing.float", 1.25f));
            Assert.IsFalse(service.Get("missing.bool", false));
            Assert.AreEqual("fallback", service.Get("missing.string", "fallback"));
        }

        [Test]
        public void FetchAsync_CompletesSuccessfully()
        {
            ConfigServiceAdapter service = new(new FakeRemoteConfigService());

            Task task = service.FetchAsync();

            Assert.IsTrue(task.IsCompleted);
        }

        private sealed class FakeRemoteConfigService : IRemoteConfigService
        {
            public UniTask<bool> Initialize() => UniTask.FromResult(true);
            public T GetConfig<T>() where T : class, IConfigModel => null;
            public bool TryGetConfig<T>(out T config) where T : class, IConfigModel
            {
                config = null;
                return false;
            }

            public UniTask<bool> RefreshConfig() => UniTask.FromResult(true);
            public UniTask<bool> RefreshConfig<T>() where T : class, IConfigModel => UniTask.FromResult(true);
            public ConfigHealthStatus HealthStatus => ConfigHealthStatus.Healthy;
            public DateTime LastUpdateTime => DateTime.UtcNow;
            public event Action<IConfigModel> OnConfigUpdated;
            public event Action<Type, IConfigModel> OnSpecificConfigUpdated;
            public event Action<ConfigHealthStatus> OnHealthStatusChanged;
        }
    }
}
