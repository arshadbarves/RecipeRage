using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Playcenter.Services;

namespace KitchenClash.Tests.EditMode.Gameplay
{
    /// <summary>
    /// Documents GameStarter → INetSession delegation contract without Unity NetworkManager.
    /// Mirrors the sync Start/Stop path used by GameStarter (GetAwaiter().GetResult()).
    /// </summary>
    public sealed class NetSessionDelegationTests
    {
        private sealed class FakeNetSession : INetSession
        {
            public bool IsActive { get; private set; }
            public NetRole? ActiveRole { get; private set; }
            public string LastToken { get; private set; }
            public int StartCount { get; private set; }
            public int StopCount { get; private set; }
            public Exception StartException { get; set; }

            public Task StartAsync(NetRole role, string sessionToken, CancellationToken ct = default)
            {
                if (StartException != null)
                {
                    throw StartException;
                }

                IsActive = true;
                ActiveRole = role;
                LastToken = sessionToken;
                StartCount++;
                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken ct = default)
            {
                IsActive = false;
                ActiveRole = null;
                StopCount++;
                return Task.CompletedTask;
            }
        }

        private sealed class FakeTransport : INetTransportConfigurator
        {
            public NetRole? LastRole { get; private set; }
            public string LastToken { get; private set; }
            public int ConfigureCount { get; private set; }

            public void ConfigureForSession(NetRole role, string sessionToken)
            {
                LastRole = role;
                LastToken = sessionToken;
                ConfigureCount++;
            }
        }

        /// <summary>
        /// Pure coordinator mirroring GameStarter host/client/end delegation to INetSession.
        /// </summary>
        private sealed class NetSessionStartCoordinator
        {
            private readonly INetSession _session;

            public NetSessionStartCoordinator(INetSession session)
            {
                _session = session;
            }

            public bool TryStartHost(string sessionToken)
            {
                try
                {
                    _session.StartAsync(NetRole.Host, sessionToken ?? string.Empty)
                        .GetAwaiter()
                        .GetResult();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public bool TryStartClient(string hostUserId)
            {
                try
                {
                    _session.StartAsync(NetRole.Client, hostUserId ?? string.Empty)
                        .GetAwaiter()
                        .GetResult();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public void End()
            {
                if (_session.IsActive)
                {
                    _session.StopAsync().GetAwaiter().GetResult();
                }
            }
        }

        /// <summary>
        /// Thin lifecycle that applies transport then marks active — unit-testable core of NgoEosNetSession
        /// without NetworkManager.
        /// </summary>
        private sealed class TransportThenActivateLifecycle
        {
            private readonly INetTransportConfigurator _transport;

            public TransportThenActivateLifecycle(INetTransportConfigurator transport)
            {
                _transport = transport;
            }

            public bool IsActive { get; private set; }
            public NetRole? ActiveRole { get; private set; }

            public void Start(NetRole role, string sessionToken)
            {
                _transport.ConfigureForSession(role, sessionToken ?? string.Empty);
                IsActive = true;
                ActiveRole = role;
            }

            public void Stop()
            {
                IsActive = false;
                ActiveRole = null;
            }
        }

        [Test]
        public void StartHost_WhenSessionOk_SetsActiveHost()
        {
            var session = new FakeNetSession();
            var coordinator = new NetSessionStartCoordinator(session);

            Assert.IsTrue(coordinator.TryStartHost("lobby-1"));
            Assert.IsTrue(session.IsActive);
            Assert.AreEqual(NetRole.Host, session.ActiveRole);
            Assert.AreEqual("lobby-1", session.LastToken);
            Assert.AreEqual(1, session.StartCount);
        }

        [Test]
        public void StartClient_WhenSessionOk_SetsActiveClientWithHostToken()
        {
            var session = new FakeNetSession();
            var coordinator = new NetSessionStartCoordinator(session);

            Assert.IsTrue(coordinator.TryStartClient("host-puid-xyz"));
            Assert.IsTrue(session.IsActive);
            Assert.AreEqual(NetRole.Client, session.ActiveRole);
            Assert.AreEqual("host-puid-xyz", session.LastToken);
        }

        [Test]
        public void StartHost_WhenSessionThrows_ReturnsFalseAndNotActive()
        {
            var session = new FakeNetSession
            {
                StartException = new InvalidOperationException("NGO StartHost failed")
            };
            var coordinator = new NetSessionStartCoordinator(session);

            Assert.IsFalse(coordinator.TryStartHost("lobby-1"));
            Assert.IsFalse(session.IsActive);
        }

        [Test]
        public void End_WhenActive_CallsStopAndClearsActive()
        {
            var session = new FakeNetSession();
            var coordinator = new NetSessionStartCoordinator(session);
            Assert.IsTrue(coordinator.TryStartHost("lobby-1"));

            coordinator.End();

            Assert.IsFalse(session.IsActive);
            Assert.IsNull(session.ActiveRole);
            Assert.AreEqual(1, session.StopCount);
        }

        [Test]
        public void End_WhenNotActive_DoesNotStop()
        {
            var session = new FakeNetSession();
            var coordinator = new NetSessionStartCoordinator(session);

            coordinator.End();

            Assert.AreEqual(0, session.StopCount);
        }

        [Test]
        public void TransportThenActivate_Host_ConfiguresThenSetsActive()
        {
            var transport = new FakeTransport();
            var lifecycle = new TransportThenActivateLifecycle(transport);

            lifecycle.Start(NetRole.Host, "token");

            Assert.AreEqual(NetRole.Host, transport.LastRole);
            Assert.AreEqual("token", transport.LastToken);
            Assert.AreEqual(1, transport.ConfigureCount);
            Assert.IsTrue(lifecycle.IsActive);
            Assert.AreEqual(NetRole.Host, lifecycle.ActiveRole);
        }

        [Test]
        public void TransportThenActivate_Stop_ClearsActive()
        {
            var transport = new FakeTransport();
            var lifecycle = new TransportThenActivateLifecycle(transport);
            lifecycle.Start(NetRole.Client, "host-id");

            lifecycle.Stop();

            Assert.IsFalse(lifecycle.IsActive);
            Assert.IsNull(lifecycle.ActiveRole);
        }
    }
}
