using System.Threading.Tasks;
using Brawlers.Stats;
using Core;
using GameSystem.Player;

namespace GameSystem.Progression
{
    public enum StatType
    {
        Trophies,
        Experience,
        Level,
        Coins,
        Tokens
    }

    public class ProgressionSystem : IGameSystem
    {
        private PlayerData _playerData;

        public async Task InitializeAsync()
        {
            _playerData = GameManager.Instance.GetSystem<PlayerSystem>().PlayerData;
            await LoadPlayerDataAsync();
        }

        public void Update()
        {
            // Game progression logic for updating experience, level, etc.
        }

        public Task CleanupAsync()
        {
            // Cleanup resources when the system shuts down
            return Task.CompletedTask;
        }

        public (bool success, string message) UnlockBrawler(string brawlerName)
        {
            BrawlerData brawlerData = BrawlerDataSystem.GetBrawlerData(brawlerName);
            if (brawlerData == null) return (false, "Brawler data not found");

            if (_playerData.IsBrawlerUnlocked(brawlerName))
            {
                return (false, "Brawler is already unlocked");
            }

            if (_playerData.Coins < brawlerData.UnlockCost)
            {
                return (false, "Not enough coins to unlock the brawler");
            }

            _playerData.UnlockBrawler(brawlerName);
            _playerData.AddCoins(-brawlerData.UnlockCost); // Deduct coins

            SavePlayerDataAsync().ConfigureAwait(false); // Save data asynchronously

            return (true, "Brawler unlocked successfully");
        }

        public (bool success, string message) UpgradeBrawler(string brawlerName)
        {
            BrawlerData brawlerData = BrawlerDataSystem.GetBrawlerData(brawlerName);
            if (brawlerData == null) return (false, "Brawler data not found");

            if (!_playerData.IsBrawlerUnlocked(brawlerName))
            {
                return (false, "Brawler is not unlocked");
            }

            int currentLevel = _playerData.GetBrawlerLevel(brawlerName);
            if (!BrawlerDataSystem.CanUpgradeBrawler(currentLevel, _playerData.Tokens, _playerData.Coins))
            {
                return (false, "Cannot upgrade brawler: Insufficient tokens or coins");
            }

            _playerData.SetBrawlerLevel(brawlerName, currentLevel + 1);
            _playerData.AddTokens(-BrawlerDataSystem.GetNextBrawlerUpgrade(currentLevel)
                .upgradeTokenCost); // Deduct tokens
            _playerData.AddCoins(-BrawlerDataSystem.GetNextBrawlerUpgrade(currentLevel)
                .upgradeCoinCost); // Deduct coins

            SavePlayerDataAsync().ConfigureAwait(false); // Save data asynchronously

            return (true, "Brawler upgraded successfully");
        }

        private async Task LoadPlayerDataAsync()
        {
            // Load player data from storage (e.g., online storage)
        }

        private async Task SavePlayerDataAsync()
        {
            // Save player data to storage (e.g., online storage)
        }

        public void UpdatePlayerProgression(StatType statType, int value)
        {
            _playerData.UpdateStat(statType, value);
        }
    }
}