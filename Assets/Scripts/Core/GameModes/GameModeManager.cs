using System;
using System.Collections.Generic;
using Core.Patterns;
using UnityEngine;

namespace Core.GameModes
{
    /// <summary>
    /// Manages game modes in RecipeRage.
    /// </summary>
    public class GameModeManager : MonoBehaviourSingleton<GameModeManager>
    {
        [Header("Game Mode Settings")]
        [SerializeField] private List<GameMode> _availableGameModes = new List<GameMode>();
        [SerializeField] private string _defaultGameModeId = "classic";

        /// <summary>
        /// Dictionary of available game modes by ID.
        /// </summary>
        private readonly Dictionary<string, GameMode> _gameModeDict = new Dictionary<string, GameMode>();

        /// <summary>
        /// The currently selected game mode.
        /// </summary>
        public GameMode SelectedGameMode { get; private set; }

        /// <summary>
        /// Initialize the game mode manager.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Build the game mode dictionary
            foreach (var gameMode in _availableGameModes)
            {
                if (gameMode != null && !string.IsNullOrEmpty(gameMode.Id))
                {
                    _gameModeDict[gameMode.Id] = gameMode;
                }
            }

            // Set the default game mode
            if (!string.IsNullOrEmpty(_defaultGameModeId) && _gameModeDict.ContainsKey(_defaultGameModeId))
            {
                SelectedGameMode = _gameModeDict[_defaultGameModeId];
            }
            else if (_availableGameModes.Count > 0)
            {
                SelectedGameMode = _availableGameModes[0];
                _defaultGameModeId = SelectedGameMode.Id;
            }

            // Register the game mode manager with the service locator
            ServiceLocator.Instance.Register(this);

            Debug.Log($"[GameModeManager] Initialized with {_availableGameModes.Count} game modes, default: {_defaultGameModeId}");
        }

        /// <summary>
        /// Event triggered when the selected game mode changes.
        /// </summary>
        public event Action<GameMode> OnGameModeChanged;

        /// <summary>
        /// Get a game mode by ID.
        /// </summary>
        /// <param name="gameModeId"> The game mode ID </param>
        /// <returns> The game mode, or null if not found </returns>
        public GameMode GetGameMode(string gameModeId)
        {
            if (string.IsNullOrEmpty(gameModeId) || !_gameModeDict.ContainsKey(gameModeId))
            {
                Debug.LogWarning($"[GameModeManager] Game mode not found: {gameModeId}");
                return null;
            }

            return _gameModeDict[gameModeId];
        }

        /// <summary>
        /// Get a game mode by name.
        /// </summary>
        /// <param name="gameModeName"> The game mode name </param>
        /// <returns> The game mode, or null if not found </returns>
        public GameMode GetGameModeByName(string gameModeName)
        {
            if (string.IsNullOrEmpty(gameModeName))
            {
                Debug.LogWarning("[GameModeManager] Game mode name is null or empty");
                return null;
            }

            foreach (var gameMode in _availableGameModes)
            {
                if (gameMode.DisplayName.Equals(gameModeName, StringComparison.OrdinalIgnoreCase))
                {
                    return gameMode;
                }
            }

            Debug.LogWarning($"[GameModeManager] Game mode not found by name: {gameModeName}");
            return null;
        }

        /// <summary>
        /// Get all available game modes.
        /// </summary>
        /// <returns> List of available game modes </returns>
        public List<GameMode> GetAllGameModes()
        {
            return new List<GameMode>(_availableGameModes);
        }

        /// <summary>
        /// Get all available game modes as an array.
        /// </summary>
        /// <returns> Array of available game modes </returns>
        public GameMode[] GetAvailableGameModes()
        {
            return _availableGameModes.ToArray();
        }

        /// <summary>
        /// Select a game mode by ID.
        /// </summary>
        /// <param name="gameModeId"> The game mode ID </param>
        /// <returns> True if the game mode was selected, false otherwise </returns>
        public bool SelectGameMode(string gameModeId)
        {
            var gameMode = GetGameMode(gameModeId);
            if (gameMode == null)
            {
                return false;
            }

            return SelectGameMode(gameMode);
        }

        /// <summary>
        /// Set the selected game mode.
        /// </summary>
        /// <param name="gameMode"> The game mode to select </param>
        /// <returns> True if the game mode was selected, false otherwise </returns>
        public bool SetSelectedGameMode(GameMode gameMode)
        {
            return SelectGameMode(gameMode);
        }

