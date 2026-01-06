using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Core.Logging;

namespace Core.GameModes
{
    /// <summary>
    /// Service for loading and unloading map scenes additively.
    /// </summary>
    public class MapLoader : IMapLoader
    {
        private readonly Dictionary<string, MapData> _maps = new Dictionary<string, MapData>();
        private MapData _currentMap;
        private string _currentLoadedSceneName;

        public MapData CurrentMap => _currentMap;
        public bool IsMapLoaded => !string.IsNullOrEmpty(_currentLoadedSceneName);

        public MapLoader()
        {
            LoadAvailableMaps();
        }

        private void LoadAvailableMaps()
        {
            // Load all map data from Resources
            MapData[] maps = Resources.LoadAll<MapData>("Maps");

            foreach (var map in maps)
            {
                if (map != null && !string.IsNullOrEmpty(map.SceneName))
                {
                    _maps[map.SceneName] = map;
                    GameLogger.Log($"Registered map: {map.DisplayName} ({map.SceneName})");
                }
            }

            GameLogger.Log($"Loaded {_maps.Count} map definitions");
        }

        public async UniTask<bool> LoadMapAsync(MapData mapData)
        {
            if (mapData == null)
            {
                GameLogger.LogError("MapData is null");
                return false;
            }

            return await LoadMapAsync(mapData.SceneName);
        }

        public async UniTask<bool> LoadMapAsync(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                GameLogger.LogError("Scene name is null or empty");
                return false;
            }

            // Unload current map if one is loaded
            if (IsMapLoaded)
            {
                await UnloadCurrentMapAsync();
            }

            try
            {
                GameLogger.Log($"Loading map scene: {sceneName}");

                // Load scene additively
                AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

                if (loadOperation == null)
                {
                    GameLogger.LogError($"Failed to start loading scene: {sceneName}");
                    return false;
                }

                await loadOperation;

                // Get the loaded scene
                Scene loadedScene = SceneManager.GetSceneByName(sceneName);

                if (!loadedScene.isLoaded)
                {
                    GameLogger.LogError($"Scene loaded but not active: {sceneName}");
                    return false;
                }

                // Set as active scene for physics/lighting
                SceneManager.SetActiveScene(loadedScene);

                _currentLoadedSceneName = sceneName;
                _currentMap = GetMapData(sceneName);

                GameLogger.Log($"Map loaded successfully: {sceneName}");
                return true;
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ex);
                return false;
            }
        }

        public async UniTask UnloadCurrentMapAsync()
        {
            if (!IsMapLoaded)
            {
                return;
            }

            try
            {
                GameLogger.Log($"Unloading map scene: {_currentLoadedSceneName}");

                Scene sceneToUnload = SceneManager.GetSceneByName(_currentLoadedSceneName);

                if (sceneToUnload.isLoaded)
                {
                    AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(sceneToUnload);
                    await unloadOperation;
                }

                _currentLoadedSceneName = null;
                _currentMap = null;

                GameLogger.Log("Map unloaded successfully");
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ex);
            }
        }

        public MapData GetMapData(string sceneName)
        {
            return _maps.TryGetValue(sceneName, out var mapData) ? mapData : null;
        }
    }
}
