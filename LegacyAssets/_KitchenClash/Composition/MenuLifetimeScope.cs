using VContainer;
using VContainer.Unity;

/// <summary>
/// Scene-owned child of <see cref="RootLifetimeScope"/> on MainMenu.
/// Does <b>not</b> install session services. Those are installed once by
/// <c>SessionManager.CreateSession</c> via <see cref="MenuSessionScopeInstaller"/> /
/// <see cref="MenuSessionRegistrations"/>.
/// Scene presentation ports bind through <see cref="MenuSceneBinder"/> into Root gateways.
/// Keeping this scope empty avoids a second root/orphan container and double wallet credit.
/// </summary>
public class MenuLifetimeScope : LifetimeScope
{
    /// <summary>
    /// Prefer live Root instance (DontDestroyOnLoad) over TypeName-only lookup.
    /// Scene YAML still sets parentReference.TypeName = RootLifetimeScope as backup.
    /// </summary>
    protected override LifetimeScope FindParent() => Find<RootLifetimeScope>();

    protected override void Configure(IContainerBuilder builder)
    {
        // Intentionally empty — session DI is CreateSession-only; scene bind-in is MenuSceneBinder.
    }
}
