using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Logging;

namespace Core.GameModes
{
    /// <summary>
    /// Game mode service - pure C# class, no MonoBehaviour
    /// Implements IDisposable for proper cleanup on logout
    /// </summary>
    public class GameModeService : IGameModeService, IDisposable
    {
        private readonly Dictionary<string, GameMode> _gameModes = new Dictionary<string, GameMode>();
        private GameMode _selectedGameMode;

        public event Action<GameMode> OnGameModeChanged;

        public GameMode SelectedGameMode => _selectedGameMode;

        public GameModeService()
        {
            LoadGameModes();
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
            GameLogger.Log("Disposing");
            _gameModes.Clear();
            _selectedGameMode = null;
        }
    }
}
