using Playcenter;
using Playcenter.UI;
using RecipeRage.Net;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    [UIScreen]
    public sealed class MatchmakingScreen : BaseUIScreen
    {
        protected override void OnShow()
        {
            var lobby = ServiceLocator.Get<Playcenter.Net.ILobbyService>();
            var label = Root.Q<Label>("players-found");
            lobby.OnPlayersChanged += count =>
                label.text = $"Players found: {count}/{lobby.MaxPlayers}";

            Root.Q<Button>("cancel-button").clicked += () =>
            {
                ServiceLocator.Get<MatchmakingController>().Cancel();
                ServiceLocator.Get<IUIService>().Show<LobbyScreen>();
            };

            UIAnimation.ScalePulse(Root.Q<VisualElement>("matchmaking-icon"));
        }
    }
}
