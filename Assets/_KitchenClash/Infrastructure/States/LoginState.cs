using System;
using KitchenClash.Application.Services;
using KitchenClash.Application.State;
using KitchenClash.Domain;

namespace KitchenClash.Infrastructure.States
{
    /// <summary>
    /// Shows the login screen and waits for auth success/failure events.
    /// On success → SessionLoadingState. On failure → stays and allows retry.
    /// </summary>
    public class LoginState : BaseState
    {
        private const string LoginScreenTypeName = "KitchenClash.Presentation.Screens.LoginScreen, KitchenClash.Presentation";

        private readonly IUIService _uiService;
        private readonly IEventBus _eventBus;
        private readonly IGameStateManager _stateManager;
        private Type _loginScreenType;

        public LoginState(IUIService uiService, IEventBus eventBus, IGameStateManager stateManager)
        {
            _uiService = uiService;
            _eventBus = eventBus;
            _stateManager = stateManager;
        }

        public override void Enter()
        {
            base.Enter();
            GameLogger.Log("[LoginState] Entered - showing LoginScreen");

            _eventBus.Subscribe<LoginSuccessEvent>(OnLoginSuccess);
            _eventBus.Subscribe<LoginFailedEvent>(OnLoginFailed);

            _loginScreenType ??= Type.GetType(LoginScreenTypeName);
            if (_loginScreenType != null)
                _uiService.Show(_loginScreenType);
            else
                GameLogger.LogWarning("[LoginState] LoginScreen type not found — UI will not be shown");
        }

        public override void Exit()
        {
            base.Exit();
            GameLogger.Log("[LoginState] Exiting - unsubscribing events");

            _eventBus.Unsubscribe<LoginSuccessEvent>(OnLoginSuccess);
            _eventBus.Unsubscribe<LoginFailedEvent>(OnLoginFailed);

            if (_loginScreenType != null)
                _uiService.Hide(_loginScreenType);
        }

        private void OnLoginSuccess(LoginSuccessEvent evt)
        {
            GameLogger.Log($"[LoginState] Login success: {evt.UserId}");
            _stateManager.ChangeState<SessionLoadingState>();
        }

        private void OnLoginFailed(LoginFailedEvent evt)
        {
            GameLogger.LogError($"[LoginState] Login failed: {evt.Error}");
            // LoginScreen stays visible – user can retry via the guest button
        }
    }
}
