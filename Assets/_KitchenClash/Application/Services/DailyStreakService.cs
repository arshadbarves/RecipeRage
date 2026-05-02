using KitchenClash.Application;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KitchenClash.Domain;

namespace KitchenClash.Application
{
    public sealed class DailyStreakService : IDailyStreakService
    {
        private readonly IConfigService _cfg;
        private readonly IPlayerDataService _playerData;
        private readonly IEconomyService _economy;
        private readonly IEventBus _eventBus;
        private const string StorageKey = "daily_streak_v1";

        private int _currentDay;
        private DateTime _lastClaimUtc;
        private int _missedDays;

        public DailyStreakService(IConfigService cfg, IPlayerDataService playerData, IEconomyService economy, IEventBus eventBus)
        {
            _cfg = cfg;
            _playerData = playerData;
            _economy = economy;
            _eventBus = eventBus;
        }

        public int CurrentDay => _currentDay;
        public int CycleDays => _cfg.Get("daily_streak_cycle_days", 60);

        public async Task LoadAsync()
        {
            var data = await _playerData.LoadAsync(StorageKey);
            if (string.IsNullOrEmpty(data)) return;
            // Parse stored streak data
            var parts = data.Split('|');
            if (parts.Length >= 3)
            {
                int.TryParse(parts[0], out _currentDay);
                DateTime.TryParse(parts[1], out _lastClaimUtc);
                int.TryParse(parts[2], out _missedDays);
            }
        }

        public bool CanClaim(DateTime utcNow)
        {
            var resetHour = 8; // 08:00 UTC per GDD
            var lastReset = utcNow.Date.AddHours(resetHour);
            if (utcNow < lastReset) lastReset = lastReset.AddDays(-1);

            return _lastClaimUtc < lastReset;
        }

        public DailyStreakReward Claim(DateTime utcNow)
        {
            if (!CanClaim(utcNow)) return null;

            var daysSinceLastClaim = (int)(utcNow - _lastClaimUtc).TotalDays;

            // Miss 2+ days = reset. Miss 1 = forgiven.
            if (daysSinceLastClaim > 2 && _currentDay > 0)
            {
                _currentDay = 0;
                _missedDays = 0;
            }

            _currentDay = (_currentDay % CycleDays) + 1;
            _lastClaimUtc = utcNow;

            var reward = GetRewardForDay(_currentDay);
            RedeemReward(reward);
            return reward;
        }

        private void RedeemReward(DailyStreakReward reward)
        {
            switch (reward.RewardType)
            {
                case "coins":
                    _economy.AddCoins(reward.Amount);
                    break;
                case "gems":
                    _economy.AddGems(reward.Amount);
                    break;
                case "crate_common":
                case "crate_rare":
                case "crate_epic":
                case "crate_legendary":
                case "crate_hypercharge":
                    _eventBus.Publish(new CrateRewardEvent { CrateType = reward.RewardType, Amount = reward.Amount });
                    break;
                case "chef_trial":
                    _eventBus.Publish(new ChefTrialEvent { DurationHours = 24 });
                    break;
                case "bp_xp_token":
                    _eventBus.Publish(new BattlePassXpTokenEvent { Amount = reward.Amount });
                    break;
            }
            _eventBus.Publish(new DailyRewardClaimedEvent { Reward = reward });
        }

        public DailyStreakReward GetRewardPreview(int day)
        {
            int clampedDay = ((day - 1) % CycleDays) + 1;
            return GetRewardForDay(clampedDay);
        }

        public List<DailyStreakReward> GetUpcomingRewards(int fromDay, int count)
        {
            var rewards = new List<DailyStreakReward>(count);
            for (int i = 0; i < count; i++)
            {
                int day = ((fromDay + i - 1) % CycleDays) + 1;
                rewards.Add(GetRewardForDay(day));
            }
            return rewards;
        }

        public async Task SaveAsync()
        {
            var data = $"{_currentDay}|{_lastClaimUtc:O}|{_missedDays}";
            await _playerData.SaveAsync(StorageKey, data);
        }

        private DailyStreakReward GetRewardForDay(int day)
        {
            // GDD reward table
            return day switch
            {
                <= 4 => new DailyStreakReward(day, "coins", 50 + (day * 12), $"Day {day} coins"),
                5 => new DailyStreakReward(day, "gems", 3, "Day 5: 3 Gems + 200 Coins"),
                <= 9 => new DailyStreakReward(day, "coins", 80 + ((day - 6) * 10), $"Day {day} coins"),
                10 => new DailyStreakReward(day, "crate_common", 1, "Common Skin Crate + 150 Coins"),
                <= 18 => new DailyStreakReward(day, "coins", 100 + ((day - 11) * 10), $"Day {day} coins"),
                19 => new DailyStreakReward(day, "crate_rare", 1, "3 Gems + Rare Skin Crate"),
                20 => new DailyStreakReward(day, "chef_trial", 1, "New Chef Trial (24h)"),
                <= 28 => new DailyStreakReward(day, "coins", 200 + ((day - 21) * 12), $"Day {day} coins"),
                29 => new DailyStreakReward(day, "crate_epic", 1, "5 Gems + Epic Skin Crate"),
                30 => new DailyStreakReward(day, "bp_xp_token", 1, "Battle Pass XP Token"),
                <= 44 => new DailyStreakReward(day, "coins", 300 + ((day - 31) * 15), $"Day {day} coins + gems"),
                45 => new DailyStreakReward(day, "crate_legendary", 1, "Legendary Skin Crate"),
                <= 58 => new DailyStreakReward(day, "coins", 500 + ((day - 46) * 23), $"Day {day} coins + gems"),
                59 => new DailyStreakReward(day, "gems", 10, "10 Gems + Epic Skin"),
                60 => new DailyStreakReward(day, "crate_hypercharge", 1, "HYPERCHARGE Skin Crate (cycle resets)"),
                _ => new DailyStreakReward(day, "coins", 100, $"Day {day}")
            };
        }
    }
}
