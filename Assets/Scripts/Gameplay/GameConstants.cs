using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Game-wide constants and configuration values
    /// </summary>
    public static class GameConstants
    {
        public static string VersionDisplay => $"v{Application.version}";

        public static string CompanyDisplay => Application.companyName;

        public static readonly string GameTitle = Application.productName;

        public static class Scenes
        {
            public const string Bootstrap = "Bootstrap";
            public const string MainMenu = "MainMenu";
            public const string Game = "Game";
            public const string Tutorial = "Tutorial";
        }
    }
}