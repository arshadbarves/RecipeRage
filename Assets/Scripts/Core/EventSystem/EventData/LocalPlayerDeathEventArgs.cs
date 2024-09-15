using Unity.Netcode;

namespace Core.EventSystem.EventData
{
    public class LocalPlayerDeathEventArgs : INetworkEventArgs
    {
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            throw new System.NotImplementedException();
        }

        public void Serialize(FastBufferWriter writer)
        {
            throw new System.NotImplementedException();
        }

        public void Deserialize(FastBufferReader reader)
        {
            throw new System.NotImplementedException();
        }
    }
}