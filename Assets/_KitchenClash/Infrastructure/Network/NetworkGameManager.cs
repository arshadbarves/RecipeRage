using System;
using System.Collections.Generic;
using KitchenClash.Domain;
using Unity.Netcode;
using UnityEngine;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// Manages network game lifecycle and object spawning.
    /// </summary>
    public class NetworkGameManager : INetworkGameManager, IDisposable
    {
        private readonly NetworkManager _networkManager;
        private readonly IPlayerNetworkManager _playerNetworkManager;
        private readonly Dictionary<ulong, NetworkObject> _spawnedPlayers;
        private bool _isGameActive;

        public event Action<ulong> OnPlayerJoined;
        public event Action<ulong> OnPlayerLeft;
        public bool IsGameActive => _isGameActive;

        public NetworkGameManager(NetworkManager networkManager, IPlayerNetworkManager playerNetworkManager)
        {
            _networkManager = networkManager;
            _playerNetworkManager = playerNetworkManager;
            _spawnedPlayers = new Dictionary<ulong, NetworkObject>();
            _isGameActive = false;

            if (_networkManager != null)
            {
                _networkManager.OnClientConnectedCallback += OnClientConnected;
                _networkManager.OnClientDisconnectCallback += OnClientDisconnected;
            }
        }

        public void StartGame()
        {
            if (!IsServer())
            {
                GameLogger.LogWarning("Only the server can start the game");
                return;
            }

            _isGameActive = true;
            GameLogger.Log("[NetworkGameManager] Game started");
        }

        public void EndGame()
        {
            if (!IsServer())
            {
                GameLogger.LogWarning("Only the server can end the game");
                return;
            }

            _isGameActive = false;
            GameLogger.Log("[NetworkGameManager] Game ended");
        }

        public void SpawnPlayer(ulong clientId, Vector3 spawnPosition)
        {
            if (!IsServer())
            {
                GameLogger.LogWarning("Only the server can spawn players");
                return;
            }

            if (_spawnedPlayers.ContainsKey(clientId))
            {
                GameLogger.LogWarning($"[NetworkGameManager] Player {clientId} is already spawned");
                return;
            }

            var playerPrefabSource = _networkManager?.NetworkConfig?.PlayerPrefab;
            if (playerPrefabSource == null)
            {
                GameLogger.LogError("[NetworkGameManager] NetworkManager player prefab is not configured");
                return;
            }

            NetworkObject playerPrefab = playerPrefabSource.GetComponent<NetworkObject>();
            if (playerPrefab == null)
            {
                GameLogger.LogError("[NetworkGameManager] Player prefab does not have a NetworkObject component");
                return;
            }

            NetworkObject playerObject = UnityEngine.Object.Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            playerObject.SpawnAsPlayerObject(clientId, true);

            _spawnedPlayers[clientId] = playerObject;

            GameLogger.Log($"[NetworkGameManager] Spawned player {clientId} at {spawnPosition}");
        }

        public void DespawnPlayer(ulong clientId)
        {
            if (!IsServer())
            {
                GameLogger.LogWarning("Only the server can despawn players");
                return;
            }

            if (!_spawnedPlayers.TryGetValue(clientId, out NetworkObject playerObject))
            {
                GameLogger.LogWarning($"[NetworkGameManager] Player {clientId} is not spawned");
                return;
            }

            if (playerObject != null && playerObject.IsSpawned)
            {
                playerObject.Despawn(true);
            }

            _spawnedPlayers.Remove(clientId);

            GameLogger.Log($"[NetworkGameManager] Despawned player {clientId}");
        }

        public NetworkObject SpawnNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!IsServer())
            {
                GameLogger.LogWarning("Only the server can spawn network objects");
                return null;
            }

            GameObject instance = UnityEngine.Object.Instantiate(prefab, position, rotation);
            NetworkObject networkObject = instance.GetComponent<NetworkObject>();

            if (networkObject == null)
            {
                GameLogger.LogError($"[NetworkGameManager] Prefab {prefab.name} does not have a NetworkObject component");
                UnityEngine.Object.Destroy(instance);
                return null;
            }

            networkObject.Spawn(true);

            GameLogger.Log($"[NetworkGameManager] Spawned network object {prefab.name} at {position}");

            return networkObject;
        }

        public void DespawnNetworkObject(NetworkObject networkObject)
        {
            if (!IsServer())
            {
                GameLogger.LogWarning("Only the server can despawn network objects");
                return;
            }

            if (networkObject == null)
            {
                GameLogger.LogWarning("Cannot despawn null network object");
                return;
            }

            if (!networkObject.IsSpawned)
            {
                GameLogger.LogWarning($"[NetworkGameManager] Network object {networkObject.name} is not spawned");
                return;
            }

            networkObject.Despawn(true);

            GameLogger.Log($"[NetworkGameManager] Despawned network object {networkObject.name}");
        }

        private void OnClientConnected(ulong clientId)
        {
            GameLogger.Log($"[NetworkGameManager] Client {clientId} connected");

            NetworkObject playerObject = _networkManager?.SpawnManager?.GetPlayerNetworkObject(clientId);
            if (playerObject != null)
            {
                var playerController = playerObject.GetComponent<IPlayerController>();

                if (playerController != null)
                {
                    _playerNetworkManager?.RegisterPlayer(clientId, playerController);
                }
            }

            OnPlayerJoined?.Invoke(clientId);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            GameLogger.Log($"[NetworkGameManager] Client {clientId} disconnected");

            CleanupPlayerObjects(clientId);

            if (_playerNetworkManager.IsPlayerRegistered(clientId))
            {
                _playerNetworkManager.UnregisterPlayer(clientId);
            }

            OnPlayerLeft?.Invoke(clientId);
        }

        private void CleanupPlayerObjects(ulong clientId)
        {
            if (!IsServer())
            {
                return;
            }

            if (_networkManager?.SpawnManager?.SpawnedObjects == null)
            {
                GameLogger.LogWarning("[NetworkGameManager] SpawnManager or SpawnedObjects is null during cleanup");
                return;
            }

            List<NetworkObject> objectsToCleanup = new List<NetworkObject>();

            foreach (var kvp in _networkManager.SpawnManager.SpawnedObjects)
            {
                NetworkObject networkObject = kvp.Value;

                if (networkObject != null && networkObject.OwnerClientId == clientId)
                {
                    objectsToCleanup.Add(networkObject);
                }
            }

            foreach (NetworkObject networkObject in objectsToCleanup)
            {
                if (networkObject == null) continue;

                if (networkObject.IsPlayerObject)
                {
                    DespawnPlayer(clientId);
                }
                else
                {
                    DespawnNetworkObject(networkObject);
                }
            }

            GameLogger.Log($"[NetworkGameManager] Cleaned up {objectsToCleanup.Count} objects for client {clientId}");
        }

        public void Dispose()
        {
            if (_networkManager != null)
            {
                _networkManager.OnClientConnectedCallback -= OnClientConnected;
                _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        private bool IsServer()
        {
            return _networkManager != null && _networkManager.IsServer;
        }
    }
}
