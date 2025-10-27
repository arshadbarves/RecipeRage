namespace UI
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
        Toast,
        
        // Game screens
        Settings,
        Pause,
        Game,
        Menu,
        Profile,
        CharacterSelection,
        GameModeSelection,
        MapSelection,
        Lobby,
        GameOver,
        
        // Background screens (lowest priority)
        HUD,
        Background
    }
}