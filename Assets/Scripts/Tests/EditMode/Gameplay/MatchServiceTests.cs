using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using KitchenClash.Application;
using KitchenClash.Application.Models;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using NUnit.Framework;
using UnityEngine;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    public class MatchServiceTests
    {
        private MatchService CreateService(DictionaryConfigService config = null)
        {
            config ??= new DictionaryConfigService();
            var database = ScriptableObject.CreateInstance<MapDatabaseSO>();
            var eventBus = new FakeEventBus();
            var mapRotation = new MapRotationCalculator(new StubRemoteConfigService(), new StubNTPTimeService(), new MapRegistry(database, eventBus));
            return new MatchService(config, mapRotation);
        }

        [Test]
        public void GetQueues_ReturnsKitchenClashLaunchQueuesByDefault()
        {
            MatchService service = CreateService();

            IReadOnlyList<MatchQueueDefinition> queues = service.GetQueues();

            Assert.AreEqual(4, queues.Count);
            Assert.AreEqual("quick_2v2", queues[0].ModeId);
            Assert.AreEqual("quick_3v3", queues[1].ModeId);
            Assert.AreEqual("ranked", queues[2].ModeId);
        }

        [Test]
        public void TryGetQueue_FindsExistingQueue()
        {
            MatchService service = CreateService();

            Assert.IsTrue(service.TryGetQueue("quick_2v2", out MatchQueueDefinition queue));
            Assert.AreEqual("quick_2v2", queue.ModeId);
        }

        [Test]
        public void TryGetQueue_ReturnsFalseForUnknownQueue()
        {
            MatchService service = CreateService();

            Assert.IsFalse(service.TryGetQueue("nonexistent", out _));
        }

        private sealed class DictionaryConfigService : IConfigService
        {
            private readonly Dictionary<string, object> _values = new();

            public void Set(string key, object value)
            {
                _values[key] = value;
            }

            public T Get<T>(string key, T fallback)
            {
                if (_values.TryGetValue(key, out object value))
                {
                    try { return (T)Convert.ChangeType(value, typeof(T)); }
                    catch { return fallback; }
                }
                return fallback;
            }

            public Task FetchAsync() => Task.CompletedTask;
        }

        private sealed class StubRemoteConfigService : IRemoteConfigService
        {
            public ConfigHealthStatus HealthStatus => ConfigHealthStatus.Healthy;
            public DateTime LastUpdateTime => DateTime.UtcNow;
            public UniTask<bool> Initialize() => UniTask.FromResult(true);
            public T GetConfig<T>() where T : class, IConfigModel => default;
            public bool TryGetConfig<T>(out T config) where T : class, IConfigModel { config = default; return false; }
            public UniTask<bool> RefreshConfig() => UniTask.FromResult(true);
            public UniTask<bool> RefreshConfig<T>() where T : class, IConfigModel => UniTask.FromResult(true);
        }

        private sealed class StubNTPTimeService : INTPTimeService
        {
            public bool IsSynced => true;
            public DateTime LastSyncTime => DateTime.UtcNow;
            public UniTask<bool> SyncTime() => UniTask.FromResult(true);
            public DateTime GetServerTime() => DateTime.UtcNow;
            public TimeSpan GetTimeOffset() => TimeSpan.Zero;
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
