using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Playcenter.Services;

namespace KitchenClash.Tests.EditMode.Playcenter.Net
{
    public sealed class NetSessionPortTests
    {
        private sealed class FakeNetSession : INetSession
        {
            public bool IsActive { get; private set; }
            public NetRole? ActiveRole { get; private set; }

            public Task StartAsync(NetRole role, string sessionToken, CancellationToken ct = default)
            {
                IsActive = true;
                ActiveRole = role;
                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken ct = default)
            {
                IsActive = false;
                ActiveRole = null;
                return Task.CompletedTask;
            }
        }

        private sealed class FakeTransportConfigurator : INetTransportConfigurator
        {
            public NetRole? LastRole { get; private set; }
            public string LastToken { get; private set; }

            public void ConfigureForSession(NetRole role, string sessionToken)
            {
                LastRole = role;
                LastToken = sessionToken;
            }
        }

        [Test]
        public async Task StartAsync_SetsActiveAndRole()
        {
            var session = new FakeNetSession();

            await session.StartAsync(NetRole.Host, "token-abc");

            Assert.IsTrue(session.IsActive);
            Assert.AreEqual(NetRole.Host, session.ActiveRole);
        }

        [Test]
        public async Task StopAsync_ClearsActive()
        {
            var session = new FakeNetSession();
            await session.StartAsync(NetRole.Client, "token-xyz");

            await session.StopAsync();

            Assert.IsFalse(session.IsActive);
            Assert.IsNull(session.ActiveRole);
        }

        [Test]
        public void ConfigureForSession_RecordsRoleAndToken()
        {
            var configurator = new FakeTransportConfigurator();

            configurator.ConfigureForSession(NetRole.Client, "session-token-1");

            Assert.AreEqual(NetRole.Client, configurator.LastRole);
            Assert.AreEqual("session-token-1", configurator.LastToken);
        }
    }
}
