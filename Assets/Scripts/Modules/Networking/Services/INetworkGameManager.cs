using System;
using Unity.Netcode;
using UnityEngine;

namespace Modules.Networking.Services
{
    /// <summary>
    /// Interface for managing network game lifecycle and object spawning.
    /// Follows Single Responsibility Principle - handles only network object management.
    /// </summary>
    public interface INetworkGameManager
    {
        /// <summary>
        /// Start the network game session.
        /// </summary>
        void StartGame();
        
        /// <summary>
        /// End the network game session.
        /// </summary>
        void EndGame();
        
        /// <summary>
        /// Spawn a player at the specified position.
        /// </summary>
        /// <param name="clientId">The client ID of the player to spawn</param>
        /// <param name="spawnPosition">The position to spawn the player at</param>
        void SpawnPlayer(ulong clientId, Vector3 spawnPosition);
        
        /// <summary>
        /// Despawn a player.
        /// </summary>
        /// <param name="clientId">The client ID of the player to despawn</param>
        void DespawnPlayer(ulong clientId);
        
        /// <summary>
        /// Spawn a network object.
        /// </summary>
        /// <param name="prefab">The prefab to spawn</param>
        /// <param name="position">The position to spawn at</param>
        /// <param name="rotation">The rotation to spawn with</param>
        /// <returns>The spawned NetworkObject</returns>
        NetworkObject SpawnNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation);
        
        /// <summary>
        /// Despawn a network object.
        /// </summary>
        /// <param name="networkObject">The network object to despawn</param>
        void DespawnNetworkObject(NetworkObject networkObject);
        
        /// <summary>
        /// Whether the game is currently active.
        /// </summary>
        bool IsGameActive { get; }
        
        /// <summary>
        /// Event triggered when a player joins the game.
        /// </summary>
        event Action<ulong> OnPlayerJoined;
        
        /// <summary>
        /// Event triggered when a player leaves the game.
        /// </summary>
        event Action<ulong> OnPlayerLeft;
    }
}
