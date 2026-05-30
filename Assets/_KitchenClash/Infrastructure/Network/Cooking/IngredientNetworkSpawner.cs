using KitchenClash.Domain;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace KitchenClash.Infrastructure.Network.Cooking
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
        private INetworkObjectPool _objectPool;

        [Inject]
        private INetworkGameManager _networkGameManager;

        [Inject]
        private IMatchContext _matchContext;

        /// <summary>
        /// Initialize the spawner.
        /// </summary>
        private void Awake()
        {
            LifetimeScope scope = LifetimeScope.Find<LifetimeScope>();
            if (scope != null)
            {
                scope.Container.Inject(this);
                return;
            }

            GameLogger.LogError("[IngredientNetworkSpawner] LifetimeScope not found. Ingredient spawner dependencies were not injected.");
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
            GameObject ingredientPrefab = ingredientData.Prefab != null ? ingredientData.Prefab : _ingredientPrefab;
            if (ingredientPrefab == null)
            {
                GameLogger.LogError($"[IngredientNetworkSpawner] No ingredient prefab configured for '{ingredientData.DisplayName}'");
                return null;
            }

            if (_objectPool == null && _networkGameManager == null)
            {
                GameLogger.LogError("[IngredientNetworkSpawner] Missing injected INetworkObjectPool and INetworkGameManager. Check GameLifetimeScope registrations.");
                return null;
            }

            NetworkObject ingredientObject;
            if (_objectPool != null)
            {
                ingredientObject = _objectPool.Get(ingredientPrefab, position, Quaternion.identity);
            }
            else if (_networkGameManager != null)
            {
                ingredientObject = _networkGameManager.SpawnNetworkObject(ingredientPrefab, position, Quaternion.identity);
            }
            else
            {
                GameLogger.LogError("[IngredientNetworkSpawner] INetworkObjectPool is null and no INetworkGameManager fallback is available.");
                return null;
            }

            if (ingredientObject == null)
            {
                GameLogger.LogError($"[IngredientNetworkSpawner] Failed to spawn ingredient '{ingredientData.DisplayName}' from prefab '{ingredientPrefab.name}'");
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
            else if (_networkGameManager != null)
            {
                _networkGameManager.DespawnNetworkObject(ingredientObject);
            }
            else
            {
                GameLogger.LogError("[IngredientNetworkSpawner] Missing injected despawn services. Check GameLifetimeScope registrations.");
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
            if (_matchContext == null || !_matchContext.TryGetSpawnedObject(stationNetworkId, out NetworkObject stationObject))
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
