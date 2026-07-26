using KitchenClash.Application.Services;
using KitchenClash.Infrastructure.Network.Cooking;
using KitchenClash.Infrastructure.Network.Spawning;
using Unity.Netcode;

namespace KitchenClash.Infrastructure.Network
{
    public interface IMatchContext
    {
        void Refresh();
        void ShutdownNetworkSession();
        bool IsHost { get; }
        bool IsServer { get; }
        bool IsClient { get; }
        ulong? LocalClientId { get; }
        int LocalTeamId { get; }

        IKitchenSupportRuntime KitchenSupportRuntime { get; }
        ScoreManager ScoreManager { get; }
        NetworkScoreManager NetworkScoreManager { get; }
        OrderManager OrderManager { get; }
        RoundTimer RoundTimer { get; }
        GamePhaseSync GamePhaseSync { get; }
        MatchResultSync MatchResultSync { get; }
        PlayerController LocalPlayer { get; }
        SpawnManager SpawnManager { get; }
        NetworkManager NetworkManager { get; }
        IngredientNetworkSpawner IngredientNetworkSpawner { get; }

        bool TryGetSpawnedObject(ulong networkObjectId, out NetworkObject networkObject);
    }
}
