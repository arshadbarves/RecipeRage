using Playcenter;
using Playcenter.UI;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// Game mode select (Brawl Stars-style cards). Ranked / Casual / Practice.
    /// Tapping a card goes to the Lobby (chef select) with that mode's team size.
    /// </summary>
    [UIScreen]
    public sealed class GameModeScreen : BaseUIScreen
    {
        protected override void OnShow()
        {
            Root.Q<Button>("back-button").clicked += () =>
                ServiceLocator.Get<IUIService>().Show<MainMenuScreen>();

            BindModeCard("ranked-card", teamSize: 2, isPractice: false);
            BindModeCard("casual-card", teamSize: 3, isPractice: false);
            BindModeCard("practice-card", teamSize: 2, isPractice: true);
        }

        private void BindModeCard(string cardName, int teamSize, bool isPractice)
        {
            var card = Root.Q<VisualElement>(cardName);
            card.RegisterCallback<ClickEvent>(e =>
            {
                // Practice mode skips matchmaking (solo vs bots); others go to lobby
                ServiceLocator.Get<IGameStateMachine>().ChangeState(new LobbyState(teamSize));
                ServiceLocator.Get<IUIService>().Show<LobbyScreen>();
            });
        }
    }
}
