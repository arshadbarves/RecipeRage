using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Core.GameFramework.Asset
{
    public class AddressableLoader : IAssetLoader
    {
        private readonly Dictionary<GameObject, AsyncOperationHandle> _instantiatedObjects = new Dictionary<GameObject, AsyncOperationHandle>();
        private readonly Dictionary<object, AsyncOperationHandle> _loadedAssets = new Dictionary<object, AsyncOperationHandle>();

        public async Task<T> LoadAssetAsync<T>(string key) where T : Object
        {
            try
            {
                AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
                T result = await handle.Task;

                if (result != null)
                {
                    _loadedAssets[result] = handle;
                }

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load asset {key}: {e.Message}");
                return null;
            }
        }

        public async Task<GameObject> InstantiateAsync(string key, Vector3 position, Quaternion rotation)
        {
            try
            {
                AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(key, position, rotation);
                GameObject result = await handle.Task;

                if (result != null)
                {
                    _instantiatedObjects[result] = handle;
                }

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to instantiate asset {key}: {e.Message}");
                return null;
            }
        }

        public void ReleaseAsset<T>(T asset) where T : Object
        {
            if (asset == null) return;

            if (asset is GameObject go && _instantiatedObjects.TryGetValue(go, out AsyncOperationHandle instantiateHandle))
            {
                Addressables.ReleaseInstance(go);
                _instantiatedObjects.Remove(go);
            }
            else if (_loadedAssets.TryGetValue(asset, out AsyncOperationHandle handle))
            {
                Addressables.Release(handle);
                _loadedAssets.Remove(asset);
            }
        }

        public async Task ReleaseAllAssets()
        {
            foreach (AsyncOperationHandle handle in _loadedAssets.Values)
            {
                Addressables.Release(handle);
            }
            _loadedAssets.Clear();

            foreach (GameObject go in _instantiatedObjects.Keys)
            {
                if (go != null)
                {
                    Addressables.ReleaseInstance(go);
                }
            }
            _instantiatedObjects.Clear();

            await Addressables.CleanBundleCache();
        }

        public float GetLoadingProgress()
        {
            float totalProgress = 0f;
            int count = 0;

            foreach (AsyncOperationHandle handle in _loadedAssets.Values)
            {
                totalProgress += handle.PercentComplete;
                count++;
            }

            foreach (AsyncOperationHandle handle in _instantiatedObjects.Values)
            {
                totalProgress += handle.PercentComplete;
                count++;
            }

            return count > 0 ? totalProgress / count : 1f;
        }
    }
}