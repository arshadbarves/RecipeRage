using System;
using System.Threading.Tasks;
using Playcenter.Services;
using Playcenter.Shell;
using VContainer.Unity;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// Session-scoped bridge: when connectivity declares match forfeit or host-drop timeout,
    /// stop the active <see cref="INetSession"/> so NGO shuts down through the Playcenter port.
    /// </summary>
    /// <remarks>
    /// Subscribes to concrete <see cref="NetworkConnectivityService"/> events (not on
    /// <see cref="IConnectivityService"/>). If the concrete service is unavailable at
    /// construction, the bridge is a no-op; EndGame still stops via <see cref="INetSession"/>.
    /// </remarks>
    public sealed class NetSessionConnectivityBridge : IStartable, IDisposable
    {
        private readonly INetSession _netSession;
        private readonly NetworkConnectivityService _connectivity;
        private bool _started;

        public NetSessionConnectivityBridge(
            INetSession netSession,
            IConnectivityService connectivity = null)
        {
            _netSession = netSession ?? throw new ArgumentNullException(nameof(netSession));
            _connectivity = connectivity as NetworkConnectivityService;
        }

        public void Start()
        {
            if (_connectivity == null || _started)
            {
                return;
            }

            _connectivity.OnMatchForfeit += OnConnectivityStopRequested;
            _connectivity.OnHostDroppedTimeout += OnConnectivityStopRequested;
            _started = true;
        }

        public void Dispose()
        {
            if (!_started || _connectivity == null)
            {
                return;
            }

            _connectivity.OnMatchForfeit -= OnConnectivityStopRequested;
            _connectivity.OnHostDroppedTimeout -= OnConnectivityStopRequested;
            _started = false;
        }

        private void OnConnectivityStopRequested()
        {
            if (_netSession == null || !_netSession.IsActive)
            {
                return;
            }

            // Fire-and-forget: StopAsync is sync underneath (NGO Shutdown).
            _ = StopSessionSafeAsync();
        }

        private async Task StopSessionSafeAsync()
        {
            try
            {
                await _netSession.StopAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Best-effort teardown on forfeit/host-drop; avoid crashing connectivity path.
            }
        }
    }
}
