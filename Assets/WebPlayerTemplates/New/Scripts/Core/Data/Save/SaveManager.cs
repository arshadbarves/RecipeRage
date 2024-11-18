using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Data.Save.Serialization;
using UnityEngine;
using VContainer;

namespace Core.Data.Save
{
    public class SaveManager
    {
        private const int SaveVersion = 1;
        private readonly Dictionary<string, object> _cache;
        private readonly ISaveSystem _saveSystem;

        [Inject]
        public SaveManager(ISaveSystem saveSystem)
        {
            _saveSystem = saveSystem;
            _cache = new Dictionary<string, object>();
        }

        public async Task<bool> SaveData<T>(string key, T data)
        {
            try
            {
                string serialized = SerializationHelper.Serialize(data);
                SaveData saveData = new SaveData(key, serialized);

                bool success = await _saveSystem.SaveData(key, saveData);
                if (success)
                {
                    _cache[key] = data;
                }
                return success;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save data for key {key}: {e.Message}");
                return false;
            }
        }

        public async Task<T> LoadData<T>(string key)
        {
            if (_cache.TryGetValue(key, out object cached))
            {
                return (T)cached;
            }

            try
            {
                SaveData saveData = await _saveSystem.LoadData<SaveData>(key);
                if (saveData == null) return default;

                T data = SerializationHelper.Deserialize<T>(saveData.data);
                _cache[key] = data;
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load data for key {key}: {e.Message}");
                return default;
            }
        }

        public async Task<bool> DeleteData(string key)
        {
            try
            {
                bool success = await _saveSystem.DeleteData(key);
                if (success)
                {
                    _cache.Remove(key);
                }
                return success;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete data for key {key}: {e.Message}");
                return false;
            }
        }

        public async Task<bool> HasData(string key)
        {
            return await _saveSystem.HasData(key);
        }

        public async Task<bool> ClearAll()
        {
            try
            {
                bool success = await _saveSystem.ClearAll();
                if (success)
                {
                    _cache.Clear();
                }
                return success;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to clear all data: {e.Message}");
                return false;
            }
        }

        public void ClearCache()
        {
            _cache.Clear();
        }

        public async Task<bool> BackupData(string backupPath)
        {
            try
            {
                // Implement backup logic
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to backup data: {e.Message}");
                return false;
            }
        }

        public async Task<bool> RestoreBackup(string backupPath)
        {
            try
            {
                // Implement restore logic
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to restore backup: {e.Message}");
                return false;
            }
        }
    }
}