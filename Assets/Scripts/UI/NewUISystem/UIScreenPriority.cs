namespace UI.UISystem
{
    /// <summary>
    /// Defines UI screen priorities for proper sorting order
    /// Higher values appear on top of lower values
    /// </summary>
    public enum UIScreenPriority
    {
        Background = 0,      // Background elements
        HUD = 100,          // Game HUD elements  
        Menu = 200,         // Menu screens
        Game = 300,         // Game UI
        Settings = 400,     // Settings screens
        Pause = 500,        // Pause menu
        Notification = 600, // Notifications
        Popup = 700,        // Popup dialogs
        Modal = 800,        // Modal dialogs
        Loading = 900,      // Loading screens
        Splash = 1000       // Splash screens (highest)
    }
}