using System;
using System.Threading.Tasks;
using Playcenter.MobileCore;
using Playcenter.Services;
using Playcenter.Shell;
using VContainer.Unity;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// Session-scoped bridge: feeds connectivity signals into the module's
    /// ReconnectStateMachine and stops the session via NetSessionOrchestrator on
    /// terminal failure (forfeit / host-drop timeout — no host migration in v1).
    /// </summary>
    /// <remarks>
    /// Subscribes to concrete <see cref="NetworkConnectivityService"/> events (not on
    /// <see cref="IConnectivityService"/>). If the concrete service is unavailable at
    /// construction, the bridge is a no-op; EndGame still stops via the orchestrator.
    /// </remarks>
    public sealed class NetSessionConnectivityBridge : IStartable, IDisposable
    {
        private readonly NetSessionOrchestrator _orchestrator;
        private readonly NetworkConnectivityService _connectivity;
        private bool _started;

        public NetSessionConnectivityBridge(
            NetSessionOrchestrator orchestrator,
            IConnectivityService connectivity = null)
        {
            _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
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

        public void NotifyDisconnected()
        {
            _orchestrator.NotifyDisconnected();
        }

        public void NotifyConnected()
        {
            _orchestrator.Reconnect.OnConnected();
        }

        private void OnConnectivityStopRequested()
        {
            if (!_orchestrator.IsActive)
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
                await _orchestrator.StopAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Best-effort teardown on forfeit/host-drop; avoid crashing connectivity path.
            }
        }
    }
}
