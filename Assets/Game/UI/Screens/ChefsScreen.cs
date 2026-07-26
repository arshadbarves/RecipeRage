using Playcenter;
using Playcenter.Services;
using Playcenter.UI;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    [UIScreen]
    public sealed class ChefsScreen : BaseUIScreen
    {
        protected override void OnShow()
        {
            var progression = ServiceLocator.Get<IChefProgressionService>();
            var catalog = ServiceLocator.Get<IChefCatalog>();
            var grid = Root.Q<ScrollView>("chef-grid");
            grid.Clear();

            foreach (var chef in catalog.All)
            {
                var unlocked = progression.IsUnlocked(chef.Id);
                grid.Add(ChefCard.Build(chef, progression.GetLevel(chef.Id), unlocked, () => ShowDetail(chef)));
            }

            Root.Q<Button>("back-button").clicked += () =>
                ServiceLocator.Get<IUIService>().Show<MainMenuScreen>();
        }

        private void ShowDetail(ChefDefinition chef)
        {
            var progression = ServiceLocator.Get<IChefProgressionService>();
            var wallet = ServiceLocator.Get<IWalletService>();

            Root.Q<Label>("detail-name").text = chef.DisplayName;
            Root.Q<Label>("detail-level").text = $"Level {progression.GetLevel(chef.Id)}/10";

            var actionButton = Root.Q<Button>("detail-action");
            var cost = progression.IsUnlocked(chef.Id)
                ? progression.GetUpgradeCost(chef.Id)
                : chef.UnlockCost;
            actionButton.text = progression.IsUnlocked(chef.Id)
                ? $"Upgrade: {cost}c"
                : $"Unlock: {cost}c";
            actionButton.SetEnabled(wallet.GetCoins() >= cost);

            actionButton.clicked += () =>
            {
                var success = progression.IsUnlocked(chef.Id)
                    ? progression.TryUpgrade(chef.Id)
                    : progression.TryUnlock(chef.Id);
                if (success)
                {
                    ShowDetail(chef); // refresh
                }
            };
        }
    }
}
