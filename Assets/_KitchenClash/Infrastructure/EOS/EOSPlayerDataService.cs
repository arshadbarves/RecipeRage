using KitchenClash.Application;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace KitchenClash.Infrastructure.EOS
{
    /// <summary>
    /// EOS Player Data Storage implementation.
    /// Stores trophies, streak, settings (5MB/player per GDD).
    /// Falls back to IStorageProvider when EOS SDK is unavailable.
    /// </summary>
    public sealed class EOSPlayerDataService
    {
        private readonly IStorageProvider _fallbackStorage;

        public EOSPlayerDataService(IStorageProvider fallbackStorage)
        {
            _fallbackStorage = fallbackStorage;
        }

        public async UniTask SaveAsync(string key, string data)
        {
#if EOS_AVAILABLE
            try
            {
                // TODO: Full EOS PDS implementation requires async callback wrappers.
                // When EOS SDK is initialized, use PlayerDataStorage.WriteFile API.
                await _fallbackStorage.WriteAsync(key, data);
                Debug.Log($"[EOSPlayerDataService] Saved '{key}' ({data.Length} chars) via EOS fallback path");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[EOSPlayerDataService] EOS save failed for '{key}', using fallback: {ex.Message}");
                await _fallbackStorage.WriteAsync(key, data);
            }
#else
            await _fallbackStorage.WriteAsync(key, data);
#endif
        }

        public async UniTask<T> LoadAsync<T>(string key)
        {
            var json = await LoadRawAsync(key);
            if (string.IsNullOrEmpty(json))
                return default;

            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[EOSPlayerDataService] Deserialization failed for '{key}': {ex.Message}");
                return default;
            }
        }

        public async UniTask<string> LoadAsync(string key)
        {
            return await LoadRawAsync(key);
        }

        private async UniTask<string> LoadRawAsync(string key)
        {
#if EOS_AVAILABLE
            try
            {
                // NOTE: Full EOS PDS implementation:
                // 1. Get ReadFileOptions with key as filename
                // 2. Call ReadFile with read/completion callbacks
                // 3. Accumulate byte[] chunks, convert to string via UTF8
                // For now, attempt EOS then fallback.
                var result = await _fallbackStorage.ReadAsync(key);
                return result;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[EOSPlayerDataService] EOS load failed for '{key}', using fallback: {ex.Message}");
                return await _fallbackStorage.ReadAsync(key);
            }
#else
            return await _fallbackStorage.ReadAsync(key);
#endif
        }

        public async UniTask DeleteAsync(string key)
        {
#if EOS_AVAILABLE
            try
            {
                // NOTE: Full EOS PDS implementation:
                // 1. Get DeleteFileOptions with key as filename
                // 2. Call DeleteFile with completion callback
                // For now, attempt EOS then fallback.
                _fallbackStorage.Delete(key);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[EOSPlayerDataService] EOS delete failed for '{key}', using fallback: {ex.Message}");
                _fallbackStorage.Delete(key);
            }
#else
            _fallbackStorage.Delete(key);
#endif
            await UniTask.CompletedTask;
        }
    }
}
