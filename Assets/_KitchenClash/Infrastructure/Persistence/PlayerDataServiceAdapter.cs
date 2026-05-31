using KitchenClash.Application.Models;
using KitchenClash.Application;
using System.Threading.Tasks;
using KitchenClash.Domain;

namespace KitchenClash.Infrastructure.Persistence
{
    public class PlayerDataServiceAdapter : IPlayerDataService
    {
        private const string ProgressKey = "player_progress.json";
        private const string StatsKey = "player_stats.json";

        private readonly Application.IStorageProvider _storageProvider;

        private PlayerProgressData _progress;
        private PlayerStatsData _stats;

        public PlayerDataServiceAdapter(Application.IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public void Initialize()
        {
            _progress = new PlayerProgressData();
            _stats = new PlayerStatsData();
            GameLogger.Log("[PlayerDataServiceAdapter] Initialized.");
        }

        public PlayerProgressData GetProgress() => _progress;
        public PlayerStatsData GetStats() => _stats;

        public void SetPlayerName(string name)
        {
            _stats.PlayerName = name;
            _stats.UsernameChangeCount++;
        }

        public void RecordGamePlayed(bool won, string gameModeId, string characterId, float playTime, int score, int xp)
        {
            _stats.RecordGamePlayed(won, gameModeId, characterId, playTime, score);
            _stats.AddExperience(xp);
        }

        public Task<string> LoadAsync(string key)
        {
            return Task.FromResult<string>(null);
        }

        public Task SaveAsync(string key, string data)
        {
            return Task.CompletedTask;
        }
    }
}
