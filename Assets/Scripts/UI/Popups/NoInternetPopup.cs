using System;
using Core.Bootstrap;
using Core.Logging;
using UI.Core;
using UnityEngine.UIElements;

namespace UI.Popups
{
    /// <summary>
    /// Popup displayed when internet connection is unavailable
    /// </summary>
    [UIScreen(UIScreenType.NoInternet, UIScreenCategory.Popup, "Popups/NoInternetPopupTemplate")]
    public class NoInternetPopup : BaseUIScreen
    {
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

            if (_retryButton != null)
            {
                _retryButton.clicked += OnRetryClicked;
            }

            if (_closeButton != null)
            {
                _closeButton.clicked += OnCloseClicked;
            }

            // Subscribe to connectivity changes
            var connectivity = GameBootstrap.Services?.ConnectivityService;
            if (connectivity != null)
            {
                connectivity.OnConnectionStatusChanged += OnConnectionStatusChanged;
            }

            GameLogger.Log("NoInternetPopup initialized");
        }

        private void OnConnectionStatusChanged(bool isConnected)
        {
            if (isConnected)
            {
                // Auto-hide when connection returns
                Hide(animate: true);
            }
            else
            {
                // Auto-show when connection lost (if not already showing)
                if (!IsVisible)
                {
                    Show();
                }
            }
        }

        protected override void OnShow()
        {
            // Set default text if not already set
            if (_titleLabel != null && string.IsNullOrEmpty(_titleLabel.text))
            {
                _titleLabel.text = "No Internet Connection";
            }

            if (_messageLabel != null && string.IsNullOrEmpty(_messageLabel.text))
            {
                _messageLabel.text = "Please check your internet connection and try again.";
            }
        }

        /// <summary>
        /// Sets popup data
        /// </summary>
        public void SetData(string title, string message, Action onRetry, Action onClose = null)
        {
            if (_titleLabel != null && !string.IsNullOrEmpty(title))
            {
                _titleLabel.text = title;
            }

            if (_messageLabel != null && !string.IsNullOrEmpty(message))
            {
                _messageLabel.text = message;
            }

            _onRetry = onRetry;
            _onClose = onClose;
        }

        private void OnRetryClicked()
        {
            GameLogger.Log("Retry button clicked");
            _onRetry?.Invoke();
            Hide(animate: true);
        }

        private void OnCloseClicked()
        {
            GameLogger.Log("Close button clicked");
            _onClose?.Invoke();
            Hide(animate: true);
        }

        protected override void OnDispose()
        {
            if (_retryButton != null)
            {
                _retryButton.clicked -= OnRetryClicked;
            }

            if (_closeButton != null)
            {
                _closeButton.clicked -= OnCloseClicked;
            }

            _onRetry = null;
            _onClose = null;
        }
    }
}
