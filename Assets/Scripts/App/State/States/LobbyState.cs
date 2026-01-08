using Core.Bootstrap;
using UI;
using Modules.Networking;
using Core.UI;
using VContainer;

namespace App.State.States
{
    /// <summary>
    /// State for the lobby.
    /// </summary>
    public class LobbyState : BaseState
    {
        private readonly IUIService _uiService;
        private readonly SessionManager _sessionManager;
        private readonly IGameStateManager _stateManager;

        public LobbyState(
            IUIService uiService, 
            SessionManager sessionManager, 
            IGameStateManager stateManager)
        {
            _uiService = uiService;
            _sessionManager = sessionManager;
            _stateManager = stateManager;
        }

        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public override void Enter()
        {
            base.Enter();

            // Show the lobby UI
            _uiService?.ShowScreen(UIScreenType.Lobby, true, false);
        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public override void Exit()
        {
            base.Exit();

            // Hide the lobby UI
            _uiService?.HideScreen(UIScreenType.Lobby, true);
        }

        /// <summary>
        /// Called every frame to update the state.
        /// </summary>
        public override void Update()
        {
            // Check if all players are ready and the host can start the game
            var sessionContainer = _sessionManager.SessionContainer;
            if (sessionContainer == null) return;

            var networking = sessionContainer.Resolve<INetworkingServices>();

            if (networking != null &&
                networking.LobbyManager.IsMatchLobbyOwner &&
                networking.LobbyManager.AreAllPlayersReady())
            {
                // Transition to the game state
                _stateManager.ChangeState<GameplayState>();
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