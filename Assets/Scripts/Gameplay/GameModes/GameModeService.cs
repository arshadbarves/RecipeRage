using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Core.Logging;
using Core.RemoteConfig.Interfaces;
using Core.RemoteConfig.Models;
using Gameplay.GameModes.Logic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Gameplay.GameModes
{
    public class GameModeService : IGameModeService, IDisposable
    {
        private readonly Dictionary<string, GameMode> _gameModes = new Dictionary<string, GameMode>();
        private GameMode _selectedGameMode;
        private readonly IRemoteConfigService _remoteConfigService;
        private string _currentLoadedSceneName;

        public event Action<GameMode> OnGameModeChanged;

        public GameMode SelectedGameMode => _selectedGameMode;

        [Inject]
        public GameModeService(IRemoteConfigService remoteConfigService)
        {
            _remoteConfigService = remoteConfigService;
            LoadGameSettings();
            LoadGameModes();
            SubscribeToConfigUpdates();
        }

        private void LoadGameSettings()
        {
            if (_remoteConfigService != null && _remoteConfigService.TryGetConfig<GameSettingsConfig>(out var settings))
            {
                GameLogger.Log("Loaded game settings from RemoteConfig");
            }
            else
            {
                GameLogger.LogWarning("RemoteConfig not available, using default settings");
            }
        }

        private void SubscribeToConfigUpdates()
        {
            if (_remoteConfigService != null)
            {
                _remoteConfigService.OnSpecificConfigUpdated += OnConfigUpdated;
            }
        }

        private void OnConfigUpdated(Type configType, IConfigModel config)
        {
            if (configType == typeof(GameSettingsConfig) && config is GameSettingsConfig settings)
            {
                GameLogger.Log("Game settings updated from RemoteConfig");
            }
        }

        private void LoadGameModes()
        {
            // Load from Resources
            GameMode[] modes = Resources.LoadAll<GameMode>("GameModes");

            foreach (var mode in modes)
            {
                if (mode != null && !string.IsNullOrEmpty(mode.Id))
                {
                    _gameModes[mode.Id] = mode;
                }
            }

            // Set default
            if (_gameModes.ContainsKey("classic"))
            {
                _selectedGameMode = _gameModes["classic"];
            }
            else if (_gameModes.Count > 0)
            {
                foreach (var mode in _gameModes.Values)
                {
                    _selectedGameMode = mode;
                    break;
                }
            }

            GameLogger.Log($"Loaded {_gameModes.Count} game modes");
        }

        public GameMode[] GetAvailableGameModes()
        {
            var modes = new GameMode[_gameModes.Count];
            _gameModes.Values.CopyTo(modes, 0);
            return modes;
        }

        public GameMode GetGameMode(string id)
        {
            return _gameModes.TryGetValue(id, out var mode) ? mode : null;
        }

        public bool SelectGameMode(string id)
        {
            var mode = GetGameMode(id);
            return mode != null && SelectGameMode(mode);
        }

        public bool SelectGameMode(GameMode mode)
        {
            if (mode == null) return false;
            if (_selectedGameMode == mode) return true;

            _selectedGameMode = mode;
            OnGameModeChanged?.Invoke(mode);

            GameLogger.Log($"Selected: {mode.DisplayName}");
            return true;
        }

        /// <summary>
        /// Create game mode logic implementation based on selected game mode
        /// </summary>
        public IGameModeLogic CreateGameModeLogic()
        {
            if (_selectedGameMode == null)
            {
                GameLogger.LogError("No game mode selected");
                return null;
            }

            // For now, always return Classic logic
            // In future, use _selectedGameMode.Id to select different implementations
            return new ClassicGameModeLogic();
        }

        /// <summary>
        /// Load map scene additively
        /// </summary>
        /// <summary>
        /// Load map scene additively
        /// </summary>
        public async UniTask<bool> LoadMapAsync(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                GameLogger.LogError("Map scene name is null or empty");
                return false;
            }

            // Unload current map if one is loaded
            if (!string.IsNullOrEmpty(_currentLoadedSceneName))
            {
                await UnloadCurrentMapAsync();
            }

            try
            {
                GameLogger.Log($"Loading map scene: {sceneName}");

                AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                if (loadOperation == null)
                {
                    GameLogger.LogError($"Failed to start loading scene: {sceneName}");
                    return false;
                }

                await loadOperation;

                Scene loadedScene = SceneManager.GetSceneByName(sceneName);
                if (!loadedScene.isLoaded)
                {
                    GameLogger.LogError($"Scene loaded but not active: {sceneName}");
                    return false;
                }

                SceneManager.SetActiveScene(loadedScene);
                _currentLoadedSceneName = sceneName;

                GameLogger.Log($"Map loaded successfully: {sceneName}");
                return true;
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
                return false;
            }
        }

        /// <summary>
        /// Unload currently loaded map scene
        /// </summary>
        public async UniTask UnloadCurrentMapAsync()
        {
            if (string.IsNullOrEmpty(_currentLoadedSceneName))
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
                GameLogger.Log("Map unloaded successfully");
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
            }
        }

        public void Dispose()
        {
            // Unsubscribe from config updates
            if (_remoteConfigService != null)
            {
                _remoteConfigService.OnSpecificConfigUpdated -= OnConfigUpdated;
            }

            GameLogger.Log("Disposing");
            _gameModes.Clear();
            _selectedGameMode = null;
        }
    }
}
