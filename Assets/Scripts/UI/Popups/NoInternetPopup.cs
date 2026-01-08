using System;
using Modules.Logging;
using Modules.Networking;
using Modules.UI;
using Modules.UI.Core;
using UnityEngine.UIElements;
using VContainer;

namespace UI.Popups
{
    /// <summary>
    /// Popup displayed when internet connection is unavailable
    /// </summary>
    [UIScreen(UIScreenType.NoInternet, UIScreenCategory.Popup, "Popups/NoInternetPopupTemplate")]
    public class NoInternetPopup : BaseUIScreen
    {
        [Inject]
        private IConnectivityService _connectivityService;

        private Label _titleLabel;
        private Label _messageLabel;
        private Button _retryButton;
        private Button _closeButton;

        private Action _onRetry;
        private Action _onClose;

        protected override void OnInitialize()
        {
            _titleLabel = GetElement<Label>("title-label");
            _messageLabel = GetElement<Label>("message-label");
            _retryButton = GetElement<Button>("retry-button");
            _closeButton = GetElement<Button>("close-button");

            if (_retryButton != null) _retryButton.clicked += OnRetryClicked;
            if (_closeButton != null) _closeButton.clicked += OnCloseClicked;

            if (_connectivityService != null) _connectivityService.OnConnectionStatusChanged += OnConnectionStatusChanged;
        }

        private void OnConnectionStatusChanged(bool isConnected)
        {
            if (isConnected) Hide(true);
            else if (!IsVisible) Show();
        }

        protected override void OnShow()
        {
            if (_titleLabel != null && string.IsNullOrEmpty(_titleLabel.text)) _titleLabel.text = "No Internet Connection";
            if (_messageLabel != null && string.IsNullOrEmpty(_messageLabel.text)) _messageLabel.text = "Please check your connection.";
        }

        public void SetData(string title, string message, Action onRetry, Action onClose = null)
        {
            if (_titleLabel != null) _titleLabel.text = title;
            if (_messageLabel != null) _messageLabel.text = message;
            _onRetry = onRetry;
            _onClose = onClose;
        }

        private void OnRetryClicked() { _onRetry?.Invoke(); Hide(true); }
        private void OnCloseClicked() { _onClose?.Invoke(); Hide(true); }

        protected override void OnDispose()
        {
            if (_retryButton != null) _retryButton.clicked -= OnRetryClicked;
            if (_closeButton != null) _closeButton.clicked -= OnCloseClicked;
        }
    }
}