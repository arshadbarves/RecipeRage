using System;
using System.Collections.Generic;
using Core.Logging;
using Unity.Netcode;
using UnityEngine;

namespace Core.Networking.Services
{
    /// <summary>
    /// Manages network game lifecycle and object spawning.
    /// Follows Single Responsibility Principle - handles only network object management.
    /// </summary>
    public class NetworkGameManager : INetworkGameManager
    {
        private readonly ILoggingService _logger;
        private readonly IPlayerNetworkManager _playerNetworkManager;
        private readonly Dictionary<ulong, NetworkObject> _spawnedPlayers;
        private bool _isGameActive;
        
        /// <summary>
        /// Event triggered when a player joins the game.
        /// </summary>
        public event Action<ulong> OnPlayerJoined;
        
        /// <summary>
        /// Event triggered when a player leaves the game.
        /// </summary>
        public event Action<ulong> OnPlayerLeft;
        
        /// <summary>
        /// Whether the game is currently active.
        /// </summary>
        public bool IsGameActive => _isGameActive;
        
        /// <summary>
        /// Initialize the network game manager.
        /// </summary>
        /// <param name="logger">The logging service</param>
        /// <param name="playerNetworkManager">The player network manager</param>
        public NetworkGameManager(ILoggingService logger, IPlayerNetworkManager playerNetworkManager)
        {
            _logger = logger;
            _playerNetworkManager = playerNetworkManager;
            _spawnedPlayers = new Dictionary<ulong, NetworkObject>();
            _isGameActive = false;
            
            // Subscribe to NetworkManager events
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }
        }
        
        /// <summary>
        /// Start the network game session.
        /// </summary>
        public void StartGame()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                GameLogger.Network.LogWarning("[NetworkGameManager] Only the server can start the game");
                return;
            }
            
            _isGameActive = true;
            GameLogger.Network.Log("[NetworkGameManager] Game started");
        }
        
        /// <summary>
        /// End the network game session.
        /// </summary>
        public void EndGame()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                GameLogger.Network.LogWarning("[NetworkGameManager] Only the server can end the game");
                return;
            }
            
            _isGameActive = false;
            GameLogger.Network.Log("[NetworkGameManager] Game ended");
        }
        
        /// <summary>
        /// Spawn a player at the specified position.
        /// </summary>
        /// <param name="clientId">The client ID of the player to spawn</param>
        /// <param name="spawnPosition">The position to spawn the player at</param>
        public void SpawnPlayer(ulong clientId, Vector3 spawnPosition)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                GameLogger.Network.LogWarning("[NetworkGameManager] Only the server can spawn players");
                return;
            }
            
            // Check if player is already spawned
            if (_spawnedPlayers.ContainsKey(clientId))
            {
                GameLogger.Network.LogWarning($"[NetworkGameManager] Player {clientId} is already spawned");
                return;
            }
            
            // Get the player prefab from NetworkManager
            NetworkObject playerPrefab = NetworkManager.Singleton.NetworkConfig.PlayerPrefab.GetComponent<NetworkObject>();
            if (playerPrefab == null)
            {
                GameLogger.Network.LogError("[NetworkGameManager] Player prefab does not have a NetworkObject component");
                return;
            }
            
            // Spawn the player
            NetworkObject playerObject = UnityEngine.Object.Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            playerObject.SpawnAsPlayerObject(clientId, true);
            
            // Track the spawned player
            _spawnedPlayers[clientId] = playerObject;
            
            GameLogger.Network.Log($"[NetworkGameManager] Spawned player {clientId} at {spawnPosition}");
        }
        
        /// <summary>
        /// Despawn a player.
        /// </summary>
        /// <param name="clientId">The client ID of the player to despawn</param>
        public void DespawnPlayer(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                GameLogger.Network.LogWarning("[NetworkGameManager] Only the server can despawn players");
                return;
            }
            
            // Check if player is spawned
            if (!_spawnedPlayers.TryGetValue(clientId, out NetworkObject playerObject))
            {
                GameLogger.Network.LogWarning($"[NetworkGameManager] Player {clientId} is not spawned");
                return;
            }
            
            // Despawn the player
            if (playerObject != null && playerObject.IsSpawned)
            {
                playerObject.Despawn(true);
            }
            
            // Remove from tracking
            _spawnedPlayers.Remove(clientId);
            
            GameLogger.Network.Log($"[NetworkGameManager] Despawned player {clientId}");
        }
        
        /// <summary>
        /// Spawn a network object.
        /// </summary>
        /// <param name="prefab">The prefab to spawn</param>
        /// <param name="position">The position to spawn at</param>
        /// <param name="rotation">The rotation to spawn with</param>
        /// <returns>The spawned NetworkObject</returns>
        public NetworkObject SpawnNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                GameLogger.Network.LogWarning("[NetworkGameManager] Only the server can spawn network objects");
                return null;
            }
            
            // Instantiate the prefab
            GameObject instance = UnityEngine.Object.Instantiate(prefab, position, rotation);
            NetworkObject networkObject = instance.GetComponent<NetworkObject>();
            
            if (networkObject == null)
            {
                GameLogger.Network.LogError($"[NetworkGameManager] Prefab {prefab.name} does not have a NetworkObject component");
                UnityEngine.Object.Destroy(instance);
                return null;
            }
            
            // Spawn the network object
            networkObject.Spawn(true);
            
            GameLogger.Network.Log($"[NetworkGameManager] Spawned network object {prefab.name} at {position}");
            
            return networkObject;
        }
        
        /// <summary>
        /// Despawn a network object.
        /// </summary>
        /// <param name="networkObject">The network object to despawn</param>
        public void DespawnNetworkObject(NetworkObject networkObject)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                GameLogger.Network.LogWarning("[NetworkGameManager] Only the server can despawn network objects");
                return;
            }
            
            if (networkObject == null)
            {
                GameLogger.Network.LogWarning("[NetworkGameManager] Cannot despawn null network object");
                return;
            }
            
            if (!networkObject.IsSpawned)
            {
                GameLogger.Network.LogWarning($"[NetworkGameManager] Network object {networkObject.name} is not spawned");
                return;
            }
            
            // Despawn the network object
            networkObject.Despawn(true);
            
            GameLogger.Network.Log($"[NetworkGameManager] Despawned network object {networkObject.name}");
        }
        
        /// <summary>
        /// Handle client connected event.
        /// </summary>
        /// <param name="clientId">The client ID that connected</param>
        private void OnClientConnected(ulong clientId)
        {
            GameLogger.Network.Log($"[NetworkGameManager] Client {clientId} connected");
            OnPlayerJoined?.Invoke(clientId);
        }
        
        /// <summary>
        /// Handle client disconnected event.
        /// </summary>
        /// <param name="clientId">The client ID that disconnected</param>
        private void OnClientDisconnected(ulong clientId)
        {
            GameLogger.Network.Log($"[NetworkGameManager] Client {clientId} disconnected");
            
            // Clean up player if spawned
            if (_spawnedPlayers.ContainsKey(clientId))
            {
                DespawnPlayer(clientId);
            }
            
            OnPlayerLeft?.Invoke(clientId);
        }
        
        /// <summary>
        /// Clean up when the manager is destroyed.
        /// </summary>
        public void Dispose()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }
    }
}
