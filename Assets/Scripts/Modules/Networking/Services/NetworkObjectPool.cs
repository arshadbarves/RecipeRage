using System.Collections.Generic;
using Modules.Logging;
using Unity.Netcode;
using UnityEngine;

namespace Modules.Networking.Services
{
    /// <summary>
    /// Network object pooling implementation.
    /// Follows Object Pool Pattern for efficient network object reuse.
    /// </summary>
    public class NetworkObjectPool : INetworkObjectPool
    {
        private readonly ILoggingService _logger;
        private readonly Dictionary<GameObject, Queue<NetworkObject>> _pools;
        private readonly HashSet<NetworkObject> _activeObjects;

        /// <summary>
        /// Initialize the network object pool.
        /// </summary>
        /// <param name="logger">The logging service</param>
        public NetworkObjectPool(ILoggingService logger)
        {
            _logger = logger;
            _pools = new Dictionary<GameObject, Queue<NetworkObject>>();
            _activeObjects = new HashSet<NetworkObject>();
        }

        /// <summary>
        /// Get a network object from the pool.
        /// </summary>
        /// <param name="prefab">The prefab to get</param>
        /// <param name="position">The position to spawn at</param>
        /// <param name="rotation">The rotation to spawn with</param>
        /// <returns>The network object</returns>
        public NetworkObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                GameLogger.LogWarning("[NetworkObjectPool] Only the server can get objects from the pool");
                return null;
            }

            // Get or create pool for this prefab
            if (!_pools.TryGetValue(prefab, out Queue<NetworkObject> pool))
            {
                pool = new Queue<NetworkObject>();
                _pools[prefab] = pool;
            }

            NetworkObject networkObject;

            // Try to get from pool
            if (pool.Count > 0)
            {
                networkObject = pool.Dequeue();
                networkObject.transform.position = position;
                networkObject.transform.rotation = rotation;
                networkObject.gameObject.SetActive(true);

                // Spawn if not already spawned
                if (!networkObject.IsSpawned)
                {
                    networkObject.Spawn(true);
                }

                GameLogger.Log($"[NetworkObjectPool] Reused pooled object {prefab.name}");
            }
            else
            {
                // Create new instance
                GameObject instance = Object.Instantiate(prefab, position, rotation);
                networkObject = instance.GetComponent<NetworkObject>();

                if (networkObject == null)
                {
                    GameLogger.LogError($"[NetworkObjectPool] Prefab {prefab.name} does not have a NetworkObject component");
                    Object.Destroy(instance);
                    return null;
                }

                networkObject.Spawn(true);
                GameLogger.Log($"[NetworkObjectPool] Created new pooled object {prefab.name}");
            }

            _activeObjects.Add(networkObject);
            return networkObject;
        }

        /// <summary>
        /// Return a network object to the pool.
        /// </summary>
        /// <param name="networkObject">The network object to return</param>
        public void Return(NetworkObject networkObject)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                GameLogger.LogWarning("[NetworkObjectPool] Only the server can return objects to the pool");
                return;
            }

            if (networkObject == null)
            {
                GameLogger.LogWarning("[NetworkObjectPool] Cannot return null network object");
                return;
            }

            // Remove from active objects
            _activeObjects.Remove(networkObject);

            // Despawn but don't destroy
            if (networkObject.IsSpawned)
            {
                networkObject.Despawn(false);
            }

            // Deactivate
            networkObject.gameObject.SetActive(false);

            // Find the prefab this object belongs to
            // Note: This is a simplified approach. In production, you might want to track the prefab reference
            GameObject prefab = FindPrefabForObject(networkObject);
            if (prefab != null)
            {
                if (!_pools.TryGetValue(prefab, out Queue<NetworkObject> pool))
                {
                    pool = new Queue<NetworkObject>();
                    _pools[prefab] = pool;
                }

                pool.Enqueue(networkObject);
                GameLogger.Log("[NetworkObjectPool] Returned object to pool");
            }
            else
            {
                // If we can't find the prefab, just destroy it
                Object.Destroy(networkObject.gameObject);
                GameLogger.LogWarning("[NetworkObjectPool] Could not find prefab for object, destroying instead");
            }
        }

        /// <summary>
        /// Prewarm the pool with a specific prefab.
        /// </summary>
        /// <param name="prefab">The prefab to prewarm</param>
        /// <param name="count">The number of instances to create</param>
        public void Prewarm(GameObject prefab, int count)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                GameLogger.LogWarning("[NetworkObjectPool] Only the server can prewarm the pool");
                return;
            }

            // Get or create pool for this prefab
            if (!_pools.TryGetValue(prefab, out Queue<NetworkObject> pool))
            {
                pool = new Queue<NetworkObject>();
                _pools[prefab] = pool;
            }

            // Create instances
            for (int i = 0; i < count; i++)
            {
                GameObject instance = Object.Instantiate(prefab);
                NetworkObject networkObject = instance.GetComponent<NetworkObject>();

                if (networkObject == null)
                {
                    GameLogger.LogError($"[NetworkObjectPool] Prefab {prefab.name} does not have a NetworkObject component");
                    Object.Destroy(instance);
                    continue;
                }

                instance.SetActive(false);
                pool.Enqueue(networkObject);
            }

            GameLogger.Log($"[NetworkObjectPool] Prewarmed pool with {count} instances of {prefab.name}");
        }

        /// <summary>
        /// Clear all pooled objects.
        /// </summary>
        public void Clear()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                GameLogger.LogWarning("[NetworkObjectPool] Only the server can clear the pool");
                return;
            }

            // Destroy all pooled objects
            foreach (var pool in _pools.Values)
            {
                while (pool.Count > 0)
                {
                    NetworkObject networkObject = pool.Dequeue();
                    if (networkObject != null)
                    {
                        Object.Destroy(networkObject.gameObject);
                    }
                }
            }

            _pools.Clear();
            _activeObjects.Clear();

            GameLogger.Log("[NetworkObjectPool] Cleared all pooled objects");
        }

        /// <summary>
        /// Find the prefab for a network object.
        /// This is a simplified implementation - in production you might want to track this explicitly.
        /// </summary>
        /// <param name="networkObject">The network object</param>
        /// <returns>The prefab, or null if not found</returns>
        private GameObject FindPrefabForObject(NetworkObject networkObject)
        {
            // Check each pool to see if this object belongs to it
            foreach (var kvp in _pools)
            {
                // Simple name-based matching
                // In production, you might want a more robust approach
                if (networkObject.name.StartsWith(kvp.Key.name))
                {
                    return kvp.Key;
                }
            }

            return null;
        }
    }
}
