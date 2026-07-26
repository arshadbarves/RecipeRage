using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Playcenter.MobileCore;

namespace RecipeRage.Tests.Playcenter.MobileCore
{
    public sealed class SessionLifecycleControllerTests
    {
        private sealed class FakeHandle : ISessionScopeHandle
        {
            public bool Disposed { get; private set; }
            public void Dispose() => Disposed = true;
            public T Get<T>() where T : class => null;
            public bool TryGet<T>(out T service) where T : class { service = null; return false; }
        }

        private sealed class FakeFactory : ISessionScopeFactory
        {
            public FakeHandle Handle { get; } = new FakeHandle();
            public bool InstallerSeen { get; private set; }

            public ISessionScopeHandle Create(ISessionScopeInstaller installer)
            {
                InstallerSeen = installer != null;
                return Handle;
            }
        }

        private sealed class FakeInstaller : ISessionScopeInstaller
        {
            public void Install(ISessionContainerBuilder builder) { }
        }

        [Test]
        public void CreateAsync_TransitionsNoneToActive()
        {
            var factory = new FakeFactory();
            var controller = new SessionLifecycleController(factory, new FakeInstaller());

            Task t = controller.CreateAsync();

            Assert.IsTrue(t.IsCompletedSuccessfully);
            Assert.AreEqual(SessionState.Active, controller.State);
            Assert.IsTrue(factory.InstallerSeen);
        }

        [Test]
        public void CreateAsync_WithoutInstaller_Throws()
        {
            var controller = new SessionLifecycleController(new FakeFactory(), null);

            Assert.ThrowsAsync<InvalidOperationException>(async () => await controller.CreateAsync());
            Assert.AreEqual(SessionState.None, controller.State);
        }

        [Test]
        public void CreateAsync_WhenActive_Throws()
        {
            var controller = new SessionLifecycleController(new FakeFactory(), new FakeInstaller());
            controller.CreateAsync().Wait();

            Assert.ThrowsAsync<InvalidOperationException>(async () => await controller.CreateAsync());
        }

        [Test]
        public async Task TeardownAsync_DisposesScope_ReturnsToNone()
        {
            var factory = new FakeFactory();
            var controller = new SessionLifecycleController(factory, new FakeInstaller());
            await controller.CreateAsync();

            await controller.TeardownAsync();

            Assert.AreEqual(SessionState.None, controller.State);
            Assert.IsTrue(factory.Handle.Disposed);
        }

        [Test]
        public void TeardownAsync_WhenNone_Throws()
        {
            var controller = new SessionLifecycleController(new FakeFactory(), new FakeInstaller());

            Assert.ThrowsAsync<InvalidOperationException>(async () => await controller.TeardownAsync());
        }
    }
}
