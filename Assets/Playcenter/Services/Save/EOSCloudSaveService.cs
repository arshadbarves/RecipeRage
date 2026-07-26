using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Playcenter.Services
{
    /// <summary>
    /// Save layer backed by IStorageService (EOS Cloud Storage in production).
    /// Writes are cached in memory immediately and flushed to storage on Flush()
    /// (called on pause / quit / match end), so gameplay never blocks on IO.
    /// </summary>
    public sealed class EOSCloudSaveService : ISaveService
    {
        private readonly IStorageService _storage;
        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>(64);
        private bool _dirty;

        public EOSCloudSaveService(IStorageService storage)
        {
            _storage = storage;
        }

        public void Save<T>(string key, T value)
        {
            _cache[key] = JsonUtility.ToJson(value);
            _dirty = true;
        }

        public T Load<T>(string key, T fallback)
        {
            if (_cache.TryGetValue(key, out var json))
            {
                return JsonUtility.FromJson<T>(json);
            }

            // Synchronous first-load path: storage read is awaited by caller via Preload.
            return fallback;
        }

        public bool Has(string key) => _cache.ContainsKey(key);

        public void Delete(string key)
        {
            _cache.Remove(key);
            _dirty = true;
        }

        /// <summary>
        /// Preloads keys from storage into memory cache. Called once after auth.
        /// </summary>
        public async Task Preload(string[] keys)
        {
            foreach (var key in keys)
            {
                var bytes = await _storage.ReadFile(key);
                if (bytes != null)
                {
                    _cache[key] = Encoding.UTF8.GetString(bytes);
                }
            }
        }

        public async Task Flush()
        {
            if (!_dirty)
            {
                return;
            }

            foreach (var kvp in _cache)
            {
                await _storage.WriteFile(kvp.Key, Encoding.UTF8.GetBytes(kvp.Value));
            }
            _dirty = false;
        }
    }
}
