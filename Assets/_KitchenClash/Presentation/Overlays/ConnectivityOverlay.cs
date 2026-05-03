using UnityEngine;
using UnityEngine.UIElements;
using KitchenClash.Domain;
using KitchenClash.Application.Services;
using KitchenClash.Presentation.Common;
using VContainer.Unity;

namespace KitchenClash.Presentation.Overlays
{
    // ─────────────────────────────────────────────────────────────────────────
    // Screen (UI layer) — registered and driven by ConnectivityOverlayPresenter
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// GDD Section 5: Connectivity overlay screen.
    /// Displays a card with title, message, optional countdown and a retry button.
    /// Shown/hidden by <see cref="ConnectivityOverlayPresenter"/> via IUIService.
    /// </summary>
    [UIScreen(UIScreenCategory.Overlay, "Overlays/ConnectivityOverlay")]
    public sealed class ConnectivityOverlayScreen : BaseUIScreen
    {
        private Label _titleLabel;
        private Label _messageLabel;
        private Label _countdownLabel;
        private Button _retryButton;

        protected override void OnInitialize()
        {
            _titleLabel    = GetElement<Label>("lbl-connectivity-title");
            _messageLabel  = GetElement<Label>("lbl-connectivity-msg");
            _countdownLabel = GetElement<Label>("lbl-retry-countdown");
            _retryButton   = GetElement<Button>("btn-retry");
        }

        /// <summary>Populate and show the overlay card.</summary>
        public void Configure(string title, string message, bool showCountdown, bool blocking)
        {
            if (_titleLabel   != null) _titleLabel.text   = title;
            if (_messageLabel != null) _messageLabel.text = message;

            if (_countdownLabel != null)
                _countdownLabel.style.display = showCountdown
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

            // blocking → fully opaque root; non-blocking → semi-transparent
            if (Container != null)
                Container.style.backgroundColor = blocking
                    ? new StyleColor(new Color(0f, 0f, 0f, 0.92f))
                    : new StyleColor(new Color(0f, 0f, 0f, 0.55f));
        }

        protected override void OnDispose() { }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Presenter (application/domain bridge) — IStartable entry point
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// GDD Section 5: Connectivity overlay presenter.
    /// - OfflineMenu: full-screen blocking overlay, auto-dismiss on restore.
    /// - OfflineMatch: semi-transparent overlay + countdown, 3 reconnect attempts (5s each).
    /// - HostDropped: "Reconnecting..." overlay, 3s timeout.
    /// </summary>
    public sealed class ConnectivityOverlayPresenter : IStartable
    {
        private readonly IConnectivityService _connectivity;
        private readonly IUIService _uiService;

        public ConnectivityOverlayPresenter(IConnectivityService connectivity, IUIService uiService)
        {
            _connectivity = connectivity;
            _uiService    = uiService;
        }

        void IStartable.Start()
        {
            _connectivity.OnConnectivityChanged += HandleConnectivityChanged;
            _connectivity.OnStateChanged        += HandleStateChanged;
        }

        private void HandleConnectivityChanged(bool isOnline)
        {
            if (isOnline) DismissOverlay();
        }

        private void HandleStateChanged(ConnectivityState state)
        {
            switch (state)
            {
                case ConnectivityState.Online:
                    DismissOverlay();
                    break;

                case ConnectivityState.OfflineMenu:
                    // Full-screen blocking overlay, retry every 3s (handled by connectivity service)
                    ShowOverlay(
                        title:         "NO CONNECTION",
                        message:       "Check your internet connection",
                        showCountdown: false,
                        blocking:      true);
                    break;

                case ConnectivityState.OfflineMatch:
                    // Semi-transparent overlay + countdown + up to 3 reconnect attempts (5s each)
                    ShowOverlay(
                        title:         "CONNECTION LOST",
                        message:       "Reconnecting…",
                        showCountdown: true,
                        blocking:      false);
                    break;

                case ConnectivityState.HostDropped:
                    // Semi-transparent "Reconnecting…" overlay, 3s timeout
                    ShowOverlay(
                        title:         "HOST DISCONNECTED",
                        message:       "Reconnecting…",
                        showCountdown: false,
                        blocking:      false);
                    break;
            }
        }

        private void ShowOverlay(string title, string message, bool showCountdown, bool blocking)
        {
            GameLogger.Log($"[ConnectivityOverlay] Show '{title}' (blocking={blocking}, countdown={showCountdown})");
            _uiService.ShowOverlay<ConnectivityOverlayScreen>();

            // Configure after show so the screen has been initialised
            var screen = _uiService.GetScreen<ConnectivityOverlayScreen>();
            screen?.Configure(title, message, showCountdown, blocking);
        }

        private void DismissOverlay()
        {
            GameLogger.Log("[ConnectivityOverlay] Dismissed");
            _uiService.HideOverlay<ConnectivityOverlayScreen>();
        }
    }
}
