using System;
using Core.Localization;
using Core.Logging;
using Core.Reactive;
using RecipeRage.Modules.Auth.Core;
using UI.Core;
using VContainer;
using Cysharp.Threading.Tasks;

namespace UI.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly ILocalizationManager _localization;

        public BindableProperty<bool> IsLoading { get; } = new BindableProperty<bool>(false);
        public BindableProperty<string> StatusText { get; } = new BindableProperty<string>("Ready to connect");

        [Inject]
        public LoginViewModel(IAuthService authService, ILocalizationManager localization)
        {
            _authService = authService;
            _localization = localization;
        }

        public override void Initialize()
        {
            base.Initialize();
            StatusText.Value = _localization.GetText("login_status_ready");
        }

        public void LoginAsGuest()
        {
            HandleLoginAsync(AuthType.DeviceID).Forget();
        }

        private async UniTaskVoid HandleLoginAsync(AuthType type)
        {
            if (IsLoading.Value) return;

            IsLoading.Value = true;
            StatusText.Value = _localization.GetText("login_status_connecting");

            try
            {
                bool success = await _authService.LoginAsync(type);
                if (success)
                {
                    StatusText.Value = _localization.GetText("login_status_connected");
                    // Success event is handled by LoginState observing AuthService or EventBus
                }
                else
                {
                    StatusText.Value = _localization.GetText("login_status_failed");
                    IsLoading.Value = false;
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"[LoginViewModel] Login Error: {ex.Message}");
                StatusText.Value = _localization.GetText("login_status_error");
                IsLoading.Value = false;
            }
        }
    }
}