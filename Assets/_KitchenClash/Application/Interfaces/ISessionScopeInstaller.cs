using VContainer;

namespace KitchenClash.Application
{
    /// <summary>
    /// Installs session/menu-scoped registrations into a child LifetimeScope.
    /// Composition owns the concrete installer (<c>MenuSessionScopeInstaller</c>);
    /// Infrastructure <c>SessionManager</c> only depends on this port.
    /// </summary>
    /// <remarks>
    /// <para><b>Law:</b> Every cold-boot <c>CreateSession</c> child MUST run an installer.
    /// A bare <c>LifetimeScope.CreateChild()</c> has no <c>Configure()</c> and will not
    /// resolve <c>IEconomyService</c> / <c>IWallet</c> / menu services — that caused production
    /// <c>VContainerException</c> after login.</para>
    /// <para>Docs marker <c>ISessionModuleInstaller</c> (if present) is not a substitute;
    /// this interface remains the in-tree registration law.</para>
    /// </remarks>
    public interface ISessionScopeInstaller
    {
        void Install(IContainerBuilder builder);
    }
}
