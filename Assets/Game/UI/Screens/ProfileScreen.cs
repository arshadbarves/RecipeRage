using Playcenter;
using Playcenter.Services;
using Playcenter.UI;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// Player profile: avatar, name/ID, trophy/coin/win stats, chef collection summary.
    /// </summary>
    [UIScreen]
    public sealed class ProfileScreen : BaseUIScreen
    {
        protected override void OnShow()
        {
            var auth = ServiceLocator.Get<IAuthService>();
            var wallet = ServiceLocator.Get<IWalletService>();
            var trophies = ServiceLocator.Get<ITrophyService>();
            var progression = ServiceLocator.Get<IChefProgressionService>();
            var catalog = ServiceLocator.Get<IChefCatalog>();

            Root.Q<Label>("profile-name").text = string.IsNullOrEmpty(auth.DisplayName) ? "Guest Chef" : auth.DisplayName;
            Root.Q<Label>("profile-id").text = $"ID: {auth.UserId}";
            Root.Q<Label>("stat-trophies").text = trophies.Trophies.ToString();
            Root.Q<Label>("stat-coins").text = wallet.GetCoins().ToString();
            Root.Q<Label>("stat-wins").text = "0"; // wins tracking lands with match history

            // Chef collection summary
            var summary = Root.Q<VisualElement>("chef-summary");
            summary.Clear();
            foreach (var chef in catalog.All)
            {
                var unlocked = progression.IsUnlocked(chef.Id);
                summary.Add(ChefCard.Build(chef, progression.GetLevel(chef.Id), unlocked, () => { }));
            }

            Root.Q<Button>("back-button").clicked += () =>
                ServiceLocator.Get<IUIService>().Show<MainMenuScreen>();
        }
    }
}
