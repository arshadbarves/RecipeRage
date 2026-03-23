namespace Core.UI
{
    /// <summary>
    /// Defines app-shell UI layers and navigation categories.
    /// Categories now describe explicit placement in the UI shell rather than
    /// implicitly driving all routing behavior.
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
        /// - Can stack
        /// Priority Range: 900-999
        /// </summary>
        Overlay = 1,

        /// <summary>
        /// Modal dialogs (Confirmation, Error dialogs)
        /// - Blocks interaction with screens below
        /// - Multiple modals can stack
        /// Priority Range: 800-899
        /// </summary>
        Modal = 2,

        /// <summary>
        /// Popup windows (Friends list, Username entry)
        /// - Multiple popups can coexist
        /// Priority Range: 700-799
        /// </summary>
        Popup = 3,

        /// <summary>
        /// Full-screen app screens (MainMenu, Matchmaking, nested feature screens)
        /// - Supports explicit root replacement and screen pushing
        /// Priority Range: 200-699
        /// </summary>
        Screen = 4,

        /// <summary>
        /// Gameplay HUD and always-on game-layer UI
        /// - No history tracking
        /// Priority Range: 100-199
        /// </summary>
        HUD = 5,

        /// <summary>
        /// Toasts and transient system notices
        /// - No history tracking
        /// - Always rendered on the top-most shell layer
        /// Priority Range: 1100+
        /// </summary>
        Toast = 6
    }
}
