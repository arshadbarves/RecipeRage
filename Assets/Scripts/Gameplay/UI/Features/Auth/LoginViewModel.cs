using System;
using VContainer;
using Cysharp.Threading.Tasks;
using Core.Auth;
using Core.Localization;
using Core.Logging;
using Core.Shared;
using Core.UI.Core;

namespace Gameplay.UI.Features.Auth
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly ILocalizationManager _localization;

        public BindableProperty<bool> IsLoading { get; } = new BindableProperty<bool>(false);
        public BindableProperty<string> StatusText { get; } = new BindableProperty<string>("Ready to connect");

        [Inject]
        public LoginViewModel(
            IAuthService authService,
            ILocalizationManager localization)
        {
            _authService = authService;
            _localization = localization;
        }

        public override void Initialize()
        {
            base.Initialize();
            Reset();
        }

        public void Reset()
        {
            IsLoading.Value = false;
            StatusText.Value = _localization.GetText("login_status_ready") ?? "Ready to connect";
        }

        /// <summary>
        /// Initiates EOS Device Login for guest play
        /// Connects to Epic Online Services using device credentials
        /// </summary>
        public void LoginAsGuest()
        {
            HandleEosGuestLoginAsync().Forget();
        }

        private async UniTaskVoid HandleEosGuestLoginAsync()
        {
            if (IsLoading.Value) return;

            IsLoading.Value = true;
            StatusText.Value = _localization.GetText("login_status_connecting") ?? "Connecting to EOS...";

            try
            {
                // Use EOS Device Login for guest authentication
                bool success = await _authService.LoginAsync(AuthType.DeviceID);

                if (success)
                {
                    StatusText.Value = _localization.GetText("login_status_connected") ?? "Connected!";
                    // Success event handled by LoginState via EventBus
                }
                else
                {
                    StatusText.Value = _localization.GetText("login_status_failed") ?? "Connection failed";
                    IsLoading.Value = false;
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"[LoginViewModel] EOS Guest Login Error: {ex.Message}");
                StatusText.Value = _localization.GetText("login_status_error") ?? "Error occurred";
                IsLoading.Value = false;
            }
        }
    }
}
