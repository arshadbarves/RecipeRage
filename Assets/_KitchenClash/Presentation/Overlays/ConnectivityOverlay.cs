using UnityEngine;
using UnityEngine.UIElements;
using KitchenClash.Domain;
using VContainer.Unity;

namespace KitchenClash.Presentation
{
    /// <summary>
    /// GDD Section 5: Full-screen blocking overlay when offline.
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
        }

        private void HandleConnectivityChanged(bool isOnline)
        {
            if (!isOnline)
            {
                // Show blocking overlay
                // In match: semi-transparent + countdown + 3 reconnect attempts
                // In menu: full block, retries every 3s
            }
            else
            {
                // Auto-dismiss overlay
            }
        }
    }
}
