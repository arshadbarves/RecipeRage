using Unity.Netcode;

namespace RecipeRage.Net
{
    public struct PlayerRosterEntry : INetworkSerializable, System.IEquatable<PlayerRosterEntry>
    {
        public ulong ClientId;
        public int ChefId;
        public int TeamId;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref ChefId);
            serializer.SerializeValue(ref TeamId);
        }

        public bool Equals(PlayerRosterEntry other) => ClientId == other.ClientId;
    }

    /// <summary>
    /// Who is in the match: client → chef → team. Server assigns teams on spawn
    /// (balanced by join order); clients read for the composition screen.
    /// </summary>
    public sealed class NetworkTeamRoster : NetworkBehaviour
    {
        public readonly NetworkList<PlayerRosterEntry> Players = new NetworkList<PlayerRosterEntry>();

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                return;
            }

            var teamSize = NetworkManager.ConnectedClients.Count / 2;
            var index = 0;
            foreach (var client in NetworkManager.ConnectedClientsList)
            {
                Players.Add(new PlayerRosterEntry
                {
                    ClientId = client.ClientId,
                    ChefId = 0, // chef selection lands in Slice 4 lobby UI
                    TeamId = index < teamSize ? 0 : 1
                });
                index++;
            }
        }

        public int GetTeamFor(ulong clientId)
        {
            foreach (var entry in Players)
            {
                if (entry.ClientId == clientId)
                {
                    return entry.TeamId;
                }
            }
            return 0;
        }
    }
}
