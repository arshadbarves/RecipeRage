using KitchenClash.Application;
using System;
using System.Collections.Generic;
using KitchenClash.Domain;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;

namespace KitchenClash.Infrastructure.EOS
{
    public class EOSCloudStorageProvider : IStorageProvider
    {
        private readonly PlayerDataStorageService _eosStorage;
        private bool _isInitialized;

        public bool IsAvailable => _isInitialized && IsUserLoggedIn();

        public EOSCloudStorageProvider()
        {
            _eosStorage = PlayerDataStorageService.Instance;

            _isInitialized = IsUserLoggedIn();

            if (_isInitialized)
            {
                RefreshFileList();
            }
        }

        public string Read(string key)
        {
            if (!IsAvailable)
            {
                GameLogger.LogWarning("Cannot read - user not logged in");
                return null;
            }

            string cachedData = _eosStorage.GetCachedFileContent(key);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return cachedData;
            }

            GameLogger.LogWarning($"Synchronous read for {key} - use ReadAsync instead");
            return null;
        }

        public void Write(string key, string content)
        {
            if (!IsAvailable)
            {
                GameLogger.LogWarning("Cannot write - user not logged in");
                return;
            }

            GameLogger.LogWarning($"Synchronous write for {key} - use WriteAsync instead");
            WriteAsync(key, content).Forget();
        }

        public async UniTask<string> ReadAsync(string key)
        {
            if (!IsAvailable)
            {
                GameLogger.LogWarning("Cannot read - user not logged in");
                return null;
            }

            try
            {
                string cachedData = _eosStorage.GetCachedFileContent(key);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    return cachedData;
                }

                var tcs = new UniTaskCompletionSource<string>();

                _eosStorage.DownloadFile(key, () =>
                {
                    string downloadedData = _eosStorage.GetCachedFileContent(key);
                    tcs.TrySetResult(downloadedData);
                });

                string result = await tcs.Task.Timeout(TimeSpan.FromSeconds(30));
                return result;
            }
            catch (TimeoutException)
            {
                GameLogger.LogError($"Timeout reading {key}");
                return null;
            }
            catch (Exception e)
            {
                GameLogger.LogError($"Error reading {key}: {e.Message}");
                return null;
            }
        }

        public async UniTask WriteAsync(string key, string content)
        {
            if (!IsAvailable)
            {
                GameLogger.LogWarning("Cannot write - user not logged in");
                return;
            }

            try
            {
                var tcs = new UniTaskCompletionSource();

                _eosStorage.AddFile(key, content, () =>
                {
                    tcs.TrySetResult();
                });

                await tcs.Task.Timeout(TimeSpan.FromSeconds(30));
            }
            catch (TimeoutException)
            {
                GameLogger.LogError($"Timeout writing {key}");
            }
            catch (Exception e)
            {
                GameLogger.LogError($"Error writing {key}: {e.Message}");
            }
        }

        public bool Exists(string key)
        {
            if (!IsAvailable)
            {
                return false;
            }

            Dictionary<string, string> cachedData = _eosStorage.GetLocallyCachedData();
            return cachedData.ContainsKey(key);
        }

        public void Delete(string key)
        {
            if (!IsAvailable)
            {
                GameLogger.LogWarning("Cannot delete - user not logged in");
                return;
            }

            _eosStorage.DeleteFile(key);
        }

        public async UniTask DeleteAsync(string key)
        {
            await UniTask.RunOnThreadPool(() => Delete(key));
        }

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

        public void OnUserLoggedIn()
        {
            _isInitialized = IsUserLoggedIn();

            if (_isInitialized)
            {
                RefreshFileList();
            }
        }

        public void OnUserLoggedOut()
        {
            GameLogger.Log("User logged out - clearing cached cloud data");

            if (_eosStorage != null)
            {
                Dictionary<string, string> cachedData = _eosStorage.GetLocallyCachedData();
                if (cachedData != null)
                {
                    cachedData.Clear();
                }
            }

            _isInitialized = false;

            GameLogger.Log("Cloud cache cleared");
        }
    }
}
