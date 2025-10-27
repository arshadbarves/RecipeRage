namespace Core.Networking.Common
{
    /// <summary>
    /// Current state of matchmaking
    /// </summary>
    public enum MatchmakingState
    {
        /// <summary>
        /// Not searching for a match
        /// </summary>
        Idle,
        
        /// <summary>
        /// Searching for existing match lobbies
        /// </summary>
        Searching,
        
        /// <summary>
        /// Creating a new match lobby
        /// </summary>
        CreatingLobby,
        
        /// <summary>
        /// Waiting for match lobby to fill
        /// </summary>
        WaitingForPlayers,
        
        /// <summary>
        /// Match found, joining lobby
        /// </summary>
        MatchFound,
        
        /// <summary>
        /// Match lobby full, starting game
        /// </summary>
        Starting,
        
        /// <summary>
        /// Matchmaking cancelled
        /// </summary>
        Cancelled,
        
        /// <summary>
        /// Matchmaking failed
        /// </summary>
        Failed
    }
}
