using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Pooling
{
    /// <summary>
    ///     Network-aware object pooling system optimized for mobile games.
    ///     Builds on Unity's ObjectPool for memory efficiency.
    /// </summary>
    public class NetworkObjectPool : NetworkBehaviour
    {

        [Header("Pool Configuration"), SerializeField]
         private PoolConfig[] poolConfigs;
        [SerializeField] private Transform poolRoot;

        private readonly Dictionary<GameObject, ObjectPool<NetworkObject>> _pools = new Dictionary<GameObject, ObjectPool<NetworkObject>>();
        private readonly Dictionary<GameObject, Queue<NetworkObject>> _warmupQueue = new Dictionary<GameObject, Queue<NetworkObject>>();

        public static NetworkObjectPool Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializePools();
        }

        public override void OnDestroy()
        {
            foreach (ObjectPool<NetworkObject> pool in _pools.Values)
            {
                pool.Clear();
            }
            _pools.Clear();
            _warmupQueue.Clear();

            if (Instance == this)
            {
                Instance = null;
            }

            base.OnDestroy();
        }

        private void InitializePools()
        {
            if (poolConfigs == null) return;

            foreach (PoolConfig config in poolConfigs)
            {
                if (config.prefab == null) continue;

                // Create pool
                ObjectPool<NetworkObject> pool = new ObjectPool<NetworkObject>(
                    () => CreatePooledObject(config.prefab),
                    OnTakeFromPool,
                    OnReturnToPool,
                    OnDestroyPoolObject,
                    true,
                    config.defaultCapacity,
                    config.maxSize
                );

                _pools[config.prefab.gameObject] = pool;
                _warmupQueue[config.prefab.gameObject] = new Queue<NetworkObject>();

                // Warm up pool
                WarmupPool(config);
            }
        }

        private NetworkObject CreatePooledObject(NetworkObject prefab)
        {
            NetworkObject obj = Instantiate(prefab, poolRoot);
            obj.gameObject.SetActive(false);

            // Add pooling component
            PooledNetworkObject poolable = obj.gameObject.AddComponent<PooledNetworkObject>();
            poolable.Pool = this;

            return obj;
        }

        private void WarmupPool(PoolConfig config)
        {
            for (int i = 0; i < config.defaultCapacity; i++)
            {
                NetworkObject obj = _pools[config.prefab.gameObject].Get();
                _warmupQueue[config.prefab.gameObject].Enqueue(obj);
            }

            // Return warmed up objects to pool
            while (_warmupQueue[config.prefab.gameObject].Count > 0)
            {
                NetworkObject obj = _warmupQueue[config.prefab.gameObject].Dequeue();
                _pools[config.prefab.gameObject].Release(obj);
            }
        }

        private void OnTakeFromPool(NetworkObject obj)
        {
            obj.gameObject.SetActive(true);
        }

        private void OnReturnToPool(NetworkObject obj)
        {
            obj.gameObject.SetActive(false);
            if (poolRoot != null)
            {
                obj.transform.SetParent(poolRoot);
            }
        }

        private void OnDestroyPoolObject(NetworkObject obj)
        {
            Destroy(obj.gameObject);
        }

        public NetworkObject GetNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!_pools.ContainsKey(prefab))
            {
                Debug.LogError($"Pool for prefab {prefab.name} doesn't exist!");
                return null;
            }

            NetworkObject obj = _pools[prefab].Get();
            NetworkObject networkObject = obj.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                networkObject.transform.SetPositionAndRotation(position, rotation);
                if (IsServer)
                {
                    networkObject.Spawn();
                }
            }

            return networkObject;
        }

        public void ReturnNetworkObject(NetworkObject obj, GameObject prefab)
        {
            if (!_pools.ContainsKey(prefab))
            {
                Debug.LogError($"Pool for prefab {prefab.name} doesn't exist!");
                return;
            }

            if (IsServer)
            {
                obj.Despawn();
            }

            _pools[prefab].Release(obj);
        }
        [Serializable]
        public class PoolConfig
        {
            public NetworkObject prefab;
            public int defaultCapacity = 10;
            public int maxSize = 20;
            [Tooltip("Auto-expand pool if max size is reached")]
            public bool autoExpand;
        }
    }

    // Helper component to handle automatic return to pool
    public class PooledNetworkObject : MonoBehaviour
    {
        private NetworkObject _networkObject;
        private GameObject _prefab;
        public NetworkObjectPool Pool { get; set; }

        private void Awake()
        {
            _networkObject = GetComponent<NetworkObject>();
        }

        public void SetPrefab(GameObject prefab)
        {
            _prefab = prefab;
        }

        public void ReturnToPool()
        {
            if (Pool != null && _prefab != null)
            {
                Pool.ReturnNetworkObject(_networkObject, _prefab);
            }
        }
    }
}