using System;
using Gameplay.Persistence.Data;
using Core.Persistence;
using Core.Logging;

namespace Gameplay.Persistence
{
    /// <summary>
    /// Service managing Player Progression (Unlocks, Levels) and Stats.
    /// Uses ISaveService for all I/O.
    /// </summary>
    public class PlayerDataService
    {
        private const string ProgressKey = "player_progress.json";
        private const string StatsKey = "player_stats.json";

        private readonly ISaveService _saveService;

        private PlayerProgressData _progress;
        private PlayerStatsData _stats;

        public event Action<PlayerProgressData> OnProgressChanged;
        public event Action<PlayerStatsData> OnStatsChanged;
        public event Action OnLevelUp;

        public PlayerDataService(ISaveService saveService)
        {
            _saveService = saveService;
        }

        public void Initialize()
        {
            _progress = _saveService.LoadData<PlayerProgressData>(ProgressKey);
            _stats = _saveService.LoadData<PlayerStatsData>(StatsKey);

            if (_progress == null) _progress = new PlayerProgressData();
            if (_stats == null) _stats = new PlayerStatsData();

            GameLogger.Log("[PlayerDataService] Initialized.");
        }

        public PlayerProgressData GetProgress() => _progress;
        public PlayerStatsData GetStats() => _stats;

        public void UnlockCharacter(string id)
        {
            _progress.UnlockCharacter(id);
            SaveProgress();
            OnProgressChanged?.Invoke(_progress);
        }

        public void UnlockMap(string id)
        {
            _progress.UnlockMap(id);
            SaveProgress();
            OnProgressChanged?.Invoke(_progress);
        }

        public void UnlockCosmetic(string id)
        {
            _progress.UnlockCosmetic(id);
            SaveProgress();
            OnProgressChanged?.Invoke(_progress);
        }

        public void SetHighScore(string gameModeId, int score)
        {
            if (_progress.UpdateHighScore(gameModeId, score))
            {
                SaveProgress();
                OnProgressChanged?.Invoke(_progress);
            }
        }

        public void SetBestTime(string gameModeId, float time)
        {
            if (_progress.UpdateBestTime(gameModeId, time))
            {
                SaveProgress();
                OnProgressChanged?.Invoke(_progress);
            }
        }

        public void RecordGamePlayed(bool won, string gameModeId, string characterId, float playTime, int score, int xp)
        {
            _stats.RecordGamePlayed(won, gameModeId, characterId, playTime, score);
            bool leveledUp = _stats.AddExperience(xp);

            SaveStats();
            OnStatsChanged?.Invoke(_stats);

            if (leveledUp) OnLevelUp?.Invoke();
        }

        public void SetPlayerName(string name)
        {
            _stats.PlayerName = name;
            _stats.UsernameChangeCount++;
            SaveStats();
            OnStatsChanged?.Invoke(_stats);
        }

        public void CompleteTutorial()
        {
            _progress.TutorialCompleted = true;
            SaveProgress();
        }

        private void SaveProgress() => _saveService.SaveData(ProgressKey, _progress);
        private void SaveStats() => _saveService.SaveData(StatsKey, _stats);
    }
}
