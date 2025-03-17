using System;
using System.Collections.Generic;

namespace RecipeRage.Modules.Friends.Interfaces
{
    /// <summary>
    /// Interface for the P2P network service.
    /// Manages peer-to-peer connections using EOS.
    /// Complexity Rating: 3
    /// </summary>
    public interface IP2PNetworkService
    {
        /// <summary>
        /// Event triggered when a message is received from a peer
        /// </summary>
        event Action<string, byte[]> OnMessageReceived;

        /// <summary>
        /// Event triggered when a connection status changes
        /// </summary>
        event Action<string, bool> OnConnectionStatusChanged;

        /// <summary>
        /// Initialize the P2P network service
        /// </summary>
        /// <param name="onComplete"> Callback when initialization is complete </param>
        void Initialize(Action<bool> onComplete = null);

        /// <summary>
        /// Connect to a peer
        /// </summary>
        /// <param name="peerId"> ID of the peer to connect to </param>
        /// <param name="onComplete"> Callback when the connection attempt is complete </param>
        void Connect(string peerId, Action<bool> onComplete = null);

        /// <summary>
        /// Disconnect from a peer
        /// </summary>
        /// <param name="peerId"> ID of the peer to disconnect from </param>
        void Disconnect(string peerId);

        /// <summary>
        /// Send a message to a peer
        /// </summary>
        /// <param name="peerId"> ID of the peer to send the message to </param>
        /// <param name="data"> Message data </param>
        /// <param name="reliable"> Whether the message should be sent reliably </param>
        /// <param name="onComplete"> Callback when the send operation is complete </param>
        void SendMessage(string peerId, byte[] data, bool reliable = true, Action<bool> onComplete = null);

        /// <summary>
        /// Check if connected to a peer
        /// </summary>
        /// <param name="peerId"> ID of the peer </param>
        /// <returns> True if connected to the peer </returns>
        bool IsConnectedTo(string peerId);

        /// <summary>
        /// Get a list of connected peers
        /// </summary>
        /// <returns> List of connected peer IDs </returns>
        IReadOnlyList<string> GetConnectedPeers();

        /// <summary>
        /// Get the NAT type for the current user
        /// </summary>
        /// <returns> NAT type string (Open, Moderate, Strict) </returns>
        string GetNATType();

        /// <summary>
        /// Close all connections and clean up
        /// </summary>
        void Shutdown();
    }
}