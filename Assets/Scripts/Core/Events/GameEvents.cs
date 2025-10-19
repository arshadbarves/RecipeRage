using UI.UISystem;

namespace Core.Events
{
    // ============================================
    // CURRENCY EVENTS
    // ============================================

    /// <summary>
    /// Published when currency amounts change
    /// </summary>
    public class CurrencyChangedEvent
    {
        public int Coins { get; set; }
        public int Gems { get; set; }
    }

    /// <summary>
    /// Published when currency is reset (e.g., on logout)
    /// </summary>
    public class CurrencyResetEvent
    {
    }

    // ============================================
    // AUTHENTICATION EVENTS
    // ============================================

    /// <summary>
    /// Published when user successfully logs in
    /// </summary>
    public class LoginSuccessEvent
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
    }

    /// <summary>
    /// Published when login fails
    /// </summary>
    public class LoginFailedEvent
    {
        public string Error { get; set; }
    }

    /// <summary>
    /// Published when user logs out
    /// </summary>
    public class LogoutEvent
    {
        public string UserId { get; set; }
    }

    /// <summary>
    /// Published when login status changes
    /// </summary>
    public class LoginStatusChangedEvent
    {
        public string Status { get; set; }
    }

    // ============================================
    // UI EVENTS
    // ============================================

    /// <summary>
    /// Published when a UI screen is shown
    /// </summary>
    public class ScreenShownEvent
    {
        public UIScreenType ScreenType { get; set; }
    }

    /// <summary>
    /// Published when a UI screen is hidden
    /// </summary>
    public class ScreenHiddenEvent
    {
        public UIScreenType ScreenType { get; set; }
    }

    // ============================================
    // GAME STATE EVENTS
    // ============================================

    /// <summary>
    /// Published when game state changes
    /// </summary>
    public class StateChangedEvent
    {
        public string PreviousState { get; set; }
        public string CurrentState { get; set; }
    }

    // ============================================
    // NETWORK EVENTS
    // ============================================

    /// <summary>
    /// Published when player joins a lobby
    /// </summary>
    public class LobbyJoinedEvent
    {
        public string LobbyId { get; set; }
    }

    /// <summary>
    /// Published when player leaves a lobby
    /// </summary>
    public class LobbyLeftEvent
    {
        public string LobbyId { get; set; }
    }

    // ============================================
    // AUDIO EVENTS
    // ============================================

    /// <summary>
    /// Published when audio settings change
    /// </summary>
    public class AudioSettingsChangedEvent
    {
        public float MasterVolume { get; set; }
        public float MusicVolume { get; set; }
        public float SFXVolume { get; set; }
        public bool IsMuted { get; set; }
    }
}
