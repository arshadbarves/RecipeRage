using Playcenter.MobileCore;

namespace KitchenClash.Composition
{
    /// <summary>
    /// Wraps the game's existing KitchenClash.Application ISessionScopeInstaller
    /// (MenuSessionScopeInstaller) into the module's container-neutral port, keeping
    /// the sole MenuSessionRegistrations path untouched (wiki law).
    /// </summary>
    public sealed class MenuSessionScopeInstallerAdapter : Playcenter.MobileCore.ISessionScopeInstaller
    {
        private readonly KitchenClash.Application.ISessionScopeInstaller _inner;

        public MenuSessionScopeInstallerAdapter(KitchenClash.Application.ISessionScopeInstaller inner)
        {
            _inner = inner;
        }

        public void Install(ISessionContainerBuilder builder)
        {
            if (builder is VContainerSessionContainerBuilder vcontainer)
            {
                _inner.Install(vcontainer.Inner);
            }
        }
    }
}
