using Gameplay.Cooking;
using Gameplay.GameModes;
using Gameplay.Scoring;
using Gameplay.Spawning;
using Gameplay.UI;

namespace Gameplay.Shared
{
    /// <summary>
    /// Mutable bridge used by gameplay scenes to publish live match objects into the app-scoped match context.
    /// </summary>
    public interface IMatchRuntimeRegistry
    {
        void RegisterSceneRuntime(
            OrderManager orderManager,
            ScoreManager scoreManager,
            GamePhaseSync gamePhaseSync,
            MatchResultSync matchResultSync,
            RoundTimer roundTimer,
            NetworkScoreManager networkScoreManager,
            MobileControlsManager mobileControlsManager,
            SpawnManager spawnManager,
            IngredientNetworkSpawner ingredientNetworkSpawner,
            IBotKitchenRuntime botKitchenRuntime,
            IKitchenSupportRuntime kitchenSupportRuntime);

        void ClearSceneRuntime();
    }
}
