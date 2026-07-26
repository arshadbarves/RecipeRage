using System;
using KitchenClash.Infrastructure.DI;
using NUnit.Framework;
using Playcenter.Shell;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    public sealed class SessionManagerInstallerTests
    {
        private sealed class StubEventBus : IEventBus
        {
            public void Publish<T>(T evt) where T : class { }
            public void Subscribe<T>(Action<T> handler) where T : class { }
            public void Unsubscribe<T>(Action<T> handler) where T : class { }
            public void ClearAllSubscriptions() { }
        }

        [Test]
        public void CreateSession_WhenInstallerMissing_ThrowsInvalidOperationException()
        {
            // Guard runs before container/UI are touched.
            var manager = new SessionManager(
                container: null,
                eventBus: new StubEventBus(),
                uiService: null,
                sessionScopeInstaller: null);

            var ex = Assert.Throws<InvalidOperationException>(() => manager.CreateSession());
            StringAssert.Contains("ISessionScopeInstaller", ex.Message);
        }
    }
}
