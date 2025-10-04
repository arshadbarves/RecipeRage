namespace UI.UISystem
{
    /// <summary>
    /// Defines different types of UI screens for categorization and behavior
    /// </summary>
    public enum UIScreenType
    {
        // System screens (highest priority)
        Splash,
        Loading,
        
        // Modal and overlay screens
        Modal,
        Popup,
        Notification,
        
        // Game screens
        Settings,
        Pause,
        Game,
        Menu,
        CharacterSelection,
        GameModeSelection,
        Lobby,
        GameOver,
        
        // Background screens (lowest priority)
        HUD,
        Background
    }
}