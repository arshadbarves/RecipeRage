using Core.Bootstrap;
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
        public override void Enter()
        {
            base.Enter();
            GameLogger.Log("[LoginState] Entered");

            // Show Login Screen
            GameBootstrap.Services.UIService.ShowScreen(UIScreenType.Login);

            // Subscribe to login events
            GameBootstrap.Services.EventBus.Subscribe<Events.LoginSuccessEvent>(OnLoginSuccess);
        }

        public override void Exit()
        {
            base.Exit();
            GameLogger.Log("[LoginState] Exiting");

            // Hide Login Screen
            GameBootstrap.Services.UIService.HideScreen(UIScreenType.Login);

            // Unsubscribe
            GameBootstrap.Services.EventBus.Unsubscribe<Events.LoginSuccessEvent>(OnLoginSuccess);
        }

        private void OnLoginSuccess(Events.LoginSuccessEvent evt)
        {
            GameLogger.Log($"[LoginState] Login successful for user: {evt.UserId}");

            // Transition to BootstrapState to perform post-login checks (config, maintenance, etc.)
            // Or directly to MainMenu if we want to skip checks (but checks are good).
            // Actually, BootstrapState handles the full flow.
            // If we are logging in, we might want to re-run the post-login checks.
            // Let's create a specific PostLoginState or just reuse BootstrapState logic?
            // Reusing BootstrapState might re-show splash.
            // Let's just transition to MainMenu for now, or a "PostLoginState".
            // Given the simplicity, let's go to MainMenu, but ideally we should check for updates again.
            // For now: MainMenu.

            GameBootstrap.Services.StateManager.ChangeState(new MainMenuState());
        }
    }
}
