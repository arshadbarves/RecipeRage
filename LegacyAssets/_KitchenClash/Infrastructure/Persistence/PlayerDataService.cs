using KitchenClash.Application;
using KitchenClash.Application.Models;
using System.Threading.Tasks;

namespace KitchenClash.Infrastructure.Persistence
{
    /// <summary>
    /// Player progress/stats backed by ISaveService. Progress and stats
    /// round-trip through SaveData/LoadData so a relaunched session restores
    /// prior state (local, or cloud via registered storage strategy).
    /// </summary>
    public class PlayerDataService : IPlayerDataService
    {
        private const string ProgressKey = "player_progress.json";
        private const string StatsKey = "player_stats.json";

        private readonly ISaveService _saveService;

        private PlayerProgressData _progress;
        private PlayerStatsData _stats;

        public PlayerDataService(ISaveService saveService)
        {
            _saveService = saveService;
        }

        public void Initialize()
        {
            _progress = _saveService.LoadData<PlayerProgressData>(ProgressKey) ?? new PlayerProgressData();
            _stats = _saveService.LoadData<PlayerStatsData>(StatsKey) ?? new PlayerStatsData();
        }

        public PlayerProgressData GetProgress() => _progress;
        public PlayerStatsData GetStats() => _stats;

        public void SetPlayerName(string name)
        {
            _stats.PlayerName = name;
            _stats.UsernameChangeCount++;
            PersistStats();
        }

        public void RecordGamePlayed(bool won, string gameModeId, string characterId, float playTime, int score, int xp)
        {
            _stats.RecordGamePlayed(won, gameModeId, characterId, playTime, score);
            _stats.AddExperience(xp);
            PersistStats();
        }

        public int GetCharacterLevel(string characterId)
        {
            return _progress?.GetCharacterLevel(characterId) ?? 1;
        }

        public bool UpgradeCharacter(string characterId, int cost)
        {
            if (_progress == null)
            {
                return false;
            }

            int currentLevel = GetCharacterLevel(characterId);
            _progress.SetCharacterLevel(characterId, currentLevel + 1);
            PersistProgress();
            return true;
        }

        public void UnlockCharacter(string characterId)
        {
            _progress?.UnlockCharacter(characterId);
            PersistProgress();
        }

        public Task<string> LoadAsync(string key)
        {
            return Task.FromResult(_saveService.Load<string>(key, null));
        }

        public Task SaveAsync(string key, string data)
        {
            _saveService.Save(key, data);
            return Task.CompletedTask;
        }

        private void PersistStats() => _saveService.SaveData(StatsKey, _stats);
        private void PersistProgress() => _saveService.SaveData(ProgressKey, _progress);
    }
}
