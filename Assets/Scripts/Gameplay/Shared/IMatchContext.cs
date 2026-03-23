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
        PlayerController LocalPlayer { get; }
        NetworkScoreManager NetworkScoreManager { get; }
        RoundTimer RoundTimer { get; }
        GamePhaseSync GamePhaseSync { get; }
        OrderManager OrderManager { get; }
        ScoreManager ScoreManager { get; }
        MobileControlsManager MobileControlsManager { get; }
        SpawnManager SpawnManager { get; }
        bool IsHost { get; }
        bool IsServer { get; }
        bool IsClient { get; }

        void Refresh();
        void ShutdownNetworkSession();
    }
}
