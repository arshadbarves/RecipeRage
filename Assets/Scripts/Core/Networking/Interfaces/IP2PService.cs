using System;
using Core.Networking.Common;
using Epic.OnlineServices;

namespace Core.Networking.Interfaces
{
    /// <summary>
    /// Interface for P2P networking operations
    /// Handles in-game communication between players
    /// </summary>
    public interface IP2PService
    {
        #region Events
        
        /// <summary>
        /// Fired when a player action is received
        /// </summary>
        event Action<PlayerInfo, PlayerAction> OnPlayerActionReceived;
        
        /// <summary>
        /// Fired when a chat message is received
        /// </summary>
        event Action<PlayerInfo, string> OnChatMessageReceived;
        
        /// <summary>
        /// Fired when an emote is received
        /// </summary>
        event Action<PlayerInfo, int> OnEmoteReceived;
        
        /// <summary>
        /// Fired when game state update is received
        /// </summary>
        event Action<byte[]> OnGameStateReceived;
        
        /// <summary>
        /// Fired when connection to a player is established
        /// </summary>
        event Action<ProductUserId> OnPlayerConnected;
        
        /// <summary>
        /// Fired when connection to a player is lost
        /// </summary>
        event Action<ProductUserId> OnPlayerDisconnected;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Whether P2P is initialized
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Whether the local player is the host
        /// </summary>
        bool IsHost { get; }
        
        #endregion
        
        #region Methods
        
        /// <summary>
        /// Initialize the P2P service
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Start hosting a P2P session
        /// </summary>
        void StartHosting();
        
        /// <summary>
        /// Connect to a host
        /// </summary>
        /// <param name="hostId">Host's product user ID</param>
        void ConnectToHost(ProductUserId hostId);
        
        /// <summary>
        /// Disconnect from P2P session
        /// </summary>
        void Disconnect();
        
        /// <summary>
        /// Send a player action to all players
        /// </summary>
        /// <param name="action">Player action to send</param>
        void SendPlayerAction(PlayerAction action);
        
        /// <summary>
        /// Send a chat message to all players
        /// </summary>
        /// <param name="message">Chat message</param>
        void SendChatMessage(string message);
        
        /// <summary>
        /// Send an emote to all players
        /// </summary>
        /// <param name="emoteId">Emote ID</param>
        void SendEmote(int emoteId);
        
        /// <summary>
        /// Send game state to all players (host only)
        /// </summary>
        /// <param name="data">Game state data</param>
        void SendGameState(byte[] data);
        
        /// <summary>
        /// Send data to a specific player
        /// </summary>
        /// <param name="targetPlayer">Target player</param>
        /// <param name="messageType">Message type</param>
        /// <param name="data">Message data</param>
        void SendToPlayer(PlayerInfo targetPlayer, byte messageType, byte[] data);
        
        /// <summary>
        /// Send data to all players
        /// </summary>
        /// <param name="messageType">Message type</param>
        /// <param name="data">Message data</param>
        void SendToAll(byte messageType, byte[] data);
        
        #endregion
    }
}
