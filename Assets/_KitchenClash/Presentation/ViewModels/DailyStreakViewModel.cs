using System;
using System.Collections.Generic;
using KitchenClash.Domain;
using KitchenClash.Presentation.Common;
using VContainer;

namespace KitchenClash.Presentation.ViewModels
{
    public class DailyStreakViewModel : BaseViewModel
    {
        private readonly IDailyStreakService _streakService;
        private readonly IEconomyService _economyService;
        private readonly IEventBus _eventBus;

        public BindableProperty<int> CurrentDay { get; } = new BindableProperty<int>(0);
        public BindableProperty<bool> CanClaim { get; } = new BindableProperty<bool>(false);
        public BindableProperty<DailyStreakReward> TodayReward { get; } = new BindableProperty<DailyStreakReward>();
        public BindableProperty<List<DailyStreakReward>> UpcomingRewards { get; } = new BindableProperty<List<DailyStreakReward>>();

        [Inject]
        public DailyStreakViewModel(IDailyStreakService streakService, IEconomyService economyService, IEventBus eventBus)
        {
            _streakService = streakService;
            _economyService = economyService;
            _eventBus = eventBus;
        }

        public override void Initialize()
        {
            base.Initialize();
            RefreshState();
        }

        public void RefreshState()
        {
            var now = DateTime.UtcNow;
            int nextDay = (_streakService.CurrentDay % _streakService.CycleDays) + 1;

            CurrentDay.Value = _streakService.CurrentDay;
            CanClaim.Value = _streakService.CanClaim(now);
            TodayReward.Value = _streakService.GetRewardPreview(nextDay);
            UpcomingRewards.Value = _streakService.GetUpcomingRewards(nextDay + 1, 7);
        }

        public bool ClaimReward()
        {
            var now = DateTime.UtcNow;
            var reward = _streakService.Claim(now);
            if (reward == null) return false;

            DispenseReward(reward);
            _streakService.SaveAsync();

            _eventBus.Publish(new DailyRewardClaimedEvent { Reward = reward });
            RefreshState();
            return true;
        }

        private void DispenseReward(DailyStreakReward reward)
        {
            switch (reward.RewardType)
            {
                case "coins":
                    _economyService.AddCoins(reward.Amount);
                    break;
                case "gems":
                    _economyService.AddGems(reward.Amount);
                    break;
                // Crates, trials, tokens — economy doesn't handle these yet;
                // AddCoins as fallback bonus where description mentions coins
                case "crate_common":
                    _economyService.AddCoins(150);
                    break;
                case "crate_rare":
                    _economyService.AddGems(3);
                    break;
                case "crate_epic":
                    _economyService.AddGems(5);
                    break;
                // Other reward types (bp_xp_token, chef_trial, crate_legendary, crate_hypercharge)
                // will be handled when their systems are implemented
            }
        }
    }
}