        /// <summary>
        /// Select a game mode.
        /// </summary>
        /// <param name="gameMode"> The game mode to select </param>
        /// <returns> True if the game mode was selected, false otherwise </returns>
        public bool SelectGameMode(GameMode gameMode)
        {
            if (gameMode == null)
            {
                Debug.LogError("[GameModeManager] Cannot select null game mode");
                return false;
            }

            if (SelectedGameMode == gameMode)
            {
                return true; // Already selected
            }

            Debug.Log($"[GameModeManager] Selecting game mode: {gameMode.DisplayName} ({gameMode.Id})");

            SelectedGameMode = gameMode;
            OnGameModeChanged?.Invoke(SelectedGameMode);

            return true;
        }

        /// <summary>
        /// Add a game mode to the available game modes.
        /// </summary>
        /// <param name="gameMode"> The game mode to add </param>
        /// <returns> True if the game mode was added, false otherwise </returns>
        public bool AddGameMode(GameMode gameMode)
        {
            if (gameMode == null || string.IsNullOrEmpty(gameMode.Id))
            {
                Debug.LogError("[GameModeManager] Cannot add invalid game mode");
                return false;
            }

            if (_gameModeDict.ContainsKey(gameMode.Id))
            {
                Debug.LogWarning($"[GameModeManager] Game mode already exists: {gameMode.Id}");
                return false;
            }

            _availableGameModes.Add(gameMode);
            _gameModeDict[gameMode.Id] = gameMode;

            Debug.Log($"[GameModeManager] Added game mode: {gameMode.DisplayName} ({gameMode.Id})");

            return true;
        }

        /// <summary>
        /// Remove a game mode from the available game modes.
        /// </summary>
        /// <param name="gameModeId"> The game mode ID to remove </param>
        /// <returns> True if the game mode was removed, false otherwise </returns>
        public bool RemoveGameMode(string gameModeId)
        {
            if (string.IsNullOrEmpty(gameModeId) || !_gameModeDict.ContainsKey(gameModeId))
            {
                Debug.LogWarning($"[GameModeManager] Game mode not found: {gameModeId}");
                return false;
            }

            var gameMode = _gameModeDict[gameModeId];
            _availableGameModes.Remove(gameMode);
            _gameModeDict.Remove(gameModeId);

            // If we removed the selected game mode, select a new one
            if (SelectedGameMode == gameMode)
            {
                if (_availableGameModes.Count > 0)
                {
                    SelectGameMode(_availableGameModes[0]);
                }
                else
                {
                    SelectedGameMode = null;
                }
            }

            Debug.Log($"[GameModeManager] Removed game mode: {gameMode.DisplayName} ({gameMode.Id})");

            return true;
        }

        /// <summary>
        /// Start the currently selected game mode.
        /// </summary>
        public void StartCurrentGameMode()
        {
            if (SelectedGameMode == null)
            {
                Debug.LogError("[GameModeManager] Cannot start game mode: No game mode selected");
                return;
            }

            Debug.Log($"[GameModeManager] Starting game mode: {SelectedGameMode.DisplayName} ({SelectedGameMode.Id})");

            // TODO: Implement game mode specific initialization
            // This would include setting up the game rules, spawning players, etc.
        }

        /// <summary>
        /// Stop the currently selected game mode.
        /// </summary>
        public void StopCurrentGameMode()
        {
            if (SelectedGameMode == null)
            {
                Debug.LogWarning("[GameModeManager] Cannot stop game mode: No game mode selected");
                return;
            }

            Debug.Log($"[GameModeManager] Stopping game mode: {SelectedGameMode.DisplayName} ({SelectedGameMode.Id})");

            // TODO: Implement game mode specific cleanup
            // This would include cleaning up game resources, saving stats, etc.
        }

        /// <summary>
        /// Check if the current game is over based on the selected game mode's rules.
        /// </summary>
        /// <returns>True if the game is over, false otherwise</returns>
        public bool IsGameOver()
        {
            if (SelectedGameMode == null)
            {
                Debug.LogWarning("[GameModeManager] Cannot check if game is over: No game mode selected");
                return false;
            }

            // TODO: Implement game mode specific game over check
            // This would include checking time limits, score limits, etc.

            // For now, return false to prevent automatic game over
            return false;
        }
    }
}