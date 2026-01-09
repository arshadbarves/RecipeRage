namespace Core.Networking.Common
{
    /// <summary>
    /// Current state of a lobby
    /// </summary>
    public enum LobbyState
    {
        /// <summary>
        /// No active lobby
        /// </summary>
        Idle,

        /// <summary>
        /// In party lobby (pre-matchmaking)
        /// </summary>
        InParty,

        /// <summary>
        /// Searching for match
        /// </summary>
        Matchmaking,

        /// <summary>
        /// In match lobby (full game lobby)
        /// </summary>
        InMatchLobby,

        /// <summary>
        /// Match is starting
        /// </summary>
        Starting,

        /// <summary>
        /// In active game session
        /// </summary>
        InGame,

        /// <summary>
        /// Match ended, transitioning back
        /// </summary>
        PostGame
    }
}
