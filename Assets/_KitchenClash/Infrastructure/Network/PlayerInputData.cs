using Unity.Netcode;
using UnityEngine;

namespace KitchenClash.Infrastructure.Network
{
    public struct PlayerInputData : INetworkSerializable
    {
        public Vector2 Movement;
        public bool InteractPressed;
        public bool AbilityPressed;
        public float Timestamp;
        public uint SequenceNumber;

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
