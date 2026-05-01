using Unity.Netcode;
using UnityEngine;

namespace KitchenClash.Infrastructure.Network
{
    public struct PlayerStateData : INetworkSerializable
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public float Timestamp;
        public uint SequenceNumber;

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
