using System;
using KitchenClash.Application;
using KitchenClash.Application.Models;
using KitchenClash.Domain;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace KitchenClash.Infrastructure.Persistence
{
    /// <summary>
    /// Lightweight ISaveService backed by PlayerPrefs. Used when full SaveService
    /// (which requires StorageProviderFactory / cloud providers) is not yet wired.
    /// </summary>
    public sealed class LocalSaveService : ISaveService
    {
        private GameSettingsData _settings;

        public event Action<GameSettingsData> OnSettingsChanged;

        public LocalSaveService()
        {
            _settings = Load("settings", new GameSettingsData());
            GameLogger.Log("[LocalSaveService] Initialized");
        }

        public void RegisterStorageConfig(string key, StorageStrategy strategy, bool encrypt) { }
        public void OnUserLoggedIn() { }
        public void OnUserLoggedOut() { }

        public GameSettingsData GetSettings() => _settings ??= new GameSettingsData();

        public void SaveSettings(GameSettingsData settings)
        {
            _settings = settings;
            Save("settings", settings);
            OnSettingsChanged?.Invoke(settings);
        }

        public void UpdateSettings(Action<GameSettingsData> updateAction)
        {
            var s = GetSettings();
            updateAction?.Invoke(s);
            SaveSettings(s);
        }

        public SyncStatus GetSyncStatus(string key) => new SyncStatus();
        public UniTask SyncAllCloudDataAsync() => UniTask.CompletedTask;

        public T LoadData<T>(string key) where T : class, new()
        {
            string json = PlayerPrefs.GetString(key, null);
            if (!string.IsNullOrEmpty(json))
            {
                try { return JsonUtility.FromJson<T>(json); }
                catch { }
            }
            return new T();
        }

        public void SaveData<T>(string key, T data) where T : class, new()
        {
            Save(key, data);
        }

        public T Load<T>(string key, T defaultValue)
        {
            string json = PlayerPrefs.GetString(key, null);
            if (!string.IsNullOrEmpty(json))
            {
                try { return JsonUtility.FromJson<T>(json); }
                catch { }
            }
            return defaultValue;
        }

        public void Save(string key, object data)
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }
    }
}
