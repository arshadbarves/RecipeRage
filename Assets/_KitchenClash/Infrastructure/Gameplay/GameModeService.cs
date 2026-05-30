using KitchenClash.Application.Models;
using KitchenClash.Application;
using System;
using KitchenClash.Application.Services;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using KitchenClash.Domain;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace KitchenClash.Infrastructure.Gameplay
{
    public class GameModeService : IGameModeService, IDisposable
    {
        private readonly Dictionary<string, GameMode> _gameModes = new Dictionary<string, GameMode>();
        private GameMode _selectedGameMode;
        private readonly IRemoteConfigService _remoteConfigService;
        private readonly Domain.IMatchService _matchService;
        private string _currentLoadedSceneName;

        public event Action<GameMode> OnGameModeChanged;
        public GameMode SelectedGameMode => _selectedGameMode;

        [Inject]
        public GameModeService(IRemoteConfigService remoteConfigService, Domain.IMatchService matchService)
        {
            _remoteConfigService = remoteConfigService;
            _matchService = matchService;
            LoadGameModes();
        }

        private void LoadGameModes()
        {
            GameMode[] modes = Resources.LoadAll<GameMode>("ScriptableObjects/GameModes");
            foreach (GameMode mode in modes)
            {
                if (mode != null && !string.IsNullOrEmpty(mode.Id))
                {
                    _gameModes[mode.Id] = mode;
                }
            }

            if (_gameModes.Count > 0)
            {
                foreach (GameMode mode in _gameModes.Values)
                {
                    if (mode.UnlockedByDefault)
                    {
                        _selectedGameMode = mode;
                        break;
                    }
                }
            }

            GameLogger.Log($"Loaded {_gameModes.Count} game modes");
        }

        public GameMode[] GetAvailableGameModes()
        {
            var unlocked = new List<GameMode>();
            foreach (GameMode mode in _gameModes.Values)
            {
                if (mode.UnlockedByDefault)
                {
                    unlocked.Add(mode);
                }
            }
            return unlocked.ToArray();
        }

        public GameMode GetGameMode(string id) => _gameModes.TryGetValue(id, out GameMode mode) ? mode : null;

        public bool SelectGameMode(string id)
        {
            GameMode mode = GetGameMode(id);
            if (mode == null)
            {
                return false;
            }

            if (_selectedGameMode == mode)
            {
                return true;
            }

            _selectedGameMode = mode;
            OnGameModeChanged?.Invoke(mode);
            return true;
        }

        public async UniTask<bool> LoadMapAsync(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(_currentLoadedSceneName))
            {
                await UnloadCurrentMapAsync();
            }

            try
            {
                await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                _currentLoadedSceneName = sceneName;
                return true;
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
                return false;
            }
        }

        public async UniTask UnloadCurrentMapAsync()
        {
            if (string.IsNullOrEmpty(_currentLoadedSceneName))
            {
                return;
            }

            try
            {
                Scene sceneToUnload = SceneManager.GetSceneByName(_currentLoadedSceneName);
                if (sceneToUnload.isLoaded)
                {
                    await SceneManager.UnloadSceneAsync(sceneToUnload);
                }
                _currentLoadedSceneName = null;
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
            }
        }

        public void Dispose()
        {
            _gameModes.Clear();
            _selectedGameMode = null;
        }
    }

    // IGameModeService is defined in Domain/Interfaces/IGameModeService.cs
}
