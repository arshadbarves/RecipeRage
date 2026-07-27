using System;
using Playcenter.Services;

namespace RecipeRage
{
    /// <summary>
    /// 7-day login reward calendar (Brawl Stars-style retention hook).
    /// Tracks current day + last claim date; resets if a day is missed.
    /// </summary>
    public sealed class DailyRewardsService
    {
        private const string DayKey = "daily_reward_day";
        private const string LastClaimKey = "daily_reward_last_claim";

        public sealed class Reward
        {
            public int Day;
            public int Coins;
            public bool IsBigReward;
        }

        public static readonly Reward[] Rewards =
        {
            new Reward { Day = 1, Coins = 50 },
            new Reward { Day = 2, Coins = 100 },
            new Reward { Day = 3, Coins = 150 },
            new Reward { Day = 4, Coins = 200 },
            new Reward { Day = 5, Coins = 250 },
            new Reward { Day = 6, Coins = 300 },
            new Reward { Day = 7, Coins = 500, IsBigReward = true },
        };

        private readonly ISaveService _save;
        private readonly IWalletService _wallet;
        private readonly IAnalyticsService _analytics;

        public DailyRewardsService(ISaveService save, IWalletService wallet, IAnalyticsService analytics)
        {
            _save = save;
            _wallet = wallet;
            _analytics = analytics;
        }

        public int CurrentDay => _save.Load(DayKey, 0);

        public bool CanClaimToday()
        {
            var lastClaim = _save.Load(LastClaimKey, string.Empty);
            if (string.IsNullOrEmpty(lastClaim))
            {
                return true;
            }
            return DateTime.TryParse(lastClaim, out var last)
                && last.Date < DateTime.UtcNow.Date;
        }

        public Reward GetTodayReward()
        {
            var day = Math.Min(CurrentDay, Rewards.Length - 1);
            return Rewards[day];
        }

        public bool TryClaim()
        {
            if (!CanClaimToday())
            {
                return false;
            }

            var reward = GetTodayReward();
            _wallet.AddCoins(reward.Coins);

            _save.Save(DayKey, (CurrentDay + 1) % Rewards.Length);
            _save.Save(LastClaimKey, DateTime.UtcNow.ToString("o"));

            _analytics.TrackEvent("daily_reward_claimed", new System.Collections.Generic.Dictionary<string, object>
            {
                { "day", reward.Day },
                { "coins", reward.Coins }
            });
            return true;
        }
    }
}
