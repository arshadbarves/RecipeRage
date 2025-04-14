using System;

namespace RecipeRage.Core.Networking
{
    /// <summary>
    /// Represents a network message.
    /// </summary>
    public class NetworkMessage
    {
        /// <summary>
        /// The type of message.
        /// </summary>
        public byte MessageType { get; set; }
        
        /// <summary>
        /// The message data.
        /// </summary>
        public byte[] Data { get; set; }
        
        /// <summary>
        /// The ID of the sender.
        /// </summary>
        public string SenderId { get; set; }
        
        /// <summary>
        /// The sender of the message.
        /// </summary>
        public NetworkPlayer Sender { get; set; }
    }
}
