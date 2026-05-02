using System;
using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Domain;

namespace KitchenClash.Presentation.ViewModels
{
    public sealed class HomeScreenViewModel : ScreenViewModel
    {
        private readonly IMatchService _matchService;
        private readonly IConfigService _cfg;
        private readonly IPlayerDataService _playerData;
        private readonly IEconomyService _economy;
        private readonly ITrophyService _trophyService;
        private readonly IDailyStreakService _dailyStreak;
        private readonly MapRotationCalculator _mapRotation;

        // Player info
        public BindableProperty<string> PlayerName { get; } = new("Player");
        public BindableProperty<int> Trophies { get; } = new(0);
        public BindableProperty<ChefId> SelectedChef { get; } = new(ChefId.Rosa);
        public BindableProperty<int> GemCount { get; } = new(0);
        public BindableProperty<int> DailyStreakDay { get; } = new(0);

        // Map slot carousel
        public BindableProperty<string> CurrentMapName { get; } = new("");
        public BindableProperty<string> CurrentMode { get; } = new("");
        public BindableProperty<string> TimeRemaining { get; } = new("");
        public BindableProperty<int> CurrentSlotIndex { get; } = new(0);
        public BindableProperty<int> TotalSlots { get; } = new(0);

        public HomeScreenViewModel(
            IMatchService matchService,
            IConfigService cfg,
            IPlayerDataService playerData,
            IEconomyService economy,
            ITrophyService trophyService,
            IDailyStreakService dailyStreak,
            MapRotationCalculator mapRotation)
        {
            _matchService = matchService;
            _cfg = cfg;
            _playerData = playerData;
            _economy = economy;
            _trophyService = trophyService;
            _dailyStreak = dailyStreak;
            _mapRotation = mapRotation;
        }

        public override void OnEnter(object param)
        {
            RefreshPlayerInfo();
            RefreshMapSlot();
            RefreshEconomy();
            RefreshStreak();
        }

        public override void OnResume()
        {
            RefreshPlayerInfo();
            RefreshEconomy();
        }

        private void RefreshPlayerInfo()
        {
            var progress = _playerData.GetProgress();
            Trophies.Value = _trophyService.CurrentTrophies;
        }

        private void RefreshMapSlot()
        {
            var queues = _matchService.GetQueues();
            TotalSlots.Value = queues.Count;

            if (queues.Count > 0)
            {
                int idx = Math.Clamp(CurrentSlotIndex.Value, 0, queues.Count - 1);
                CurrentSlotIndex.Value = idx;
                var q = queues[idx];
                CurrentMapName.Value = q.SceneName;
                CurrentMode.Value = $"{q.TeamCount}v{q.PlayersPerTeam} — {q.DisplayName}";

                var remaining = _mapRotation.GetTimeUntilRotationChange();
                TimeRemaining.Value = remaining > TimeSpan.Zero
                    ? FormatTimeRemaining(remaining)
                    : "";
            }
        }

        private void RefreshEconomy()
        {
            GemCount.Value = _economy.Gems;
        }

        private void RefreshStreak()
        {
            DailyStreakDay.Value = _dailyStreak.CurrentDay;
        }

        public void NextSlot()
        {
            var queues = _matchService.GetQueues();
            if (queues.Count == 0) return;
            CurrentSlotIndex.Value = (CurrentSlotIndex.Value + 1) % queues.Count;
            RefreshMapSlot();
        }

        public void PreviousSlot()
        {
            var queues = _matchService.GetQueues();
            if (queues.Count == 0) return;
            CurrentSlotIndex.Value = (CurrentSlotIndex.Value - 1 + queues.Count) % queues.Count;
            RefreshMapSlot();
        }

        private static string FormatTimeRemaining(TimeSpan ts)
        {
            if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours}h {ts.Minutes}m left";
            return $"{ts.Minutes}m left";
        }
    }
}
