using Unity.Netcode;

namespace Core.GameFramework.Time
{
    public struct NetworkTimeData : INetworkSerializable
    {
        public double ServerTime;
        public double ClientTime;
        public float Latency;
        public float TimeScale;
        public bool IsPaused;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ServerTime);
            serializer.SerializeValue(ref ClientTime);
            serializer.SerializeValue(ref Latency);
            serializer.SerializeValue(ref TimeScale);
            serializer.SerializeValue(ref IsPaused);
        }
    }
}