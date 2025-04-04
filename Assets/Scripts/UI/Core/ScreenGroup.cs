namespace RecipeRage.UI.Core
{
    /// <summary>
    /// Defines logical groups for screens, influencing how they are displayed and managed.
    /// </summary>
    public enum ScreenGroup
    {
        /// <summary>
        /// Screens that typically occupy the full viewport.
        /// Only one FullScreen can be active at a time (usually replaces the previous one).
        /// </summary>
        FullScreen = 0,

        /// <summary>
        /// Screens that appear on top of existing FullScreens (e.g., modals, confirmation dialogs).
        /// Multiple Popups can potentially be stacked.
        /// </summary>
        Popup = 1,

        /// <summary>
        /// Small, often temporary UI elements like tooltips or notifications.
        /// </summary>
        Overlay = 2,

        // Add more groups if needed (e.g., HudElements)
    }
}
