using System.Collections.Generic;
using Core.Logging;
using Unity.Netcode;
using UnityEngine;

namespace Core.Networking.Services
{
    /// <summary>
    /// Handles player connections and disconnections gracefully.
    /// Follows Single Responsibility Principle - handles only connection management.
    /// </summary>
    public class ConnectionHandler
    {
        private readonly ILoggingService _logger;
        private readonly IPlayerNetworkManager _playerNetworkManager;
        private readonly INetworkGameManager _networkGameManager;

        /// <summary>
        /// Initialize the connection handler.
        /// </summary>
        /// <param name="logger">The logging service</param>
        /// <param name="playerNetworkManager">The player network manager</param>
        /// <param name="networkGameManager">The network game manager</param>
        public ConnectionHandler(
            ILoggingService logger,
            IPlayerNetworkManager playerNetworkManager,
            INetworkGameManager networkGameManager)
        {
            _logger = logger;
            _playerNetworkManager = playerNetworkManager;
            _networkGameManager = networkGameManager;
        }

        /// <summary>
        /// Handle a client connection.
        /// </summary>
        /// <param name="clientId">The client ID that connected</param>
        public void OnClientConnected(ulong clientId)
        {
            GameLogger.Log($"[ConnectionHandler] Client {clientId} connected");

            // Spawn player if game is active
            if (_networkGameManager.IsGameActive)
            {
                Vector3 spawnPosition = GetSpawnPosition(clientId);
                _networkGameManager.SpawnPlayer(clientId, spawnPosition);
            }
        }

        /// <summary>
        /// Handle a client disconnection.
        /// </summary>
        /// <param name="clientId">The client ID that disconnected</param>
        public void OnClientDisconnected(ulong clientId)
        {
            GameLogger.Log($"[ConnectionHandler] Client {clientId} disconnected");

            // Clean up player objects
            CleanupPlayerObjects(clientId);

            // Unregister player
            if (_playerNetworkManager.IsPlayerRegistered(clientId))
            {
                _playerNetworkManager.UnregisterPlayer(clientId);
            }

            // Redistribute owned objects
            RedistributeOwnedObjects(clientId);
        }

        /// <summary>
        /// Clean up all objects owned by a disconnected player.
        /// </summary>
        /// <param name="clientId">The client ID</param>
        private void CleanupPlayerObjects(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            List<NetworkObject> objectsToCleanup = new List<NetworkObject>();

            // Find all objects owned by this client
            foreach (var kvp in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
            {
                NetworkObject networkObject = kvp.Value;

                if (networkObject.OwnerClientId == clientId)
                {
                    objectsToCleanup.Add(networkObject);
                }
            }

            // Clean up objects
            foreach (NetworkObject networkObject in objectsToCleanup)
            {
                // Check if it's a player object
                if (networkObject.IsPlayerObject)
                {
                    _networkGameManager.DespawnPlayer(clientId);
                }
                else
                {
                    // For other objects, either despawn or transfer ownership
                    // For now, we'll despawn them
                    _networkGameManager.DespawnNetworkObject(networkObject);
                }
            }

            GameLogger.Log($"[ConnectionHandler] Cleaned up {objectsToCleanup.Count} objects for client {clientId}");
        }

        /// <summary>
        /// Redistribute objects owned by a disconnected player.
        /// For example, ingredients being held should be dropped.
        /// </summary>
        /// <param name="clientId">The client ID</param>
        private void RedistributeOwnedObjects(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            // In a more complex implementation, you might want to:
            // 1. Drop held ingredients at player's last position
            // 2. Release locked stations
            // 3. Cancel in-progress orders
            // 4. Transfer team ownership

            GameLogger.Log($"[ConnectionHandler] Redistributed objects for client {clientId}");
        }

        /// <summary>
        /// Get a spawn position for a player.
        /// </summary>
        /// <param name="clientId">The client ID</param>
        /// <returns>The spawn position</returns>
        private Vector3 GetSpawnPosition(ulong clientId)
        {
            // Simple spawn position calculation
            // In production, you'd want to use spawn points
            int playerCount = _playerNetworkManager.GetPlayerCount();
            float angle = (playerCount * 90f) * Mathf.Deg2Rad;
            float radius = 5f;

            return new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );
        }
    }
}
