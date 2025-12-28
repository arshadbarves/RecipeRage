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
        NoInternet,
        FriendsPopup,
        UsernamePopup,

        // Game screens
        Settings,
        Pause,
        Game,
        MainMenu,
        Profile,
        CharacterSelection,
        GameModeSelection,
        MapSelection,
        Matchmaking,
        Lobby,
        GameOver,

        // Special screens
        HUDEditor,

        // Background screens (lowest priority)
        HUD,
        Background
    }
}