using Core.GameFramework.Event.Core;
using Unity.Netcode;

namespace Gameplay.Character
{
    public class PlayerSpawnedEvent : INetworkedGameEvent
    {
        public PlayerSpawnedEvent(ulong clientId)
        {
            ClientId = clientId;
        }
        public ulong ClientId { get; }

        public bool IsNetworked => true;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            ulong clientId = ClientId;
            serializer.SerializeValue(ref clientId);
        }
    }

    public class PlayerDisconnectedEvent : IGameEvent
    {
        public PlayerDisconnectedEvent(ulong clientId)
        {
            ClientId = clientId;
        }
        public ulong ClientId { get; }

        public bool IsNetworked => false;
    }

    public class PlayerReconnectedEvent : IGameEvent
    {
        public PlayerReconnectedEvent(ulong clientId)
        {
            ClientId = clientId;
        }
        public ulong ClientId { get; }

        public bool IsNetworked => false;
    }

    public class BotTakeoverEvent : INetworkedGameEvent
    {
        public BotTakeoverEvent(ulong replacedPlayerId)
        {
            ReplacedPlayerId = replacedPlayerId;
        }
        public ulong ReplacedPlayerId { get; }

        public bool IsNetworked => true;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            ulong replacedPlayerId = ReplacedPlayerId;
            serializer.SerializeValue(ref replacedPlayerId);
        }
    }

    public class TeamScoreUpdatedEvent : INetworkedGameEvent
    {

        public TeamScoreUpdatedEvent(ushort teamId, int newScore)
        {
            TeamId = teamId;
            NewScore = newScore;
        }

        public ushort TeamId { get; }

        public int NewScore { get; }

        public bool IsNetworked => true;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            ushort teamId = TeamId;
            int newScore = NewScore;
            serializer.SerializeValue(ref teamId);
            serializer.SerializeValue(ref newScore);
        }
    }
}