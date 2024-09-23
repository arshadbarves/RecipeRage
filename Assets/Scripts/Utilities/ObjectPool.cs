using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Utilities
{
    public class ObjectPoolManager : MonoSingleton<ObjectPoolManager>
    {
        private readonly Dictionary<string, IObjectPool<GameObject>> _pools = new Dictionary<string, IObjectPool<GameObject>>();

        public IObjectPool<GameObject> CreatePool(string poolKey, GameObject prefab, int defaultCapacity = 10,
            int maxSize = 20)
        {
            if (_pools.TryGetValue(poolKey, out var existingPool))
            {
                Debug.LogWarning($"Pool with key {poolKey} already exists. Returning existing pool.");
                return existingPool;
            }

            IObjectPool<GameObject> pool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(prefab),
                actionOnGet: (obj) => obj.SetActive(true),
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => Destroy(obj),
                collectionCheck: true,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );

            _pools[poolKey] = pool;
            return pool;
        }

        public GameObject GetObjectFromPool(string poolKey)
        {
            if (!_pools.ContainsKey(poolKey))
            {
                Debug.LogError($"Pool with key {poolKey} does not exist.");
                return null;
            }

            return _pools[poolKey].Get();
        }

        public void ReturnObjectToPool(string poolKey, GameObject obj)
        {
            if (!_pools.ContainsKey(poolKey))
            {
                Debug.LogError($"Pool with key {poolKey} does not exist.");
                return;
            }

            _pools[poolKey].Release(obj);
        }
    }
}