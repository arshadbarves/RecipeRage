using System;
using Core.Networking.Interfaces;
using Core.Networking.Services;

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
        /// Game starter service for Unity Netcode integration
        /// </summary>
        GameStarter GameStarter { get; }
        
        /// <summary>
        /// Friends service for EOS friends management
        /// </summary>
        IFriendsService FriendsService { get; }
        
        // P2P networking now handled by Unity Netcode + EOSTransport
    }
}
