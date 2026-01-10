using Core.UI.Interfaces;
using Core.Networking;
using Core.Session;
using VContainer;

namespace Gameplay.App.State.States
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

            // TODO: LobbyScreen doesn't exist yet - implement when needed
            // _uiService?.Show<LobbyScreen>(true, false);
        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public override void Exit()
        {
            base.Exit();

            // TODO: LobbyScreen doesn't exist yet
            // _uiService?.Hide<LobbyScreen>(true);
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