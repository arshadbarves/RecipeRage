using Playcenter;
using Playcenter.Services;
using Playcenter.UI;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// 7-day login reward calendar. Shows all 7 days, highlights today,
    /// marks claimed days, big reward on day 7. Claim button collects today's reward.
    /// </summary>
    [UIScreen]
    public sealed class DailyRewardsScreen : BaseUIScreen
    {
        private DailyRewardsService _service;

        protected override void OnShow()
        {
            _service = ServiceLocator.Get<DailyRewardsService>();
            BuildGrid();
            RefreshClaimButton();

            Root.Q<Button>("claim-button").clicked += OnClaim;
            Root.Q<Button>("back-button").clicked += () =>
                ServiceLocator.Get<IUIService>().Show<MainMenuScreen>();
        }

        private void BuildGrid()
        {
            var grid = Root.Q<VisualElement>("rewards-grid");
            grid.Clear();

            var today = _service.CurrentDay;
            for (int i = 0; i < DailyRewardsService.Rewards.Length; i++)
            {
                var reward = DailyRewardsService.Rewards[i];
                var card = new VisualElement();
                card.AddToClassList("card");
                card.AddToClassList("center");
                card.style.width = 110;
                card.style.height = 130;

                if (i < today)
                {
                    card.AddToClassList("card--recessed"); // claimed
                }
                else if (i == today)
                {
                    card.style.borderBottomColor = new StyleColor(new UnityEngine.Color(0.79f, 0.64f, 0.15f)); // today highlight
                    card.style.borderBottomWidth = 3;
                }

                var dayLabel = new Label($"Day {reward.Day}");
                dayLabel.AddToClassList("text-soft");
                card.Add(dayLabel);

                var coinLabel = new Label($"+{reward.Coins}");
                coinLabel.AddToClassList("heading");
                coinLabel.style.fontSize = 22;
                coinLabel.style.color = new StyleColor(new UnityEngine.Color(0.79f, 0.64f, 0.15f));
                card.Add(coinLabel);

                if (reward.IsBigReward)
                {
                    var big = new Label("BIG");
                    big.AddToClassList("badge");
                    card.Add(big);
                }

                grid.Add(card);
            }
        }

        private void RefreshClaimButton()
        {
            var button = Root.Q<Button>("claim-button");
            var reward = _service.GetTodayReward();
            button.text = _service.CanClaimToday() ? $"Claim +{reward.Coins} Coins" : "Claimed — Come back tomorrow!";
            button.SetEnabled(_service.CanClaimToday());
        }

        private void OnClaim()
        {
            if (_service.TryClaim())
            {
                BuildGrid();
                RefreshClaimButton();
                UIAnimation.ScaleBounce(Root.Q<VisualElement>("rewards-grid"));
            }
        }
    }
}
