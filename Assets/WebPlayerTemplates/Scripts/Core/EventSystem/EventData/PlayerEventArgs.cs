using Unity.Netcode;

namespace Core.EventSystem.EventData
{
    [EventType("PlayerDamage")]
    public class PlayerDamageEventArgs : INetworkEventArgs
    {
        public ulong PlayerId { get; set; }
        public float DamageAmount { get; set; }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            ulong playerId = PlayerId;
            float damageAmount = DamageAmount;
            serializer.SerializeValue(ref playerId);
            serializer.SerializeValue(ref damageAmount);
            PlayerId = playerId;
            DamageAmount = damageAmount;
        }

        public void Serialize(FastBufferWriter writer)
        {
            writer.WriteValueSafe(PlayerId);
            writer.WriteValueSafe(DamageAmount);
        }

        public void Deserialize(FastBufferReader reader)
        {
            reader.ReadValueSafe(out ulong playerId);
            reader.ReadValueSafe(out float damageAmount);
            PlayerId = playerId;
            DamageAmount = damageAmount;
        }
    }
}