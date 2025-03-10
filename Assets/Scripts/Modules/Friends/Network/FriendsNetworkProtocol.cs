using System;
using System.Text;
using UnityEngine;

namespace RecipeRage.Modules.Friends.Network
{
    /// <summary>
    /// Message types for the friends network protocol
    /// 
    /// Complexity Rating: 1
    /// </summary>
    public enum FriendsMessageType : byte
    {
        Ping = 0,
        Pong = 1,
        PresenceUpdate = 10,
        FriendRequest = 20,
        FriendAccept = 21,
        FriendReject = 22,
        FriendRemove = 23,
        ChatMessage = 30,
        GameInvite = 40
    }

    /// <summary>
    /// Protocol specification for the friends network system
    /// Handles serialization and deserialization of network messages
    /// 
    /// Complexity Rating: 3
    /// </summary>
    public static class FriendsNetworkProtocol
    {
        // Protocol version - increment when making breaking changes
        public const byte PROTOCOL_VERSION = 1;
        
        // Maximum message size in bytes
        public const int MAX_MESSAGE_SIZE = 1024;
        
        // Header size: 1 byte version + 1 byte message type + 2 bytes payload length
        public const int HEADER_SIZE = 4;
        
        /// <summary>
        /// Create a message packet with header and payload
        /// </summary>
        /// <param name="messageType">Type of message</param>
        /// <param name="payload">Message payload</param>
        /// <returns>Serialized message packet</returns>
        public static byte[] CreatePacket(FriendsMessageType messageType, byte[] payload = null)
        {
            payload = payload ?? new byte[0];
            
            if (payload.Length > MAX_MESSAGE_SIZE - HEADER_SIZE)
            {
                Debug.LogError($"Payload size ({payload.Length}) exceeds maximum ({MAX_MESSAGE_SIZE - HEADER_SIZE})");
                return null;
            }
            
            byte[] packet = new byte[HEADER_SIZE + payload.Length];
            
            // Write header
            packet[0] = PROTOCOL_VERSION;
            packet[1] = (byte)messageType;
            packet[2] = (byte)(payload.Length & 0xFF);
            packet[3] = (byte)((payload.Length >> 8) & 0xFF);
            
            // Copy payload
            if (payload.Length > 0)
            {
                Buffer.BlockCopy(payload, 0, packet, HEADER_SIZE, payload.Length);
            }
            
            return packet;
        }
        
        /// <summary>
        /// Parse a received packet
        /// </summary>
        /// <param name="data">Raw packet data</param>
        /// <param name="messageType">Output message type</param>
        /// <param name="payload">Output payload</param>
        /// <returns>True if parsing was successful</returns>
        public static bool ParsePacket(byte[] data, out FriendsMessageType messageType, out byte[] payload)
        {
            messageType = FriendsMessageType.Ping;
            payload = null;
            
            if (data == null || data.Length < HEADER_SIZE)
            {
                Debug.LogError("Invalid packet: too small");
                return false;
            }
            
            byte version = data[0];
            if (version != PROTOCOL_VERSION)
            {
                Debug.LogError($"Unsupported protocol version: {version}");
                return false;
            }
            
            messageType = (FriendsMessageType)data[1];
            
            int payloadLength = (data[2] & 0xFF) | ((data[3] & 0xFF) << 8);
            if (payloadLength != data.Length - HEADER_SIZE)
            {
                Debug.LogError($"Invalid payload length: expected {payloadLength}, got {data.Length - HEADER_SIZE}");
                return false;
            }
            
            payload = new byte[payloadLength];
            if (payloadLength > 0)
            {
                Buffer.BlockCopy(data, HEADER_SIZE, payload, 0, payloadLength);
            }
            
            return true;
        }
        
        /// <summary>
        /// Create a ping message
        /// </summary>
        /// <returns>Serialized ping packet</returns>
        public static byte[] CreatePing()
        {
            return CreatePacket(FriendsMessageType.Ping);
        }
        
        /// <summary>
        /// Create a pong message (response to ping)
        /// </summary>
        /// <returns>Serialized pong packet</returns>
        public static byte[] CreatePong()
        {
            return CreatePacket(FriendsMessageType.Pong);
        }
        
        /// <summary>
        /// Create a presence update message
        /// </summary>
        /// <param name="statusJson">JSON-serialized presence data</param>
        /// <returns>Serialized presence update packet</returns>
        public static byte[] CreatePresenceUpdate(string statusJson)
        {
            byte[] payload = Encoding.UTF8.GetBytes(statusJson);
            return CreatePacket(FriendsMessageType.PresenceUpdate, payload);
        }
        
        /// <summary>
        /// Create a friend request message
        /// </summary>
        /// <param name="requestJson">JSON-serialized friend request data</param>
        /// <returns>Serialized friend request packet</returns>
        public static byte[] CreateFriendRequest(string requestJson)
        {
            byte[] payload = Encoding.UTF8.GetBytes(requestJson);
            return CreatePacket(FriendsMessageType.FriendRequest, payload);
        }
        
        /// <summary>
        /// Create a friend accept message
        /// </summary>
        /// <param name="requestId">ID of the friend request being accepted</param>
        /// <returns>Serialized friend accept packet</returns>
        public static byte[] CreateFriendAccept(string requestId)
        {
            byte[] payload = Encoding.UTF8.GetBytes(requestId);
            return CreatePacket(FriendsMessageType.FriendAccept, payload);
        }
        
        /// <summary>
        /// Create a friend reject message
        /// </summary>
        /// <param name="requestId">ID of the friend request being rejected</param>
        /// <returns>Serialized friend reject packet</returns>
        public static byte[] CreateFriendReject(string requestId)
        {
            byte[] payload = Encoding.UTF8.GetBytes(requestId);
            return CreatePacket(FriendsMessageType.FriendReject, payload);
        }
        
        /// <summary>
        /// Create a friend remove message
        /// </summary>
        /// <param name="userId">ID of the friend being removed</param>
        /// <returns>Serialized friend remove packet</returns>
        public static byte[] CreateFriendRemove(string userId)
        {
            byte[] payload = Encoding.UTF8.GetBytes(userId);
            return CreatePacket(FriendsMessageType.FriendRemove, payload);
        }
        
        /// <summary>
        /// Create a chat message
        /// </summary>
        /// <param name="messageJson">JSON-serialized chat message</param>
        /// <returns>Serialized chat message packet</returns>
        public static byte[] CreateChatMessage(string messageJson)
        {
            byte[] payload = Encoding.UTF8.GetBytes(messageJson);
            return CreatePacket(FriendsMessageType.ChatMessage, payload);
        }
        
        /// <summary>
        /// Create a game invite message
        /// </summary>
        /// <param name="inviteJson">JSON-serialized game invite data</param>
        /// <returns>Serialized game invite packet</returns>
        public static byte[] CreateGameInvite(string inviteJson)
        {
            byte[] payload = Encoding.UTF8.GetBytes(inviteJson);
            return CreatePacket(FriendsMessageType.GameInvite, payload);
        }
        
        /// <summary>
        /// Extract string payload from a message
        /// </summary>
        /// <param name="payload">Binary payload</param>
        /// <returns>UTF-8 decoded string</returns>
        public static string GetStringPayload(byte[] payload)
        {
            if (payload == null || payload.Length == 0)
            {
                return string.Empty;
            }
            
            return Encoding.UTF8.GetString(payload);
        }
    }
} 