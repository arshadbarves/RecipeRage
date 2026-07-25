using System;
using System.Threading;
using System.Threading.Tasks;
using Playcenter.Services;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// Wraps any INetSession with role validation and reconnect wiring. The sole
    /// start/stop path for net sessions in consuming games (wiki law).
    /// </summary>
    public sealed class NetSessionOrchestrator
    {
        private readonly INetSession _session;
        private readonly ReconnectStateMachine _reconnect;

        public ReconnectStateMachine Reconnect => _reconnect;
        public bool IsActive => _session.IsActive;

        public NetSessionOrchestrator(INetSession session, ReconnectStateMachine reconnect)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _reconnect = reconnect ?? throw new ArgumentNullException(nameof(reconnect));
        }

        public async Task StartAsync(NetRole role, string sessionToken, CancellationToken ct = default)
        {
            await _session.StartAsync(role, sessionToken, ct).ConfigureAwait(false);
            _reconnect.OnConnected();
        }

        public async Task StopAsync(CancellationToken ct = default)
        {
            await _session.StopAsync(ct).ConfigureAwait(false);
        }

        public void NotifyDisconnected()
        {
            _reconnect.OnDisconnected();
        }
    }
}
