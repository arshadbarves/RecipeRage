using Core.UI;

using Core.UI.Core;
using Core.UI.Interfaces;
using UnityEngine.UIElements;
using VContainer;

namespace Gameplay.UI.Features.Auth
{
    /// <summary>
    /// New Login View implementing the Skewed Shop aesthetic.
    /// Handles production-ready EOS DeviceID authentication via ViewModel.
    /// </summary>
    [UIScreen(UIScreenCategory.Overlay, "Screens/LoginViewTemplate")]
    public class LoginView : BaseUIScreen
    {
        [Inject]
        private LoginViewModel _viewModel;

        private Button _guestLoginButton;
        private Label _statusText;
        private VisualElement _loadingIndicator;

        protected override void OnInitialize()
        {
            _guestLoginButton = GetElement<Button>("guest-login-button");
            _statusText = GetElement<Label>("status-text");
            _loadingIndicator = GetElement<VisualElement>("loading-indicator");

            TransitionType = UITransitionType.SlideUp;

            if (_guestLoginButton != null)
            {
                _guestLoginButton.clicked += OnGuestLoginClicked;
            }

            BindViewModel();
        }

        private void BindViewModel()
        {
            if (_viewModel == null) return;

            _viewModel.IsLoading.Bind(isLoading =>
            {
                if (_loadingIndicator != null) _loadingIndicator.style.display = isLoading ? DisplayStyle.Flex : DisplayStyle.None;
                if (_guestLoginButton != null) _guestLoginButton.SetEnabled(!isLoading);
            });

            _viewModel.StatusText.Bind(text =>
            {
                if (_statusText != null) _statusText.text = text.ToUpper();
            });
        }

        protected override void OnShow()
        {
            // Reset logic if needed, but ViewModel state persists if not disposed
        }

        public override void ResetState()
        {
            base.ResetState();
            _viewModel?.Reset();
        }

        private void OnGuestLoginClicked()
        {
            _viewModel?.LoginAsGuest();
        }

        protected override void OnDispose()
        {
            if (_guestLoginButton != null)
            {
                _guestLoginButton.clicked -= OnGuestLoginClicked;
            }
            _viewModel?.Dispose();
        }
    }
}
