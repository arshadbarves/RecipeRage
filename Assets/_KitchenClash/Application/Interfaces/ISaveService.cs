using KitchenClash.Domain;
using System;
using KitchenClash.Application.Models;
using Cysharp.Threading.Tasks;

namespace KitchenClash.Application
{
    public interface ISaveService
    {
        event Action<GameSettingsData> OnSettingsChanged;

        void RegisterStorageConfig(string key, StorageStrategy strategy, bool encrypt);
        void OnUserLoggedIn();
        void OnUserLoggedOut();
        GameSettingsData GetSettings();
        void SaveSettings(GameSettingsData settings);
        void UpdateSettings(Action<GameSettingsData> updateAction);
        SyncStatus GetSyncStatus(string key);
        UniTask SyncAllCloudDataAsync();
        T LoadData<T>(string key) where T : class, new();
        void SaveData<T>(string key, T data) where T : class, new();
        T Load<T>(string key, T defaultValue);
        void Save(string key, object data);
    }
}
