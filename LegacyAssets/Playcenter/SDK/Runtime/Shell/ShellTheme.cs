namespace Playcenter.SDK
{
    public sealed class ShellTheme : IShellTheme
    {
        public string OverrideUssResourcesPath { get; }

        public ShellTheme(string overrideUssResourcesPath = null)
        {
            OverrideUssResourcesPath = overrideUssResourcesPath;
        }
    }
}
