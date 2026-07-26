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

        public void Update(float deltaTime)
        {
            // Dev: press P to enter 2v2 lobby (Slice 5 replaces with Play button UI)
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.P))
            {
                ServiceLocator.Get<IGameStateMachine>().ChangeState(new RecipeRage.LobbyState(teamSize: 2));
            }
        }
    }
}
