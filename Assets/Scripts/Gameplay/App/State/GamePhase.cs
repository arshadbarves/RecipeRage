namespace Gameplay.App.State
{
    /// <summary>
    /// Game phases for network synchronization.
    /// Used by NetworkGameStateManager and UI systems.
    /// </summary>
    public enum GamePhase
    {
        Waiting,      // Waiting for players (not in preparation or playing)
        Preparation,  // Countdown before game starts
        Playing,      // Active gameplay
        Results       // Game over, showing scores
    }
}
