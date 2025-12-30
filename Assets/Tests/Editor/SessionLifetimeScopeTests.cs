using NUnit.Framework;
using VContainer;
using VContainer.Unity;
using UnityEngine;
using Core.Bootstrap;
using Core.Audio;
using Core.Characters;
using Core.Currency;
using Core.Events;
using Core.GameModes;
using Core.Input;
using Core.Logging;
using Core.Networking;
using Core.Networking.Services;
using Core.SaveSystem;
using Core.Skins;
using System.Reflection;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Tests.Editor
{
    public class SessionLifetimeScopeTests
    {
        private class TestableSessionLifetimeScope : SessionLifetimeScope
        {
            public new void Configure(IContainerBuilder builder)
            {
                base.Configure(builder);
            }
        }

        private class DummySaveService : ISaveService 
        {
            public void Initialize() {}
            public void SaveData<T>(string key, T data) where T : class, new() {}
            public T LoadData<T>(string key) where T : class, new() => new T();
            public void DeleteData(string key) {}
            public GameSettingsData GetSettings() => new GameSettingsData();
            public void SaveSettings(GameSettingsData settings) {}
            public void UpdateSettings(Action<GameSettingsData> updateAction) {}
            public event Action<GameSettingsData> OnSettingsChanged;

            public PlayerProgressData GetPlayerProgress() => new PlayerProgressData();
            public void SavePlayerProgress(PlayerProgressData progress) {}
            public void UpdatePlayerProgress(Action<PlayerProgressData> updateAction) {}
            public event Action<PlayerProgressData> OnPlayerProgressChanged;

            public PlayerStatsData GetPlayerStats() => new PlayerStatsData();
            public void SavePlayerStats(PlayerStatsData stats) {}
            public void UpdatePlayerStats(Action<PlayerStatsData> updateAction) {}
            public event Action<PlayerStatsData> OnPlayerStatsChanged;

            public void DeleteAllData() {}
            public void ClearUserCache() {}
            public SyncStatus GetSyncStatus(string key) => null;
            public UniTask SyncAllCloudDataAsync() => UniTask.CompletedTask;
            public void OnUserLoggedIn() {}
            public void OnUserLoggedOut() {}
        }

        private class DummyEventBus : IEventBus 
        {
            public void Initialize() {}
            public void Publish<T>(T evt) where T : class {}
            public void Subscribe<T>(Action<T> handler) where T : class {}
            public void Unsubscribe<T>(Action<T> handler) where T : class {}
            public void ClearSubscriptions<T>() where T : class {}
            public void ClearAllSubscriptions() {}
        }

        private class DummyLoggingService : ILoggingService
        {
            public event Action<LogEntry> OnLogAdded;
            public void Initialize() {}
            public void Log(string message, LogLevel level = LogLevel.Info, string category = "General") {}
            public void LogInfo(string message, string category = "General") {}
            public void LogWarning(string message, string category = "General") {}
            public void LogError(string message, string category = "General") {}
            public void LogException(Exception exception, string category = "General") {}
            public LogEntry[] GetLogs() => Array.Empty<LogEntry>();
            public LogEntry[] GetLogsByLevel(LogLevel level) => Array.Empty<LogEntry>();
            public LogEntry[] GetLogsByCategory(string category) => Array.Empty<LogEntry>();
            public void ClearLogs() {}
            public string ExportLogs() => "";
            public void SaveLogsToFile(string filePath) {}
            public void SetLogLevel(LogLevel minLevel) {}
            public void EnableCategory(string category) {}
            public void DisableCategory(string category) {}
            public void Dispose() {}
        }

        [Test]
        public void Configure_RegistersExpectedServices()
        {
            // Arrange
            var go = new GameObject("SessionScope");
            var scope = go.AddComponent<TestableSessionLifetimeScope>();
            
            var builder = new ContainerBuilder();
            
            // Register dummy dependencies that would normally come from parent scope
            builder.RegisterInstance<ISaveService>(new DummySaveService());
            builder.RegisterInstance<IEventBus>(new DummyEventBus());
            builder.RegisterInstance<ILoggingService>(new DummyLoggingService());

            // Act
            scope.Configure(builder);
            var container = builder.Build();

            // Assert
            Assert.DoesNotThrow(() => container.Resolve<ICurrencyService>(), "Failed to resolve ICurrencyService");
            Assert.DoesNotThrow(() => container.Resolve<IAudioService>(), "Failed to resolve IAudioService");
            Assert.DoesNotThrow(() => container.Resolve<IInputService>(), "Failed to resolve IInputService");
            Assert.DoesNotThrow(() => container.Resolve<IGameModeService>(), "Failed to resolve IGameModeService");
            Assert.DoesNotThrow(() => container.Resolve<ICharacterService>(), "Failed to resolve ICharacterService");
            Assert.DoesNotThrow(() => container.Resolve<ISkinsService>(), "Failed to resolve ISkinsService");
            Assert.DoesNotThrow(() => container.Resolve<INetworkingServices>(), "Failed to resolve INetworkingServices");
            Assert.DoesNotThrow(() => container.Resolve<IPlayerNetworkManager>(), "Failed to resolve IPlayerNetworkManager");
            Assert.DoesNotThrow(() => container.Resolve<INetworkObjectPool>(), "Failed to resolve INetworkObjectPool");
            Assert.DoesNotThrow(() => container.Resolve<INetworkGameManager>(), "Failed to resolve INetworkGameManager");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}