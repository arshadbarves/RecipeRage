using System;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using Epic.OnlineServices.PlayerDataStorage;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;
using UnityEngine;

namespace Core.SaveSystem
{
    /// <summary>
    /// Epic Online Services cloud storage provider.
    /// Persistent, cross-device storage for player data.
    /// Wraps EOS PlayerDataStorageService.
    /// </summary>
    public class EOSCloudStorageProvider : IStorageProvider
    {
        private readonly PlayerDataStorageService _eosStorage;
        private bool _isInitialized;

        public bool IsAvailable => _isInitialized && IsUserLoggedIn();

        public EOSCloudStorageProvider()
        {
            _eosStorage = PlayerDataStorageService.Instance;

            // Check if already logged in
            _isInitialized = IsUserLoggedIn();
            
            // Refresh file list if already logged in
            if (_isInitialized)
            {
                RefreshFileList();
            }
        }

        public string Read(string key)
        {
            if (!IsAvailable)
            {
                Debug.LogWarning("[EOSCloudStorageProvider] Cannot read - user not logged in");
                return null;
            }

            // Check if already cached
            string cachedData = _eosStorage.GetCachedFileContent(key);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return cachedData;
            }

            // Synchronous read not recommended for cloud storage
            // Return null and log warning
            Debug.LogWarning($"[EOSCloudStorageProvider] Synchronous read for {key} - use ReadAsync instead");
            return null;
        }

        public void Write(string key, string content)
        {
            if (!IsAvailable)
            {
                Debug.LogWarning("[EOSCloudStorageProvider] Cannot write - user not logged in");
                return;
            }

            // Synchronous write not recommended for cloud storage
            // Queue async write instead
            Debug.LogWarning($"[EOSCloudStorageProvider] Synchronous write for {key} - use WriteAsync instead");
            WriteAsync(key, content).Forget();
        }

        public async UniTask<string> ReadAsync(string key)
        {
            if (!IsAvailable)
            {
                Debug.LogWarning("[EOSCloudStorageProvider] Cannot read - user not logged in");
                return null;
            }

            try
            {
                // Check if already cached
                string cachedData = _eosStorage.GetCachedFileContent(key);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    return cachedData;
                }

                // Download from cloud
                var tcs = new UniTaskCompletionSource<string>();

                _eosStorage.DownloadFile(key, () =>
                {
                    string downloadedData = _eosStorage.GetCachedFileContent(key);
                    tcs.TrySetResult(downloadedData);
                });

                // Wait for download with timeout
                var result = await tcs.Task.Timeout(TimeSpan.FromSeconds(30));
                return result;
            }
            catch (TimeoutException)
            {
                Debug.LogError($"[EOSCloudStorageProvider] Timeout reading {key}");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[EOSCloudStorageProvider] Error reading {key}: {e.Message}");
                return null;
            }
        }

        public async UniTask WriteAsync(string key, string content)
        {
            if (!IsAvailable)
            {
                Debug.LogWarning("[EOSCloudStorageProvider] Cannot write - user not logged in");
                return;
            }

            try
            {
                var tcs = new UniTaskCompletionSource();

                _eosStorage.AddFile(key, content, () =>
                {
                    tcs.TrySetResult();
                });

                // Wait for upload with timeout
                await tcs.Task.Timeout(TimeSpan.FromSeconds(30));
            }
            catch (TimeoutException)
            {
                Debug.LogError($"[EOSCloudStorageProvider] Timeout writing {key}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[EOSCloudStorageProvider] Error writing {key}: {e.Message}");
            }
        }

        public bool Exists(string key)
        {
            if (!IsAvailable)
            {
                return false;
            }

            // Check if file exists in cached file list
            var cachedData = _eosStorage.GetLocallyCachedData();
            return cachedData.ContainsKey(key);
        }

        public void Delete(string key)
        {
            if (!IsAvailable)
            {
                Debug.LogWarning("[EOSCloudStorageProvider] Cannot delete - user not logged in");
                return;
            }

            _eosStorage.DeleteFile(key);
        }

        public async UniTask DeleteAsync(string key)
        {
            await UniTask.RunOnThreadPool(() => Delete(key));
        }

        /// <summary>
        /// Query file list from cloud (call after login).
        /// </summary>
        public void RefreshFileList()
        {
            if (IsAvailable)
            {
                _eosStorage.QueryFileList();
            }
        }

        private bool IsUserLoggedIn()
        {
            try
            {
                ProductUserId userId = EOSManager.Instance?.GetProductUserId();
                return userId != null && userId.IsValid();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Call this after successful login to refresh file list and enable cloud storage.
        /// Should be called by SaveService when authentication succeeds.
        /// </summary>
        public void OnUserLoggedIn()
        {
            _isInitialized = IsUserLoggedIn();
            
            if (_isInitialized)
            {
                RefreshFileList();
            }
        }
    }
}
