using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using KitchenClash.Application;
using KitchenClash.Application.Models;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Persistence;
using NUnit.Framework;
using Playcenter.Services;

namespace RecipeRage.Tests.EditMode.Persistence
{
    /// <summary>
    /// PlayerDataService must persist progress/stats through ISaveService so a
    /// new service instance (app relaunch) sees previously written data.
    /// </summary>
    public class PlayerDataServiceSaveTests
    {
        private sealed class MemSave : ISaveService
        {
            private readonly Dictionary<string, string> _data = new();

            public void RegisterStorageConfig(string key, StorageStrategy strategy, bool encrypt) { }
            public void OnUserLoggedIn() { }
            public void OnUserLoggedOut() { }
            public GameSettingsData GetSettings() => new GameSettingsData();
            public void SaveSettings(GameSettingsData settings) { }
            public void UpdateSettings(Action<GameSettingsData> updateAction) { }
            public SyncStatus GetSyncStatus(string key) => null;
            public UniTask SyncAllCloudDataAsync() => UniTask.CompletedTask;
            public T LoadData<T>(string key) where T : class, new()
            {
                if (_data.TryGetValue(key, out string content) && !string.IsNullOrEmpty(content))
                {
                    return UnityEngine.JsonUtility.FromJson<T>(content);
                }
                return new T();
            }
            public void SaveData<T>(string key, T data) where T : class, new()
            {
                _data[key] = UnityEngine.JsonUtility.ToJson(data);
            }
            public T Load<T>(string key, T defaultValue)
            {
                if (_data.TryGetValue(key, out string content) && !string.IsNullOrEmpty(content))
                {
                    try { return UnityEngine.JsonUtility.FromJson<T>(content); }
                    catch { }
                }
                return defaultValue;
            }
            public void Save(string key, object data)
            {
                _data[key] = UnityEngine.JsonUtility.ToJson(data);
            }
        }

        [Test]
        public void SetPlayerName_ThenNewService_LoadsName()
        {
            var save = new MemSave();
            var a = new PlayerDataService(save);
            a.Initialize();
            a.SetPlayerName("ChefTest");

            var b = new PlayerDataService(save);
            b.Initialize();
            Assert.AreEqual("ChefTest", b.GetStats().PlayerName);
        }

        [Test]
        public void RecordGamePlayed_ThenNewService_LoadsStats()
        {
            var save = new MemSave();
            var a = new PlayerDataService(save);
            a.Initialize();
            a.RecordGamePlayed(won: true, gameModeId: "rush_service", characterId: "chef_a", playTime: 120f, score: 5, xp: 50);

            var b = new PlayerDataService(save);
            b.Initialize();
            Assert.AreEqual(1, b.GetStats().GamesPlayed);
            Assert.AreEqual(1, b.GetStats().GamesWon);
        }

        [Test]
        public void UpgradeCharacter_ThenNewService_LoadsLevel()
        {
            var save = new MemSave();
            var a = new PlayerDataService(save);
            a.Initialize();
            a.UpgradeCharacter("chef_a", 100);

            var b = new PlayerDataService(save);
            b.Initialize();
            Assert.AreEqual(2, b.GetProgress().GetCharacterLevel("chef_a"));
        }

        [Test]
        public void UnlockCharacter_ThenNewService_KeepsUnlock()
        {
            var save = new MemSave();
            var a = new PlayerDataService(save);
            a.Initialize();
            a.UnlockCharacter("chef_b");

            var b = new PlayerDataService(save);
            b.Initialize();
            Assert.Contains("chef_b", b.GetProgress().UnlockedCharacters);
        }
    }
}
