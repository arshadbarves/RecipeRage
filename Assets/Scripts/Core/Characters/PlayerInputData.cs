using Unity.Netcode;
using UnityEngine;

namespace Core.Characters
{
    /// <summary>
    /// Represents a single frame of player input for network prediction.
    /// Used for client-side prediction and server reconciliation.
    /// </summary>
    public struct PlayerInputData : INetworkSerializable
    {
        /// <summary>
        /// Movement input vector (normalized).
        /// </summary>
        public Vector2 Movement;

        /// <summary>
        /// Whether the interact button was pressed this frame.
        /// </summary>
        public bool InteractPressed;

        /// <summary>
        /// Whether the ability button was pressed this frame.
        /// </summary>
        public bool AbilityPressed;

        /// <summary>
        /// Server timestamp when this input was received.
        /// </summary>
        public float Timestamp;

        /// <summary>
        /// Sequence number for this input (increments each frame).
        /// Used for reconciliation.
        /// </summary>
        public uint SequenceNumber;

        /// <summary>
        /// Serialize the input data for network transmission.
        /// </summary>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Movement);
            serializer.SerializeValue(ref InteractPressed);
            serializer.SerializeValue(ref AbilityPressed);
            serializer.SerializeValue(ref Timestamp);
            serializer.SerializeValue(ref SequenceNumber);
        }
    }
}
