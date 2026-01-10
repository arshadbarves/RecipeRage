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
        /// Game starter service for Unity Netcode integration
        /// </summary>
        IGameStarter GameStarter { get; }
        
        /// <summary>
        /// Bot spawner service for spawning AI bots
        /// </summary>
        IBotSpawner BotSpawner { get; }
        
        /// <summary>
        /// Friends service for EOS friends management
        /// </summary>
        IFriendsService FriendsService { get; }
        
        // P2P networking now handled by Unity Netcode + EOSTransport
    }
}
