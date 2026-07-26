namespace Playcenter.SDK
{
    public sealed class NullShellUi : IShellUi
    {
        public void Show(ShellScreenId id) { }
        public void Hide(ShellScreenId id) { }
        public void HideAll() { }
        public void SetProgress(float overall01, string status) { }
        public void SetTheme(IShellTheme theme) { }
    }
}
