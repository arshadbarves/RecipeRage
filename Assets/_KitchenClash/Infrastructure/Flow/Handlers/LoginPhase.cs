using System;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using Playcenter.GameFlow;

namespace KitchenClash.Infrastructure.Flow.Handlers
{
    /// <summary>
    /// Login side phase: show LoginScreen; on success load session then CompleteSidePhase.
    /// </summary>
    public sealed class LoginPhase
    {
        private const string LoginScreenTypeName =
            "KitchenClash.Presentation.Screens.LoginScreen, KitchenClash.Presentation";

        private readonly IUIService _uiService;
        private readonly IEventBus _eventBus;
        private readonly IAppFlow _appFlow;
        private readonly SessionLoader _sessionLoader;

        private Type _loginScreenType;
        private bool _active;

        public LoginPhase(
            IUIService uiService,
            IEventBus eventBus,
            IAppFlow appFlow,
            SessionLoader sessionLoader)
        {
            _uiService = uiService;
            _eventBus = eventBus;
            _appFlow = appFlow;
            _sessionLoader = sessionLoader;
        }

        public void Enter()
        {
            Exit();
            _active = true;
            GameLogger.Log("[LoginPhase] Entered - showing LoginScreen");

            _eventBus?.Subscribe<LoginSuccessEvent>(OnLoginSuccess);
            _eventBus?.Subscribe<LoginFailedEvent>(OnLoginFailed);

            _loginScreenType ??= Type.GetType(LoginScreenTypeName);
            if (_loginScreenType != null)
            {
                _uiService?.Show(_loginScreenType);
            }
            else
            {
                GameLogger.LogWarning("[LoginPhase] LoginScreen type not found — UI will not be shown");
            }
        }

        public void Exit()
        {
            if (!_active)
            {
                return;
            }

            _active = false;
            GameLogger.Log("[LoginPhase] Exiting - unsubscribing events");

            _eventBus?.Unsubscribe<LoginSuccessEvent>(OnLoginSuccess);
            _eventBus?.Unsubscribe<LoginFailedEvent>(OnLoginFailed);

            if (_loginScreenType != null)
            {
                _uiService?.Hide(_loginScreenType);
            }
        }

        private async void OnLoginSuccess(LoginSuccessEvent evt)
        {
            if (!_active)
            {
                return;
            }

            GameLogger.Log($"[LoginPhase] Login success: {evt.UserId}");

            try
            {
                await _sessionLoader.LoadAsync();
                if (!_active)
                {
                    return;
                }

                _appFlow?.CompleteSidePhase();
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
            }
        }

        private void OnLoginFailed(LoginFailedEvent evt)
        {
            GameLogger.LogError($"[LoginPhase] Login failed: {evt.Error}");
        }
    }
}
