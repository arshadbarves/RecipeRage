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
                ServiceLocator.Get<IGameStateMachine>().ChangeState(new LobbyState(teamSize: 2));
            };
            UIAnimation.ScalePulse(playButton);

            Root.Q<Button>("chefs-tab").clicked += () => ui.Show<ChefsScreen>();
            Root.Q<Button>("shop-tab").clicked += () => ui.Show<ShopScreen>();
            Root.Q<Button>("friends-button").clicked += () => ui.Show<FriendsScreen>();

            var showcase = Root.Q<VisualElement>("chef-showcase");
            _chefShowcase.Bind(showcase);

            // Daily reward stub: watch ad for 100 coins (3/day limit tracked per spec)
            var adButton = Root.Q<Button>("daily-ad-button");
            adButton.clicked += () =>
            {
                ServiceLocator.Get<IAdsService>().ShowRewardedAd("daily_coins", success =>
                {
                    if (success)
                    {
                        wallet.AddCoins(100);
                    }
                });
            };
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
