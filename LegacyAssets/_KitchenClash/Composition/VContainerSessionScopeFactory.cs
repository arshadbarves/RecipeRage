using Playcenter.MobileCore;
using VContainer;
using VContainer.Unity;

namespace KitchenClash.Composition
{
    /// <summary>
    /// Game-side session scope factory: wraps LifetimeScope child creation behind the
    /// module's container-neutral seam. Enforces the installer law (sole
    /// MenuSessionRegistrations path) exactly as wiki mandates.
    /// </summary>
    public sealed class VContainerSessionScopeFactory : ISessionScopeFactory
    {
        private readonly LifetimeScope _root;

        public VContainerSessionScopeFactory(LifetimeScope root)
        {
            _root = root;
        }

        public ISessionScopeHandle Create(ISessionScopeInstaller installer)
        {
            LifetimeScope child = _root.CreateChild(builder =>
                installer.Install(new VContainerSessionContainerBuilder(builder)));
            return new VContainerSessionScopeHandle(child);
        }
    }
}
