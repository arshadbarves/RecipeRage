using Playcenter;
using Playcenter.Services;
using Playcenter.UI;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    [UIScreen]
    public sealed class ResultsScreen : BaseUIScreen
    {
        private int _lastMatchCoins;

        protected override void OnShow()
        {
            Root.Q<Button>("play-again-button").clicked += () =>
                ServiceLocator.Get<IGameStateMachine>().ChangeState(new LobbyState(teamSize: 2));
            Root.Q<Button>("main-menu-button").clicked += () =>
                ServiceLocator.Get<IUIService>().Show<MainMenuScreen>();

            Root.Q<Button>("double-coins-ad").clicked += () =>
            {
                ServiceLocator.Get<IAdsService>().ShowRewardedAd("results_double_coins", success =>
                {
                    if (success)
                    {
                        // Doubles the match's coin grant — simplest correct behavior:
                        // award the same base again.
                        ServiceLocator.Get<IWalletService>().AddCoins(_lastMatchCoins);
                    }
                });
            };

            UIAnimation.ScaleBounce(Root.Q<Label>("result-title"));
            UIAnimation.StaggerChildren(Root.Q<VisualElement>("rewards-container"), 0.15f);
        }

        public void SetResults(bool won, int teamRecipes, int enemyRecipes, int coinsEarned, int trophyDelta)
        {
            _lastMatchCoins = coinsEarned;
            Root.Q<Label>("result-title").text = won ? "VICTORY!" : "DEFEAT";
            Root.Q<Label>("score-line").text = $"{teamRecipes} vs {enemyRecipes}";
            Root.Q<Label>("trophy-delta").text = $"{(trophyDelta >= 0 ? "+" : "")}{trophyDelta} 🏆";
            Root.Q<Label>("coin-total").text = $"+{coinsEarned} 💰";
        }
    }
}
