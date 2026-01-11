using Core.Logging;
using Core.Shared.Events;
using Core.UI.Interfaces;
using Gameplay.UI.Features.Auth;

namespace Gameplay.App.State.States
{
    /// <summary>
    /// State for the Login screen.
    /// Handles showing the login UI and waiting for authentication.
    /// </summary>
    public class LoginState : BaseState
    {
        private readonly IUIService _uiService;
        private readonly IEventBus _eventBus;
        private readonly IGameStateManager _stateManager;

        public LoginState(
            IUIService uiService,
            IEventBus eventBus,
            IGameStateManager stateManager)
        {
            _uiService = uiService;
            _eventBus = eventBus;
            _stateManager = stateManager;
        }

        public override void Enter()
        {
            base.Enter();
            GameLogger.Log("[LoginState] Entered - Subscribing to events");

            _uiService.Show<LoginView>();
            _eventBus.Subscribe<LoginSuccessEvent>(OnLoginSuccess);
        }

        public override void Exit()
        {
            base.Exit();
            GameLogger.Log("[LoginState] Exiting - Unsubscribing");

            _uiService.Hide<LoginView>();
            _eventBus.Unsubscribe<LoginSuccessEvent>(OnLoginSuccess);
        }

        private void OnLoginSuccess(LoginSuccessEvent evt)
        {
            GameLogger.Log($"[LoginState] EVENT RECEIVED: Login successful for user: {evt.UserId}");
            _stateManager.ChangeState<SessionLoadingState>();
        }
    }
}