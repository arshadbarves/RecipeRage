using Playcenter;

namespace RecipeRage.Net
{
    /// <summary>
    /// Active match. Loads the daily-rotation map additively; unloads on exit.
    /// Map selection: config key current_map (daily rotation, Slice 5 map set).
    /// </summary>
    public sealed class MatchRuntimeState : IGameState
    {
        private string _loadedMapScene;

        public async void Enter()
        {
            ServiceLocator.Get<ILoggingService>().Log("[Flow] Match started");

            // Load the daily-rotation map additively (MapRotationService: config current_map override)
            if (ServiceLocator.TryGet<MapRotationService>(out var mapService)
                && ServiceLocator.TryGet<ISceneLoader>(out var sceneLoader))
            {
                var map = mapService.CurrentMap;
                if (map != null)
                {
                    _loadedMapScene = map.SceneName;
                    await sceneLoader.LoadSceneAdditive(_loadedMapScene);
                }
            }
        }

        public async void Exit()
        {
            if (!string.IsNullOrEmpty(_loadedMapScene)
                && ServiceLocator.TryGet<ISceneLoader>(out var sceneLoader))
            {
                await sceneLoader.UnloadScene(_loadedMapScene);
                _loadedMapScene = null;
            }
        }

        public void Update(float deltaTime) { }
    }
}
