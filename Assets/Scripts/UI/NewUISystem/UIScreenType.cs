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
        Login,
        Maintenance,
        
        // Modal and overlay screens
        Modal,
        Popup,
        Notification,
        
        // Game screens
        Settings,
        Pause,
        Game,
        Menu,
        Profile,
        CharacterSelection,
        GameModeSelection,
        Lobby,
        GameOver,
        
        // Background screens (lowest priority)
        HUD,
        Background
    }
}