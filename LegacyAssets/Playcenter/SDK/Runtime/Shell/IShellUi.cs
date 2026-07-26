namespace Playcenter.SDK
{
    public interface IShellUi
    {
        void Show(ShellScreenId id);
        void Hide(ShellScreenId id);
        void HideAll();
        void SetProgress(float overall01, string status);
        void SetTheme(IShellTheme theme);
    }
}
