using UnityEngine;

namespace Core
{
    /// <summary>
    /// Game-wide constants and configuration values
    /// </summary>
    public static class GameConstants
    {
        /// <summary>
        /// Version display string for UI
        /// </summary>
        public static string VERSION_DISPLAY => $"v{Application.version}";

        /// <summary>
        /// Company display string for UI
        /// </summary>
        public static string COMPANY_DISPLAY => Application.companyName;


        public static readonly string GAME_TITLE = Application.productName;

        /// <summary>
        /// Default loading tips
        /// </summary>
        public static readonly string[] DEFAULT_LOADING_TIPS = {
            "Tip: Use teamwork to complete orders faster!",
            "Tip: Keep your kitchen clean for bonus points!",
            "Tip: Watch the timer - speed matters!",
            "Tip: Coordinate with your team for maximum efficiency!",
            "Tip: Different ingredients cook at different speeds!",
            "Tip: Plan your moves ahead to avoid kitchen chaos!"
        };
    }
}