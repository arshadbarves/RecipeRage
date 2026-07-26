using KitchenClash.Application;
using VContainer;

namespace KitchenClash.Composition
{
    /// <summary>
    /// Root-registered installer applied when SessionManager creates the session child scope.
    /// Keeps Infrastructure free of Composition/MenuLifetimeScope types.
    /// </summary>
    public sealed class MenuSessionScopeInstaller : ISessionScopeInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            MenuSessionRegistrations.Install(builder);
        }
    }
}
