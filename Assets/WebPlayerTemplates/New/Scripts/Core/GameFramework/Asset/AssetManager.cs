using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace Core.GameFramework.Asset
{
    public class AssetManager
    {
        private readonly Dictionary<string, Object> _assetCache;
        private readonly IAssetLoader _assetLoader;
        private readonly HashSet<string> _loadingAssets;

        [Inject]
        public AssetManager(IAssetLoader assetLoader)
        {
            _assetLoader = assetLoader;
            _assetCache = new Dictionary<string, Object>();
            _loadingAssets = new HashSet<string>();
        }

        public async Task<T> LoadAsset<T>(string key, bool useCache = true) where T : Object
        {
            // Check cache first
            if (useCache && _assetCache.TryGetValue(key, out Object cachedAsset))
            {
                return cachedAsset as T;
            }

            // Prevent duplicate loading
            if (!_loadingAssets.Add(key))
            {
                while (_loadingAssets.Contains(key))
                {
                    await Task.Yield();
                }
                return _assetCache[key] as T;
            }

            try
            {
                T asset = await _assetLoader.LoadAssetAsync<T>(key);

                if (asset != null && useCache)
                {
                    _assetCache[key] = asset;
                }

                return asset;
            }
            finally
            {
                _loadingAssets.Remove(key);
            }
        }

        public async Task<GameObject> InstantiatePrefab(string key, Vector3 position, Quaternion rotation)
        {
            return await _assetLoader.InstantiateAsync(key, position, rotation);
        }

        public void ReleaseAsset<T>(T asset) where T : Object
        {
            _assetLoader.ReleaseAsset(asset);

            // Remove from cache if present
            string keyToRemove = "";
            foreach (KeyValuePair<string, Object> kvp in _assetCache)
            {
                if (kvp.Value == asset)
                {
                    keyToRemove = kvp.Key;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(keyToRemove))
            {
                _assetCache.Remove(keyToRemove);
            }
        }

        public void ClearCache()
        {
            _assetCache.Clear();
        }

        public async Task ClearAll()
        {
            ClearCache();
            await _assetLoader.ReleaseAllAssets();
        }

        public float GetLoadingProgress()
        {
            return _assetLoader.GetLoadingProgress();
        }

        public class AssetLoadException : Exception
        {
            public AssetLoadException(string message) : base(message) { }
        }
    }
}