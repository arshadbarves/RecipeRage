namespace Core.Core.UI
{
    /// <summary>
    /// Defines UI screen categories for stack-based management
    /// Each category has its own stack, allowing proper layering and history
    /// AAA-grade approach: Categories prevent priority conflicts and enable clean state management
    /// </summary>
    public enum UIScreenCategory
    {
        /// <summary>
        /// System-level screens (Splash, Maintenance)
        /// - Only one active at a time
        /// - Blocks all other UI
        /// - No history tracking
        /// Priority Range: 1000+
        /// </summary>
        System = 0,

        /// <summary>
        /// Full-screen overlays (Loading, Login)
        /// - Blocks interaction with screens below
        /// - Can stack (e.g., loading over login)
        /// - Limited history tracking
        /// Priority Range: 900-999
        /// </summary>
        Overlay = 1,

        /// <summary>
        /// Modal dialogs (Confirmation, Error dialogs)
        /// - Blocks interaction with screens below
        /// - Multiple modals can stack
        /// - Full history tracking for back navigation
        /// Priority Range: 800-899
        /// </summary>
        Modal = 2,

        /// <summary>
        /// Popup windows (Friends list, Username entry)
        /// - Allows interaction with screens below (optional)
        /// - Multiple popups can coexist
        /// - Full history tracking
        /// Priority Range: 700-799
        /// </summary>
        Popup = 3,

        /// <summary>
        /// Main navigation screens (MainMenu, Lobby, Game)
        /// - Only one active at a time per category
        /// - Full history tracking for back navigation
        /// - Can have overlays/modals/popups on top
        /// Priority Range: 200-699
        /// </summary>
        Screen = 4,

        /// <summary>
        /// Persistent UI (HUD, Background)
        /// - Always visible unless explicitly hidden
        /// - No history tracking
        /// - Lowest priority
        /// Priority Range: 0-199
        /// </summary>
        Persistent = 5
    }
}
