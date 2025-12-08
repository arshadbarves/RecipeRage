using Core.Bootstrap;
using Core.Logging;
using Core.Networking.Services;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Cooking
{
    /// <summary>
    /// Network-aware ingredient spawning system.
    /// Follows Single Responsibility Principle - handles only ingredient spawning.
    /// </summary>
    public class IngredientNetworkSpawner : NetworkBehaviour
    {
        [Header("Ingredient Prefabs")]
        [SerializeField] private GameObject _ingredientPrefab;

        private INetworkObjectPool _objectPool;
        private INetworkGameManager _networkGameManager;

        /// <summary>
        /// Initialize the spawner.
        /// </summary>
        private void Awake()
        {
            // Get services from ServiceContainer
            _objectPool = GameBootstrap.Services?.Session?.NetworkObjectPool;
            _networkGameManager = GameBootstrap.Services?.Session?.NetworkGameManager;
        }

        /// <summary>
        /// Spawn an ingredient at the specified position.
        /// </summary>
        /// <param name="ingredientData">The ingredient data</param>
        /// <param name="position">The position to spawn at</param>
        /// <returns>The spawned ingredient NetworkObject</returns>
        public NetworkObject SpawnIngredient(Ingredient ingredientData, Vector3 position)
        {
            if (!IsServer)
            {
                GameLogger.LogWarning("Only the server can spawn ingredients");
                return null;
            }

            if (ingredientData == null)
            {
                GameLogger.LogError("Cannot spawn null ingredient");
                return null;
            }

            // Get ingredient from pool or create new
            NetworkObject ingredientObject;
            if (_objectPool != null)
            {
                ingredientObject = _objectPool.Get(_ingredientPrefab, position, Quaternion.identity);
            }
            else
            {
                // Fallback to direct spawning if pool not available
                ingredientObject = _networkGameManager?.SpawnNetworkObject(_ingredientPrefab, position, Quaternion.identity);
            }

            if (ingredientObject == null)
            {
                GameLogger.LogError("Failed to spawn ingredient");
                return null;
            }

            // Set the ingredient data
            IngredientItem ingredientItem = ingredientObject.GetComponent<IngredientItem>();
            if (ingredientItem != null)
            {
                ingredientItem.SetIngredient(ingredientData);
            }

            return ingredientObject;
        }

        /// <summary>
        /// Despawn an ingredient.
        /// </summary>
        /// <param name="ingredientObject">The ingredient NetworkObject to despawn</param>
        public void DespawnIngredient(NetworkObject ingredientObject)
        {
            if (!IsServer)
            {
                GameLogger.LogWarning("Only the server can despawn ingredients");
                return;
            }

            if (ingredientObject == null)
            {
                GameLogger.LogWarning("Cannot despawn null ingredient");
                return;
            }

            // Return to pool or despawn
            if (_objectPool != null)
            {
                _objectPool.Return(ingredientObject);
            }
            else
            {
                _networkGameManager?.DespawnNetworkObject(ingredientObject);
            }
        }

        /// <summary>
        /// Spawn an ingredient at a specific station.
        /// </summary>
        /// <param name="ingredientData">The ingredient data</param>
        /// <param name="stationNetworkId">The station's NetworkObject ID</param>
        /// <returns>The spawned ingredient NetworkObject</returns>
        public NetworkObject SpawnIngredientAtStation(Ingredient ingredientData, ulong stationNetworkId)
        {
            if (!IsServer)
            {

                GameLogger.LogWarning("Only the server can spawn ingredients");
                return null;
            }

            // Find the station
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(stationNetworkId, out NetworkObject stationObject))
            {
                GameLogger.LogError($"Station with ID {stationNetworkId} not found");
                return null;
            }

            // Spawn at station position
            Vector3 spawnPosition = stationObject.transform.position + Vector3.up * 0.5f;
            return SpawnIngredient(ingredientData, spawnPosition);
        }
    }
}
