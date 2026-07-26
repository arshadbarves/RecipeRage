using Playcenter;

namespace RecipeRage.Net
{
    /// <summary>
    /// Active match. Loads the daily-rotation map additively; unloads on exit.
    /// Map selection: config key current_map (daily rotation, Slice 5 map set).
    /// </summary>
    public sealed class MatchRuntimeState : IGameState
    {
        public void Enter()
        {
            ServiceLocator.Get<ILoggingService>().Log("[Flow] Match started");
            // Scene load via ISceneLoader when maps exist (Slice 5).
            // Match ticking lives in NetworkMatch (server) / MatchController (offline).
        }

        public void Exit() { }
        public void Update(float deltaTime) { }
    }
}
