using Playcenter;
using Playcenter.Services;
using Playcenter.UI;

namespace RecipeRage.UI
{
    /// <summary>
    /// Shows the correct screen when the game enters the main menu:
    /// Login if signed out, MainMenu if signed in. Lives in RecipeRage.UI
    /// (knows both gameplay events and screens).
    /// </summary>
    public sealed class MainMenuPresenter
    {
        public void Initialize(IEventBus eventBus, IUIService ui, IAuthService auth)
        {
            eventBus.Subscribe<MainMenuEnteredEvent>(e =>
            {
                if (auth.IsSignedIn)
                {
                    ui.Show<MainMenuScreen>();
                }
                else
                {
                    ui.Show<LoginScreen>();
                }
            });

            eventBus.Subscribe<Net.MatchHudRequestedEvent>(e =>
            {
                ui.Show<HUDScreen>();
            });
        }
    }
}
