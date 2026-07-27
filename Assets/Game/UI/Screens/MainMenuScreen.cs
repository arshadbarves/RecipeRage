using Playcenter;
using Playcenter.Services;
using Playcenter.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    [UIScreen]
    public sealed class MainMenuScreen : BaseUIScreen
    {
        [SerializeField] private ChefShowcase3D _chefShowcase;

        protected override void OnShow()
        {
            var wallet = ServiceLocator.Get<IWalletService>();
            var trophies = ServiceLocator.Get<ITrophyService>();
            var ui = ServiceLocator.Get<IUIService>();

            var coinLabel = Root.Q<Label>("coin-count");
            var trophyLabel = Root.Q<Label>("trophy-count");
            coinLabel.text = wallet.GetCoins().ToString();
            trophyLabel.text = trophies.Trophies.ToString();

            wallet.OnCoinsChanged += OnCoinsChanged;
            trophies.OnTrophiesChanged += OnTrophiesChanged;

            var playButton = Root.Q<Button>("play-button");
            playButton.clicked += () =>
            {
                // PLAY → Game Mode Select (Ranked/Casual/Practice)
                ui.Show<GameModeScreen>();
            };
            UIAnimation.ScalePulse(playButton);

            Root.Q<Button>("chefs-tab").clicked += () => ui.Show<ChefsScreen>();
            Root.Q<Button>("shop-tab").clicked += () => ui.Show<ShopScreen>();
            Root.Q<Button>("friends-button").clicked += () => ui.Show<FriendsScreen>();
            Root.Q<Button>("settings-button").clicked += () => ui.Show<SettingsScreen>();
            Root.Q<Button>("events-tab").clicked += () => ui.Show<DailyRewardsScreen>();

            // Profile: tap avatar/name → Profile screen
            var profileArea = Root.Q<VisualElement>("profile-area");
            if (profileArea != null)
            {
                profileArea.RegisterCallback<ClickEvent>(e => ui.Show<ProfileScreen>());
            }

            var showcase = Root.Q<VisualElement>("chef-showcase");
            _chefShowcase.Bind(showcase);

            // Daily rewards: open the 7-day calendar
            var dailyButton = Root.Q<Button>("daily-ad-button");
            dailyButton.text = "Daily Rewards";
            dailyButton.clicked += () => ui.Show<DailyRewardsScreen>();
        }

        protected override void OnHide()
        {
            ServiceLocator.Get<IWalletService>().OnCoinsChanged -= OnCoinsChanged;
            ServiceLocator.Get<ITrophyService>().OnTrophiesChanged -= OnTrophiesChanged;
        }

        private void OnCoinsChanged(int coins)
        {
            UIAnimation.CountUp(Root.Q<Label>("coin-count"), int.Parse(Root.Q<Label>("coin-count").text), coins);
        }

        private void OnTrophiesChanged(int trophies)
        {
            Root.Q<Label>("trophy-count").text = trophies.ToString();
        }
    }
}
