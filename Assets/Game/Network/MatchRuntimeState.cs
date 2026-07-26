using Playcenter;

namespace RecipeRage.Net
{
    /// <summary>Published when the match HUD should be shown. RecipeRage.UI listens.</summary>
    public readonly struct MatchHudRequestedEvent { }

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

            // Offline/dev: start the match once the map is loaded.
            // Networked: NetworkMatch (server) owns match lifecycle instead.
            var netManager = UnityEngine.Object.FindFirstObjectByType<Unity.Netcode.NetworkManager>();
            var isNetworked = netManager != null && netManager.IsListening;
            if (!isNetworked && ServiceLocator.TryGet<MatchController>(out var match))
            {
                match.StartMatch(seed: UnityEngine.Random.Range(0, int.MaxValue));
            }

            // Notify UI to show the in-match HUD (RecipeRage.UI listens — no screen ref here)
            ServiceLocator.Get<IEventBus>().Publish(new MatchHudRequestedEvent());
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
