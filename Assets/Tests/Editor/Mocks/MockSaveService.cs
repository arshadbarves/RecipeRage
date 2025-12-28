using System;
using Core.SaveSystem;
using Cysharp.Threading.Tasks;

namespace Tests.Editor.Mocks
{
    public class MockSaveService : ISaveService
    {
        public GameSettingsData Settings = new GameSettingsData();
        public PlayerStatsData Stats = new PlayerStatsData();
        public PlayerProgressData Progress = new PlayerProgressData();
        public bool IsUserLoggedInVal = false;

        public event Action<GameSettingsData> OnSettingsChanged;
        public event Action<PlayerProgressData> OnPlayerProgressChanged;
        public event Action<PlayerStatsData> OnPlayerStatsChanged;

        public void Initialize() { }
        public GameSettingsData GetSettings() => Settings;
        public void SaveSettings(GameSettingsData settings) { Settings = settings; OnSettingsChanged?.Invoke(Settings); }
        public void UpdateSettings(Action<GameSettingsData> updateAction) { updateAction(Settings); OnSettingsChanged?.Invoke(Settings); }
        public void OnUserLoggedIn() { IsUserLoggedInVal = true; }
        public void OnUserLoggedOut() { IsUserLoggedInVal = false; }
        
        public PlayerProgressData GetPlayerProgress() => Progress;
        public void SavePlayerProgress(PlayerProgressData progress) { Progress = progress; OnPlayerProgressChanged?.Invoke(progress); }
        public void UpdatePlayerProgress(Action<PlayerProgressData> updateAction) { updateAction(Progress); OnPlayerProgressChanged?.Invoke(Progress); }
        
        public PlayerStatsData GetPlayerStats() => Stats;
        public void SavePlayerStats(PlayerStatsData stats) { Stats = stats; OnPlayerStatsChanged?.Invoke(Stats); }
        public void UpdatePlayerStats(Action<PlayerStatsData> updateAction) { updateAction(Stats); OnPlayerStatsChanged?.Invoke(Stats); }

        public void DeleteAllData() { }
        public void ClearUserCache() { }
        public SyncStatus GetSyncStatus(string key) => new SyncStatus();
        public UniTask SyncAllCloudDataAsync() => UniTask.CompletedTask;
        public T LoadData<T>(string key) where T : class, new() => new T();
        public void SaveData<T>(string key, T data) where T : class, new() { }
    }
}
