using System;
using System.Collections.Generic;
using Core.RemoteConfig.Enums;
using Core.RemoteConfig.Interfaces;
using Core.RemoteConfig.Models;
using Cysharp.Threading.Tasks;
using Gameplay.Match;
using NUnit.Framework;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    public class ConfigServiceTests
    {
        [Test]
        public void GetValues_ReadsCustomRulesAndConvertsPrimitiveTypes()
        {
            GameSettingsConfig config = new()
            {
                CustomGameRules = new Dictionary<string, object>
                {
                    ["queues.classic.duration_seconds"] = 210L,
                    ["queues.ranked.enabled"] = "true",
                    ["queues.team_battle.display_name"] = "Scrim 3v3",
                    ["scoring.multiplier"] = 1.5d
                }
            };

            ConfigService service = new(new FakeRemoteConfigService(config));

            Assert.AreEqual(210, service.GetInt("queues.classic.duration_seconds", 180));
            Assert.IsTrue(service.GetBool("queues.ranked.enabled", false));
            Assert.AreEqual("Scrim 3v3", service.GetString("queues.team_battle.display_name", "Quick 3v3"));
            Assert.AreEqual(1.5f, service.GetFloat("scoring.multiplier", 1f));
        }

        [Test]
        public void GetValues_UsesFallbackWhenRuleMissing()
        {
            ConfigService service = new(new FakeRemoteConfigService(new GameSettingsConfig()));

            Assert.AreEqual(180, service.GetInt("missing.int", 180));
            Assert.AreEqual(1.25f, service.GetFloat("missing.float", 1.25f));
            Assert.IsFalse(service.GetBool("missing.bool", false));
            Assert.AreEqual("fallback", service.GetString("missing.string", "fallback"));
        }

        private sealed class FakeRemoteConfigService : IRemoteConfigService
        {
            private readonly GameSettingsConfig _config;

            public FakeRemoteConfigService(GameSettingsConfig config)
            {
                _config = config;
            }

            public UniTask<bool> Initialize() => UniTask.FromResult(true);
            public T GetConfig<T>() where T : class, IConfigModel => _config as T;
            public bool TryGetConfig<T>(out T config) where T : class, IConfigModel
            {
                config = _config as T;
                return config != null;
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
