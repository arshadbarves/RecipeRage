using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Characters
{
    /// <summary>
    /// Represents the authoritative state of a player at a specific point in time.
    /// Used for server reconciliation in client-side prediction.
    /// </summary>
    public struct PlayerStateData : INetworkSerializable
    {
        /// <summary>
        /// Player position.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Player rotation.
        /// </summary>
        public Quaternion Rotation;

        /// <summary>
        /// Player velocity.
        /// </summary>
        public Vector3 Velocity;

        /// <summary>
        /// Server timestamp for this state.
        /// </summary>
        public float Timestamp;

        /// <summary>
        /// Sequence number of the input that produced this state.
        /// </summary>
        public uint SequenceNumber;

        /// <summary>
        /// Serialize the state data for network transmission.
        /// </summary>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref Rotation);
            serializer.SerializeValue(ref Velocity);
            serializer.SerializeValue(ref Timestamp);
            serializer.SerializeValue(ref SequenceNumber);
        }
    }
}
