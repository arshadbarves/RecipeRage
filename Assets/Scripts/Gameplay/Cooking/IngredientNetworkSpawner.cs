using Core.Bootstrap;
using Core.Logging;
using Core.Networking.Services;
using Unity.Netcode;
using UnityEngine;
using VContainer;

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

        [Inject]
        private SessionManager _sessionManager;

        private INetworkObjectPool _objectPool;
        private INetworkGameManager _networkGameManager;

        /// <summary>
        /// Initialize the spawner.
        /// </summary>
        private void Awake()
        {
            // Services will be resolved from SessionManager when needed
        }

        private void EnsureServices()
        {
            if (_objectPool == null || _networkGameManager == null)
            {
                var sessionContainer = _sessionManager?.SessionContainer;
                if (sessionContainer != null)
                {
                    _objectPool = sessionContainer.Resolve<INetworkObjectPool>();
                    _networkGameManager = sessionContainer.Resolve<INetworkGameManager>();
                }
            }
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

            EnsureServices();

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

            EnsureServices();

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
