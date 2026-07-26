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
        private EOSPlayerDataTransport _transport;
        private string _rootPath;

        public bool IsReady { get; private set; }

        public EOSCloudStorageService(ILoggingService log)
        {
            _log = log;
        }

        /// <summary>Attach the EOS transport (called by PlaycenterCompositionRoot after auth).</summary>
        public void SetTransport(EOSPlayerDataTransport transport)
        {
            _transport = transport;
        }

        public IEnumerator Initialize()
        {
            _rootPath = Path.Combine(Application.persistentDataPath, "cloud");
            Directory.CreateDirectory(_rootPath);
            IsReady = true;
            _log.Log("[Storage] Initialized (local-persist mode, EOS transport pending)");
            yield break;
        }

        public async Task<bool> WriteFile(string key, byte[] data)
        {
            // Cloud-first: try EOS transport when available, fall back to local
            if (_transport != null && _transport.IsAvailable)
            {
                if (await _transport.Write(key, data))
                {
                    return true;
                }
                _log.Log($"[Storage] Cloud write unavailable for {key} — falling back to local");
            }

            try
            {
                File.WriteAllBytes(GetPath(key), data);
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError($"[Storage] Write failed for {key}: {ex.Message}");
                return false;
            }
        }

        public async Task<byte[]> ReadFile(string key)
        {
            // Cloud-first
            if (_transport != null && _transport.IsAvailable)
            {
                var cloudData = await _transport.Read(key);
                if (cloudData != null)
                {
                    return cloudData;
                }
            }

            var path = GetPath(key);
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                return File.ReadAllBytes(path);
            }
            catch (Exception ex)
            {
                _log.LogError($"[Storage] Read failed for {key}: {ex.Message}");
                return null;
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
