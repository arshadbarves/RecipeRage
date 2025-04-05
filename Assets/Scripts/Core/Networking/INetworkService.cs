using System;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeRage.Core.Networking
{
    /// <summary>
    /// Interface for network services that handle multiplayer functionality.
    /// </summary>
    public interface INetworkService
    {
        /// <summary>
        /// Event triggered when the connection state changes.
        /// </summary>
        event Action<NetworkConnectionState> OnConnectionStateChanged;
        
        /// <summary>
        /// Event triggered when a player joins the session.
        /// </summary>
        event Action<NetworkPlayer> OnPlayerJoined;
        
        /// <summary>
        /// Event triggered when a player leaves the session.
        /// </summary>
        event Action<NetworkPlayer> OnPlayerLeft;
        
        /// <summary>
        /// Event triggered when the session is created.
        /// </summary>
        event Action<bool, string> OnSessionCreated;
        
        /// <summary>
        /// Event triggered when the session is joined.
        /// </summary>
        event Action<bool, string> OnSessionJoined;
        
        /// <summary>
        /// The current connection state.
        /// </summary>
        NetworkConnectionState ConnectionState { get; }
        
        /// <summary>
        /// The local player.
        /// </summary>
        NetworkPlayer LocalPlayer { get; }
        
        /// <summary>
        /// The list of connected players.
        /// </summary>
        IReadOnlyList<NetworkPlayer> ConnectedPlayers { get; }
        
        /// <summary>
        /// Initialize the network service.
        /// </summary>
        /// <param name="callback">Callback when initialization is complete</param>
        void Initialize(Action<bool> callback);
        
        /// <summary>
        /// Shutdown the network service.
        /// </summary>
        void Shutdown();
        
        /// <summary>
        /// Create a new session.
        /// </summary>
        /// <param name="sessionName">Name of the session</param>
        /// <param name="maxPlayers">Maximum number of players</param>
        /// <param name="isPrivate">Whether the session is private</param>
        void CreateSession(string sessionName, int maxPlayers, bool isPrivate);
        
        /// <summary>
        /// Join an existing session.
        /// </summary>
        /// <param name="sessionId">ID of the session to join</param>
        void JoinSession(string sessionId);
        
        /// <summary>
        /// Join an existing session by invite.
        /// </summary>
        /// <param name="inviteToken">Invite token</param>
        void JoinSessionByInvite(string inviteToken);
        
        /// <summary>
        /// Leave the current session.
        /// </summary>
        void LeaveSession();
        
        /// <summary>
        /// Start the session.
        /// </summary>
        void StartSession();
        
        /// <summary>
        /// Find available sessions.
        /// </summary>
        /// <param name="callback">Callback with list of sessions</param>
        void FindSessions(Action<List<NetworkSessionInfo>> callback);
        
        /// <summary>
        /// Send data to all connected players.
        /// </summary>
        /// <param name="data">Data to send</param>
        /// <param name="reliable">Whether the data should be sent reliably</param>
        void SendToAll(byte[] data, bool reliable);
        
        /// <summary>
        /// Send data to a specific player.
        /// </summary>
        /// <param name="playerId">ID of the player to send to</param>
        /// <param name="data">Data to send</param>
        /// <param name="reliable">Whether the data should be sent reliably</param>
        void SendToPlayer(string playerId, byte[] data, bool reliable);
        
        /// <summary>
        /// Register a message handler for a specific message type.
        /// </summary>
        /// <param name="messageType">Type of message to handle</param>
        /// <param name="handler">Handler function</param>
        void RegisterMessageHandler(byte messageType, Action<NetworkMessage> handler);
        
        /// <summary>
        /// Unregister a message handler.
        /// </summary>
        /// <param name="messageType">Type of message to unregister</param>
        void UnregisterMessageHandler(byte messageType);
    }
}
