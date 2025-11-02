using System;
using System.Collections.Generic;
using Core.Characters;
using Core.Logging;

namespace Core.Networking.Services
{
    /// <summary>
    /// Manages player network registration and lookup.
    /// Follows Single Responsibility Principle - handles only player tracking.
    /// </summary>
    public class PlayerNetworkManager : IPlayerNetworkManager
    {
        private readonly ILoggingService _logger;
        private readonly Dictionary<ulong, PlayerController> _players;

        /// <summary>
        /// Event triggered when a player is registered.
        /// </summary>
        public event Action<PlayerController> OnPlayerRegistered;

        /// <summary>
        /// Event triggered when a player is unregistered.
        /// </summary>
        public event Action<ulong> OnPlayerUnregistered;

        /// <summary>
        /// Initialize the player network manager.
        /// </summary>
        /// <param name="logger">The logging service</param>
        public PlayerNetworkManager(ILoggingService logger)
        {
            _logger = logger;
            _players = new Dictionary<ulong, PlayerController>();
        }

        /// <summary>
        /// Register a player.
        /// </summary>
        /// <param name="clientId">The client ID of the player</param>
        /// <param name="player">The player controller</param>
        public void RegisterPlayer(ulong clientId, PlayerController player)
        {
            if (player == null)
            {
                GameLogger.LogWarning($"[PlayerNetworkManager] Cannot register null player for client {clientId}");
                return;
            }

            if (_players.ContainsKey(clientId))
            {
                GameLogger.LogWarning($"[PlayerNetworkManager] Player {clientId} is already registered");
                return;
            }

            _players[clientId] = player;
            GameLogger.Log($"[PlayerNetworkManager] Registered player {clientId}");

            OnPlayerRegistered?.Invoke(player);
        }

        /// <summary>
        /// Unregister a player.
        /// </summary>
        /// <param name="clientId">The client ID of the player</param>
        public void UnregisterPlayer(ulong clientId)
        {
            if (!_players.ContainsKey(clientId))
            {
                GameLogger.LogWarning($"[PlayerNetworkManager] Player {clientId} is not registered");
                return;
            }

            _players.Remove(clientId);
            GameLogger.Log($"[PlayerNetworkManager] Unregistered player {clientId}");

            OnPlayerUnregistered?.Invoke(clientId);
        }

        /// <summary>
        /// Get a player by client ID.
        /// </summary>
        /// <param name="clientId">The client ID of the player</param>
        /// <returns>The player controller, or null if not found</returns>
        public PlayerController GetPlayer(ulong clientId)
        {
            if (_players.TryGetValue(clientId, out PlayerController player))
            {
                return player;
            }

            return null;
        }

        /// <summary>
        /// Get all registered players.
        /// </summary>
        /// <returns>A read-only list of all players</returns>
        public IReadOnlyList<PlayerController> GetAllPlayers()
        {
            return new List<PlayerController>(_players.Values);
        }

        /// <summary>
        /// Get the number of registered players.
        /// </summary>
        /// <returns>The player count</returns>
        public int GetPlayerCount()
        {
            return _players.Count;
        }

        /// <summary>
        /// Check if a player is registered.
        /// </summary>
        /// <param name="clientId">The client ID to check</param>
        /// <returns>True if the player is registered</returns>
        public bool IsPlayerRegistered(ulong clientId)
        {
            return _players.ContainsKey(clientId);
        }
    }
}
