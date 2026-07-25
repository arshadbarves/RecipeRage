namespace Playcenter.MobileCore
{
    /// <summary>
    /// Installs session-scoped registrations. Law: every CreateAsync MUST run an
    /// installer — a bare scope has no services and fails at resolve time.
    /// </summary>
    public interface ISessionScopeInstaller
    {
        void Install(ISessionContainerBuilder builder);
    }
}
