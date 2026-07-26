using Playcenter.SDK;

namespace KitchenClash.Infrastructure.Flow
{
    /// <summary>
    /// Mutable <see cref="IShellUi"/> holder that breaks the circular dependency between the
    /// <c>IAppFlow</c> factory (which needs the shell for gate phases) and
    /// <c>Composition.PlaycenterSdkBootstrap</c> (which owns the shell but depends on
    /// <c>IAppFlow</c>). Registered as the container's <see cref="IShellUi"/>; bound by
    /// <c>PlaycenterSdkBootstrap.Start()</c> once the SDK shell exists. Mirrors
    /// <see cref="BootRetryRef"/>. Calls made before binding are no-ops.
    /// </summary>
    public sealed class ShellRef : IShellUi
    {
        private IShellUi _target;

        public void Bind(IShellUi target) => _target = target;

        public void Show(ShellScreenId id) => _target?.Show(id);

        public void Hide(ShellScreenId id) => _target?.Hide(id);

        public void HideAll() => _target?.HideAll();

        public void SetProgress(float overall01, string status) => _target?.SetProgress(overall01, status);

        public void SetTheme(IShellTheme theme) => _target?.SetTheme(theme);
    }
}
