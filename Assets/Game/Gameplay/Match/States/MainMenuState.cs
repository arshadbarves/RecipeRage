using Playcenter;
using Playcenter.Services;

namespace RecipeRage
{
    /// <summary>Published when the game enters the main menu. RecipeRage.UI shows the screen.</summary>
    public readonly struct MainMenuEnteredEvent { }

    /// <summary>
    /// Main menu state. Publishes MainMenuEnteredEvent; the UI layer shows
    /// Login (if signed out) or MainMenu. Gameplay never references screens directly.
    /// </summary>
    public sealed class MainMenuState : IGameState
    {
        public void Enter()
        {
            ServiceLocator.Get<ILoggingService>().Log("[Game] MainMenuState entered");
            ServiceLocator.Get<IEventBus>().Publish(new MainMenuEnteredEvent());
        }

        public void Exit() { }

        public void Update(float deltaTime) { }
    }
}
