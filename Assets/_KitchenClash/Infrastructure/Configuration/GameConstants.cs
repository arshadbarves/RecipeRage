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

            // v2 mode map scenes (Tools → RecipeRage → Create v2 Mode Scenes)
            public const string RushService = "Map_RushService";
            public const string HellsKitchen = "Map_HellsKitchen";
            public const string LastPlateStanding = "Map_LastPlateStanding";
        }
    }
}
