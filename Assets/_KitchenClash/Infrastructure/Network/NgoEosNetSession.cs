using System;
using System.Threading;
using System.Threading.Tasks;
using Playcenter.Services;
using Unity.Netcode;
using VContainer;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// NGO + EOS adapter for Playcenter <see cref="INetSession"/>.
    /// Obtains <see cref="NetworkManager"/> from <see cref="IMatchContext"/> (fallback Singleton)
    /// and configures transport via <see cref="INetTransportConfigurator"/> before StartHost/StartClient.
    /// </summary>
    public sealed class NgoEosNetSession : INetSession
    {
        private readonly INetTransportConfigurator _transport;
        private IMatchContext _matchContext;

        /// <summary>
        /// Session-scoped construction: match context is supplied later via
        /// <see cref="SetMatchContext"/> when the match scope is created.
        /// </summary>
        [Inject]
        public NgoEosNetSession(INetTransportConfigurator transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public NgoEosNetSession(INetTransportConfigurator transport, IMatchContext matchContext)
            : this(transport)
        {
            _matchContext = matchContext;
        }

        /// <inheritdoc />
        public bool IsActive { get; private set; }

        /// <inheritdoc />
        public NetRole? ActiveRole { get; private set; }

        /// <summary>
        /// Supplies or replaces the match context (NetworkManager + shutdown path).
        /// Called when match scope becomes available after session-scoped construction.
        /// </summary>
        public void SetMatchContext(IMatchContext matchContext)
        {
            _matchContext = matchContext;
        }

        /// <inheritdoc />
        public Task StartAsync(NetRole role, string sessionToken, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (IsActive)
            {
                throw new InvalidOperationException("[NgoEosNetSession] Session is already active.");
            }

            var networkManager = ResolveNetworkManager();
            if (networkManager == null)
            {
                throw new InvalidOperationException("[NgoEosNetSession] NetworkManager not available.");
            }

            _transport.ConfigureForSession(role, sessionToken ?? string.Empty);

            var started = role == NetRole.Host
                ? networkManager.StartHost()
                : networkManager.StartClient();

            if (!started)
            {
                throw new InvalidOperationException($"[NgoEosNetSession] NGO Start{role} failed.");
            }

            IsActive = true;
            ActiveRole = role;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            // Prefer match-context shutdown (clears scene runtime + NGO).
            if (_matchContext != null)
            {
                _matchContext.ShutdownNetworkSession();
            }
            else
            {
                var networkManager = NetworkManager.Singleton;
                if (networkManager != null &&
                    (networkManager.IsListening || networkManager.IsServer || networkManager.IsClient))
                {
                    networkManager.Shutdown();
                }
            }

            IsActive = false;
            ActiveRole = null;
            return Task.CompletedTask;
        }

        private NetworkManager ResolveNetworkManager()
        {
            return _matchContext?.NetworkManager ?? NetworkManager.Singleton;
        }
    }
}
