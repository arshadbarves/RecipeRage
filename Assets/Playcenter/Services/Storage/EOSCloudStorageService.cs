using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Playcenter.Services
{
    /// <summary>
    /// EOS Player Data Storage provider. Until the EOS transport is wired (Slice 2),
    /// persists to Application.persistentDataPath under the same interface.
    /// </summary>
    public sealed class EOSCloudStorageService : IStorageService
    {
        private readonly ILoggingService _log;
        private string _rootPath;

        public bool IsReady { get; private set; }

        public EOSCloudStorageService(ILoggingService log)
        {
            _log = log;
        }

        public IEnumerator Initialize()
        {
            _rootPath = Path.Combine(Application.persistentDataPath, "cloud");
            Directory.CreateDirectory(_rootPath);
            IsReady = true;
            _log.Log("[Storage] Initialized (local-persist mode, EOS transport pending)");
            yield break;
        }

        public Task<bool> WriteFile(string key, byte[] data)
        {
            try
            {
                File.WriteAllBytes(GetPath(key), data);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _log.LogError($"[Storage] Write failed for {key}: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        public Task<byte[]> ReadFile(string key)
        {
            var path = GetPath(key);
            if (!File.Exists(path))
            {
                return Task.FromResult<byte[]>(null);
            }

            try
            {
                return Task.FromResult(File.ReadAllBytes(path));
            }
            catch (Exception ex)
            {
                _log.LogError($"[Storage] Read failed for {key}: {ex.Message}");
                return Task.FromResult<byte[]>(null);
            }
        }

        public Task<bool> DeleteFile(string key)
        {
            var path = GetPath(key);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            return Task.FromResult(true);
        }

        private string GetPath(string key)
        {
            var safeKey = string.Concat(key.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
            return Path.Combine(_rootPath, safeKey + ".dat");
        }
    }
}
