using System;
using Core.Networking.Interfaces;

namespace Core.Networking
{
    /// <summary>
    /// Interface for all networking services
    /// Central access point for all networking functionality
    /// </summary>
    public interface INetworkingServices : IDisposable
    {
        /// <summary>
        /// Lobby management service (Party and Match lobbies)
        /// </summary>
        ILobbyManager LobbyManager { get; }
        
        /// <summary>
        /// Player management service
        /// </summary>
        IPlayerManager PlayerManager { get; }
        
        /// <summary>
        /// Matchmaking service
        /// </summary>
        IMatchmakingService MatchmakingService { get; }
        
        /// <summary>
        /// Team management service
        /// </summary>
        ITeamManager TeamManager { get; }
        
        /// <summary>
        /// P2P networking service
        /// </summary>
        IP2PService P2PService { get; }
    }
}
