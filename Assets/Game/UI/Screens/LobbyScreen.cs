using Playcenter;
using Playcenter.UI;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// Chef select happens HERE (Brawl Stars-style), before matchmaking.
    /// Play → chef locks → matchmaking. No separate pre-match screen.
    /// </summary>
    [UIScreen]
    public sealed class LobbyScreen : BaseUIScreen
    {
        private LobbyState _lobbyState;

        protected override void OnShow()
        {
            var progression = ServiceLocator.Get<IChefProgressionService>();
            var catalog = ServiceLocator.Get<IChefCatalog>();
            var grid = Root.Q<ScrollView>("chef-grid");
            grid.Clear();

            foreach (var chef in catalog.All)
            {
                var unlocked = progression.IsUnlocked(chef.Id);
                var card = ChefCard.Build(chef, progression.GetLevel(chef.Id), unlocked, () =>
                {
                    if (unlocked)
                    {
                        progression.SelectChef(chef.Id);
                        RefreshSelection();
                    }
                });
                grid.Add(card);
            }

            RefreshSelection();

            Root.Q<Button>("play-button").clicked += () =>
            {
                _lobbyState = new LobbyState(teamSize: 2);
                ServiceLocator.Get<IGameStateMachine>().ChangeState(_lobbyState);
                _lobbyState.OnPlayPressed();
                ServiceLocator.Get<IUIService>().Show<MatchmakingScreen>();
            };
        }

        private void RefreshSelection()
        {
            var selected = ServiceLocator.Get<IChefProgressionService>().GetSelectedChef();
            var nameLabel = Root.Q<Label>("selected-chef-name");
            nameLabel.text = ServiceLocator.Get<IChefCatalog>().Get(selected)?.DisplayName ?? string.Empty;
        }
    }
}
