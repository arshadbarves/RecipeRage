using Core.Bootstrap;
using Core.Events;
using Core.Logging;
using UI;

namespace Core.State.States
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
        private readonly ServiceContainer _serviceContainer;

        public LoginState(
            IUIService uiService,
            IEventBus eventBus,
            IGameStateManager stateManager,
            ServiceContainer serviceContainer)
        {
            _uiService = uiService;
            _eventBus = eventBus;
            _stateManager = stateManager;
            _serviceContainer = serviceContainer;
        }

        public override void Enter()
        {
            base.Enter();
            GameLogger.Log("[LoginState] Entered - Subscribing to events");

            // Show Login Screen
            _uiService.ShowScreen(UIScreenType.Login);

            // Subscribe to login events
            _eventBus.Subscribe<Events.LoginSuccessEvent>(OnLoginSuccess);
        }

        public override void Exit()
        {
            base.Exit();
            GameLogger.Log("[LoginState] Exiting - Unsubscribing");

            // Hide Login Screen
            _uiService.HideScreen(UIScreenType.Login);

            // Unsubscribe
            _eventBus.Unsubscribe<Events.LoginSuccessEvent>(OnLoginSuccess);
        }

        private void OnLoginSuccess(Events.LoginSuccessEvent evt)
        {
            GameLogger.Log($"[LoginState] EVENT RECEIVED: Login successful for user: {evt.UserId}");

            // Session is created internally by AuthenticationService after successful auth
            _stateManager.ChangeState(new MainMenuState());
        }
    }
}
