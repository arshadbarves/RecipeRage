using UnityEngine;
using UnityEngine.UIElements;
using KitchenClash.Domain;
using VContainer.Unity;

namespace KitchenClash.Presentation
{
    /// <summary>
    /// GDD Section 5: Connectivity overlay presenter.
    /// - OfflineMenu: full-screen blocking overlay, auto-dismiss on restore.
    /// - OfflineMatch: semi-transparent overlay + countdown, 3 reconnect attempts (5s each).
    /// - HostDropped: "Reconnecting..." overlay, 3s timeout.
    /// ConnectivityOverlayPresenter is an IStartable entry point.
    /// </summary>
    public sealed class ConnectivityOverlayPresenter : IStartable
    {
        private readonly IConnectivityService _connectivity;
        private readonly IRouterService _router;
        private VisualElement _overlay;

        public ConnectivityOverlayPresenter(IConnectivityService connectivity, IRouterService router)
        {
            _connectivity = connectivity;
            _router = router;
        }

        void IStartable.Start()
        {
            _connectivity.OnConnectivityChanged += HandleConnectivityChanged;
            _connectivity.OnStateChanged += HandleStateChanged;
        }

        private void HandleConnectivityChanged(bool isOnline)
        {
            if (isOnline)
            {
                // Auto-dismiss overlay on restore
                DismissOverlay();
            }
        }

        private void HandleStateChanged(ConnectivityState state)
        {
            switch (state)
            {
                case ConnectivityState.Online:
                    DismissOverlay();
                    break;

                case ConnectivityState.OfflineMenu:
                    // Full-screen blocking overlay, retry every 3s (handled by service)
                    ShowOverlay(blocking: true, message: "No Internet Connection", showCountdown: false);
                    break;

                case ConnectivityState.OfflineMatch:
                    // Semi-transparent overlay + countdown + 3 reconnect attempts
                    ShowOverlay(blocking: false, message: "Connection Lost - Reconnecting...", showCountdown: true);
                    break;

                case ConnectivityState.HostDropped:
                    // "Reconnecting..." overlay, 3s timeout
                    ShowOverlay(blocking: false, message: "Host Disconnected - Reconnecting...", showCountdown: false);
                    break;
            }
        }

        private void ShowOverlay(bool blocking, string message, bool showCountdown)
        {
            // TODO: Create/show actual UI overlay via UIService
            // blocking=true → opaque full-screen; blocking=false → semi-transparent
            // message → label text
            // showCountdown → show attempt counter (e.g. "Attempt 1/3")
            GameLogger.Log($"[ConnectivityOverlay] Show: {message} (blocking={blocking}, countdown={showCountdown})");
        }

        private void DismissOverlay()
        {
            // TODO: Hide overlay via UIService
            GameLogger.Log("[ConnectivityOverlay] Dismissed");
        }
    }
}
