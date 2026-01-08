using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Bootstrap;
using Modules.Logging;
using Modules.RemoteConfig;
using Modules.RemoteConfig.Models;
using VContainer;

namespace Gameplay.GameModes
{
    /// <summary>
    /// Game mode service - pure C# class, no MonoBehaviour
    /// Implements IDisposable for proper cleanup on logout
    /// Uses RemoteConfigService for game settings
    /// </summary>
    public class GameModeService : IGameModeService, IDisposable
    {
        private readonly Dictionary<string, GameMode> _gameModes = new Dictionary<string, GameMode>();
        private GameMode _selectedGameMode;
        private GameSettingsConfig _gameSettings;
        private readonly IRemoteConfigService _remoteConfigService;

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
                _gameSettings = settings;
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
                _gameSettings = settings;
                GameLogger.Log("Game settings updated from RemoteConfig");
            }
        }
        
        /// <summary>
        /// Gets game settings from RemoteConfig
        /// </summary>
        public GameSettingsConfig GetGameSettings()
        {
            return _gameSettings;
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
            _gameSettings = null;
        }
    }
}
