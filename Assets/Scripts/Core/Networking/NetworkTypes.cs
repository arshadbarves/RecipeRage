using System;
using System.Collections.Generic;

namespace RecipeRage.Core.Networking
{
    /// <summary>
    /// Enum for network connection states.
    /// </summary>
    public enum NetworkConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Disconnecting,
        Failed
    }

    /// <summary>
    /// Class representing a network player.
    /// </summary>
    public class NetworkPlayer
    {
        /// <summary>
        /// The player's unique ID.
        /// </summary>
        public string PlayerId { get; set; }

        /// <summary>
        /// The player's display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Whether this player is the local player.
        /// </summary>
        public bool IsLocal { get; set; }

        /// <summary>
        /// Whether this player is the host.
        /// </summary>
        public bool IsHost { get; set; }

        /// <summary>
        /// The player's team ID.
        /// </summary>
        public int TeamId { get; set; }

        /// <summary>
        /// The player's character type.
        /// </summary>
        public int CharacterType { get; set; }

        /// <summary>
        /// Whether the player is ready to start the game.
        /// </summary>
        public bool IsReady { get; set; }

        /// <summary>
        /// Custom data associated with the player.
        /// </summary>
        public Dictionary<string, string> CustomData { get; set; }

        /// <summary>
        /// Create a new network player.
        /// </summary>
        public NetworkPlayer()
        {
            PlayerId = string.Empty;
            DisplayName = "Player";
            IsLocal = false;
            IsHost = false;
            TeamId = 0;
            CharacterType = 0;
            IsReady = false;
            CustomData = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Class representing a network session.
    /// </summary>
    public class NetworkSessionInfo
    {
        /// <summary>
        /// The session's unique ID.
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// The session's name.
        /// </summary>
        public string SessionName { get; set; }

        /// <summary>
        /// The number of players in the session.
        /// </summary>
        public int PlayerCount { get; set; }

        /// <summary>
        /// The maximum number of players allowed in the session.
        /// </summary>
        public int MaxPlayers { get; set; }

        /// <summary>
        /// Whether the session is private.
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// The host's display name.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// The session's game mode.
        /// </summary>
        public string GameMode { get; set; }

        /// <summary>
        /// The session's map name.
        /// </summary>
        public string MapName { get; set; }

        /// <summary>
        /// Custom data associated with the session.
        /// </summary>
        public Dictionary<string, string> CustomData { get; set; }

        /// <summary>
        /// Create a new network session info.
        /// </summary>
        public NetworkSessionInfo()
        {
            CustomData = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Class representing a network message.
    /// </summary>
    public class NetworkMessage
    {
        /// <summary>
        /// The message type.
        /// </summary>
        public byte MessageType { get; set; }

        /// <summary>
        /// The sender's ID.
        /// </summary>
        public string SenderId { get; set; }

        /// <summary>
        /// The message data.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// The timestamp when the message was sent.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Create a new network message.
        /// </summary>
        public NetworkMessage()
        {
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Create a new network message with the specified parameters.
        /// </summary>
        /// <param name="messageType">The message type</param>
        /// <param name="senderId">The sender's ID</param>
        /// <param name="data">The message data</param>
        public NetworkMessage(byte messageType, string senderId, byte[] data)
        {
            MessageType = messageType;
            SenderId = senderId;
            Data = data;
            Timestamp = DateTime.UtcNow;
        }
    }
}
