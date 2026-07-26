using Playcenter;

namespace RecipeRage
{
    /// <summary>
    /// Placeholder main menu state — real UI lands in Slice 5. Logs entry so the
    /// boot → gameplay handoff is verifiable in the console today.
    /// </summary>
    public sealed class MainMenuState : IGameState
    {
        public void Enter()
        {
            ServiceLocator.Get<ILoggingService>().Log("[Game] MainMenuState entered");
        }

        public void Exit() { }

        public void Update(float deltaTime) { }
    }
}
