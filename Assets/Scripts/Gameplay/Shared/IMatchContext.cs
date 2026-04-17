using Gameplay.Characters;
using Gameplay.Cooking;
using Gameplay.GameModes;
using Gameplay.Scoring;
using Gameplay.Spawning;
using Gameplay.UI;
using Unity.Netcode;

namespace Gameplay.Shared
{
    /// <summary>
    /// Read-only gateway to match-scoped runtime objects needed by UI and app states.
    /// </summary>
    public interface IMatchContext
    {
        NetworkManager NetworkManager { get; }
        ulong? LocalClientId { get; }
        int? LocalTeamId { get; }
        PlayerController LocalPlayer { get; }
        NetworkScoreManager NetworkScoreManager { get; }
        RoundTimer RoundTimer { get; }
        GamePhaseSync GamePhaseSync { get; }
        MatchResultSync MatchResultSync { get; }
        OrderManager OrderManager { get; }
        ScoreManager ScoreManager { get; }
        MobileControlsManager MobileControlsManager { get; }
        SpawnManager SpawnManager { get; }
        IngredientNetworkSpawner IngredientNetworkSpawner { get; }
        IBotKitchenRuntime BotKitchenRuntime { get; }
        IKitchenSupportRuntime KitchenSupportRuntime { get; }
        bool IsHost { get; }
        bool IsServer { get; }
        bool IsClient { get; }

        void Refresh();
        void ShutdownNetworkSession();
        bool TryGetSpawnedObject(ulong networkObjectId, out NetworkObject networkObject);
    }
}
