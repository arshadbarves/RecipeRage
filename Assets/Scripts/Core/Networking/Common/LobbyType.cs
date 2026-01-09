namespace Core.Core.Networking.Common
{
    /// <summary>
    /// Types of lobbies in the game
    /// </summary>
    public enum LobbyType
    {
        /// <summary>
        /// Party lobby - persistent squad lobby for friends
        /// Survives across multiple matches
        /// </summary>
        Party,

        /// <summary>
        /// Match lobby - temporary lobby for full game
        /// Created by matchmaking, disbanded after match
        /// </summary>
        Match
    }
}
