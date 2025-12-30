using Core.Bootstrap;
using UI;

namespace Core.State.States
{
    /// <summary>
    /// State for the lobby.
    /// </summary>
    public class LobbyState : BaseState
    {
        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public override void Enter()
        {
            base.Enter();

            // Show the lobby UI
            var uiService = GameBootstrap.Services?.UIService;
            if (uiService != null)
            {
                uiService.ShowScreen(UIScreenType.Lobby, true, false);
            }
        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public override void Exit()
        {
            base.Exit();

            // Hide the lobby UI
            var uiService = GameBootstrap.Services?.UIService;
            if (uiService != null)
            {
                uiService.HideScreen(UIScreenType.Lobby, true);
            }
        }

        /// <summary>
        /// Called every frame to update the state.
        /// </summary>
        public override void Update()
        {
            // Check if all players are ready and the host can start the game
            var services = GameBootstrap.Services;
            var networking = services?.Session?.NetworkingServices;

            if (networking != null &&
                networking.LobbyManager.IsMatchLobbyOwner &&
                networking.LobbyManager.AreAllPlayersReady())
            {
                // Transition to the game state
                if (services != null)
                {
                    services.StateManager.ChangeState(new GameplayState());
                }
            }
        }

        /// <summary>
        /// Called at fixed intervals for physics updates.
        /// </summary>
        public override void FixedUpdate()
        {
            // Lobby physics update logic
        }
    }
}