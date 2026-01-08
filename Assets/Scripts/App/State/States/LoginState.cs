using Modules.Shared.Events;
using Modules.Logging;
using Core.UI;
using UI;
using UnityEditor.PackageManager;

namespace App.State.States
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
            // Transition to loading state to fetch profile/currency before showing MainMenu
            _stateManager.ChangeState<SessionLoadingState>();
        }
    }
}
