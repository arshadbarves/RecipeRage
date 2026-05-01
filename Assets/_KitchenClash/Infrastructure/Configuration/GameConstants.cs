namespace KitchenClash.Infrastructure.Configuration
{
    public static class GameConstants
    {
        public static string VersionDisplay => $"v{UnityEngine.Application.version}";
        public static string CompanyDisplay => UnityEngine.Application.companyName;
        public static readonly string GameTitle = UnityEngine.Application.productName;

        public static class Scenes
        {
            public const string Bootstrap = "Bootstrap";
            public const string MainMenu = "MainMenu";
            public const string Game = "Game";
            public const string Tutorial = "Tutorial";
        }
    }
}
