using System;
using System.Collections.Generic;
using Core.Characters;

namespace Core.Networking.Services
{
    /// <summary>
    /// Interface for managing player network registration and lookup.
    /// Follows Interface Segregation Principle - focused on player tracking only.
    /// </summary>
    public interface IPlayerNetworkManager
    {
        /// <summary>
        /// Register a player.
        /// </summary>
        /// <param name="clientId">The client ID of the player</param>
        /// <param name="player">The player controller</param>
        void RegisterPlayer(ulong clientId, PlayerController player);
        
        /// <summary>
        /// Unregister a player.
        /// </summary>
        /// <param name="clientId">The client ID of the player</param>
        void UnregisterPlayer(ulong clientId);
        
        /// <summary>
        /// Get a player by client ID.
        /// </summary>
        /// <param name="clientId">The client ID of the player</param>
        /// <returns>The player controller, or null if not found</returns>
        PlayerController GetPlayer(ulong clientId);
        
        /// <summary>
        /// Get all registered players.
        /// </summary>
        /// <returns>A read-only list of all players</returns>
        IReadOnlyList<PlayerController> GetAllPlayers();
        
        /// <summary>
        /// Get the number of registered players.
        /// </summary>
        /// <returns>The player count</returns>
        int GetPlayerCount();
        
        /// <summary>
        /// Check if a player is registered.
        /// </summary>
        /// <param name="clientId">The client ID to check</param>
        /// <returns>True if the player is registered</returns>
        bool IsPlayerRegistered(ulong clientId);
        
        /// <summary>
        /// Event triggered when a player is registered.
        /// </summary>
        event Action<PlayerController> OnPlayerRegistered;
        
        /// <summary>
        /// Event triggered when a player is unregistered.
        /// </summary>
        event Action<ulong> OnPlayerUnregistered;
    }
}
