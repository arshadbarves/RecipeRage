using System;
using System.IO;
using Core.Core.Logging;
using Core.Core.Persistence.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Core.Persistence.Providers
{
    /// <summary>
    /// Local file system storage provider.
    /// Fast, offline storage for settings and cache.
    /// </summary>
    public class LocalStorageProvider : IStorageProvider
    {
        private readonly string _basePath;

        public bool IsAvailable => true; // Always available

        public LocalStorageProvider()
        {
            _basePath = Application.persistentDataPath;

            // Ensure directory exists
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public string Read(string key)
        {
            string path = GetFilePath(key);

            try
            {
                if (!File.Exists(path))
                {
                    return null;
                }

                return File.ReadAllText(path);
            }
            catch (Exception e)
            {
                GameLogger.LogError($"Error reading {key}: {e.Message}");
                return null;
            }
        }

        public void Write(string key, string content)
        {
            string path = GetFilePath(key);

            try
            {
                File.WriteAllText(path, content);
            }
            catch (Exception e)
            {
                GameLogger.LogError($"Error writing {key}: {e.Message}");
            }
        }

        public async UniTask<string> ReadAsync(string key)
        {
            // For local storage, async is just a wrapper around sync
            return await UniTask.RunOnThreadPool(() => Read(key));
        }

        public async UniTask WriteAsync(string key, string content)
        {
            // For local storage, async is just a wrapper around sync
            await UniTask.RunOnThreadPool(() => Write(key, content));
        }

        public bool Exists(string key)
        {
            string path = GetFilePath(key);
            return File.Exists(path);
        }

        public void Delete(string key)
        {
            string path = GetFilePath(key);

            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception e)
                {
                    GameLogger.LogError($"Error deleting {key}: {e.Message}");
                }
            }
        }

        public async UniTask DeleteAsync(string key)
        {
            await UniTask.RunOnThreadPool(() => Delete(key));
        }

        private string GetFilePath(string key)
        {
            return Path.Combine(_basePath, key);
        }
    }
}
