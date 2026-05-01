using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Presentation.Common;
using KitchenClash.Presentation.ViewModels;
using UnityEngine.UIElements;
using VContainer;

namespace KitchenClash.Presentation.Screens
{
    [UIScreen(UIScreenCategory.Popup, "Popups/DailyStreakTemplate")]
    public class DailyStreakScreen : BaseUIScreen
    {
        [Inject] private DailyStreakViewModel _viewModel;
        [Inject] private IEventBus _eventBus;

        private Label _streakDayLabel;
        private Label _rewardDescLabel;
        private Label _rewardAmountLabel;
        private Button _claimButton;
        private VisualElement _upcomingContainer;

        protected override void OnInitialize()
        {
            QueryElements();
            SetupButtons();
            _eventBus?.Subscribe<DailyRewardClaimedEvent>(OnRewardClaimed);
        }

        private void QueryElements()
        {
            _streakDayLabel = GetElement<Label>("streak-day");
            _rewardDescLabel = GetElement<Label>("reward-description");
            _rewardAmountLabel = GetElement<Label>("reward-amount");
            _claimButton = GetElement<Button>("claim-button");
            _upcomingContainer = GetElement<VisualElement>("upcoming-rewards");
        }

        private void SetupButtons()
        {
            _claimButton?.RegisterCallback<ClickEvent>(_ => OnClaimClicked());
            GetElement<Button>("close-button")?.RegisterCallback<ClickEvent>(_ => Hide());
        }

        protected override void OnShow()
        {
            _viewModel.Initialize();
            BindViewModel();
        }

        private void BindViewModel()
        {
            UpdateStreakDay(_viewModel.CurrentDay.Value);
            UpdateTodayReward(_viewModel.TodayReward.Value);
            UpdateCanClaim(_viewModel.CanClaim.Value);
            UpdateUpcomingRewards();

            _viewModel.CurrentDay.OnValueChanged += UpdateStreakDay;
            _viewModel.TodayReward.OnValueChanged += UpdateTodayReward;
            _viewModel.CanClaim.OnValueChanged += UpdateCanClaim;
        }

        private void UpdateStreakDay(int day)
        {
            if (_streakDayLabel != null)
                _streakDayLabel.text = $"Day {day}";
        }

        private void UpdateTodayReward(DailyStreakReward reward)
        {
            if (reward == null) return;
            if (_rewardDescLabel != null)
                _rewardDescLabel.text = reward.Description;
            if (_rewardAmountLabel != null)
                _rewardAmountLabel.text = reward.RewardType == "coins" || reward.RewardType == "gems"
                    ? $"{reward.Amount} {reward.RewardType}"
                    : reward.Description;
        }

        private void UpdateCanClaim(bool canClaim)
        {
            if (_claimButton != null)
            {
                _claimButton.SetEnabled(canClaim);
                _claimButton.text = canClaim ? "CLAIM" : "CLAIMED";
            }
        }

        private void UpdateUpcomingRewards()
        {
            if (_upcomingContainer == null) return;
            _upcomingContainer.Clear();

            var upcoming = _viewModel.UpcomingRewards.Value;
            if (upcoming == null) return;

            foreach (var reward in upcoming)
            {
                var row = new VisualElement();
                row.AddToClassList("upcoming-reward-row");

                var dayLabel = new Label($"Day {reward.Day}");
                dayLabel.AddToClassList("upcoming-day");

                var descLabel = new Label(reward.Description);
                descLabel.AddToClassList("upcoming-desc");

                row.Add(dayLabel);
                row.Add(descLabel);
                _upcomingContainer.Add(row);
            }
        }

        private void OnClaimClicked()
        {
            if (_viewModel.ClaimReward())
            {
                UpdateUpcomingRewards();
                UIService?.ShowNotification("Daily reward claimed!", NotificationType.Success);
            }
        }

        private void OnRewardClaimed(DailyRewardClaimedEvent evt)
        {
            // Auto-dismiss after a short delay
            Container?.schedule.Execute(() => Hide()).StartingIn(1500);
        }

        protected override void OnDispose()
        {
            _eventBus?.Unsubscribe<DailyRewardClaimedEvent>(OnRewardClaimed);

            if (_viewModel != null)
            {
                _viewModel.CurrentDay.OnValueChanged -= UpdateStreakDay;
                _viewModel.TodayReward.OnValueChanged -= UpdateTodayReward;
                _viewModel.CanClaim.OnValueChanged -= UpdateCanClaim;
            }
        }
    }
}
