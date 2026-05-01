using System.Collections.Generic;
using KitchenClash.Domain;
using Unity.Netcode;
using UnityEngine;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// Network object pooling implementation.
    /// </summary>
    public class NetworkObjectPool : INetworkObjectPool
    {
        private readonly NetworkManager _networkManager;
        private readonly Dictionary<GameObject, Queue<NetworkObject>> _pools;
        private readonly HashSet<NetworkObject> _activeObjects;

        public NetworkObjectPool(NetworkManager networkManager)
        {
            _networkManager = networkManager;
            _pools = new Dictionary<GameObject, Queue<NetworkObject>>();
            _activeObjects = new HashSet<NetworkObject>();
        }

        public NetworkObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!IsServer())
            {
                GameLogger.LogWarning("[NetworkObjectPool] Only the server can get objects from the pool");
                return null;
            }

            if (!_pools.TryGetValue(prefab, out Queue<NetworkObject> pool))
            {
                pool = new Queue<NetworkObject>();
                _pools[prefab] = pool;
            }

            NetworkObject networkObject;

            if (pool.Count > 0)
            {
                networkObject = pool.Dequeue();
                networkObject.transform.position = position;
                networkObject.transform.rotation = rotation;
                networkObject.gameObject.SetActive(true);

                if (!networkObject.IsSpawned)
                {
                    networkObject.Spawn(true);
                }

                GameLogger.Log($"[NetworkObjectPool] Reused pooled object {prefab.name}");
            }
            else
            {
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

        public void Return(NetworkObject networkObject)
        {
            if (!IsServer())
            {
                GameLogger.LogWarning("[NetworkObjectPool] Only the server can return objects to the pool");
                return;
            }

            if (networkObject == null)
            {
                GameLogger.LogWarning("[NetworkObjectPool] Cannot return null network object");
                return;
            }

            _activeObjects.Remove(networkObject);

            if (networkObject.IsSpawned)
            {
                networkObject.Despawn(false);
            }

            networkObject.gameObject.SetActive(false);

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
                Object.Destroy(networkObject.gameObject);
                GameLogger.LogWarning("[NetworkObjectPool] Could not find prefab for object, destroying instead");
            }
        }

        public void Prewarm(GameObject prefab, int count)
        {
            if (!IsServer())
            {
                GameLogger.LogWarning("[NetworkObjectPool] Only the server can prewarm the pool");
                return;
            }

            if (!_pools.TryGetValue(prefab, out Queue<NetworkObject> pool))
            {
                pool = new Queue<NetworkObject>();
                _pools[prefab] = pool;
            }

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

        public void Clear()
        {
            if (!IsServer())
            {
                GameLogger.LogWarning("[NetworkObjectPool] Only the server can clear the pool");
                return;
            }

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

        private GameObject FindPrefabForObject(NetworkObject networkObject)
        {
            foreach (var kvp in _pools)
            {
                if (networkObject.name.StartsWith(kvp.Key.name))
                {
                    return kvp.Key;
                }
            }

            return null;
        }

        private bool IsServer()
        {
            return _networkManager != null && _networkManager.IsServer;
        }
    }
}
