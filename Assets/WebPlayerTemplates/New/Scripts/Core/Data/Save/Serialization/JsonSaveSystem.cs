using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Core.Data.Save.Serialization
{
    public class JsonSaveSystem : ISaveSystem
    {
        private readonly Dictionary<string, SaveData> _memoryCache;
        private readonly string _savePath;

        public JsonSaveSystem()
        {
            _savePath = Path.Combine(Application.persistentDataPath, "SaveData");
            _memoryCache = new Dictionary<string, SaveData>();

            // Ensure save directory exists
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }
        }

        public async Task<bool> SaveData<T>(string key, T data)
        {
            string filePath = GetFilePath(key);
            try
            {
                string json = JsonUtility.ToJson(data);
                await File.WriteAllTextAsync(filePath, json);

                if (data is SaveData saveData)
                {
                    _memoryCache[key] = saveData;
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save data to {filePath}: {e.Message}");
                return false;
            }
        }

        public async Task<T> LoadData<T>(string key)
        {
            string filePath = GetFilePath(key);

            try
            {
                if (!File.Exists(filePath))
                {
                    return default;
                }

                string json = await File.ReadAllTextAsync(filePath);
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load data from {filePath}: {e.Message}");
                return default;
            }
        }

        public async Task<bool> DeleteData(string key)
        {
            string filePath = GetFilePath(key);
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _memoryCache.Remove(key);
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete data at {filePath}: {e.Message}");
                return false;
            }
        }

        public async Task<bool> HasData(string key)
        {
            string filePath = GetFilePath(key);
            return File.Exists(filePath);
        }

        public async Task<bool> ClearAll()
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(_savePath);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                _memoryCache.Clear();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to clear all data: {e.Message}");
                return false;
            }
        }

        private string GetFilePath(string key)
        {
            return Path.Combine(_savePath, $"{key}.json");
        }

        private async Task<bool> BackupSaveData(string backupPath)
        {
            try
            {
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }

                foreach (string file in Directory.GetFiles(_savePath))
                {
                    string fileName = Path.GetFileName(file);
                    string destFile = Path.Combine(backupPath, fileName);
                    File.Copy(file, destFile, true);
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to backup save data: {e.Message}");
                return false;
            }
        }

        public async Task<Dictionary<string, SaveData>> LoadAllSaveData()
        {
            Dictionary<string, SaveData> allData = new Dictionary<string, SaveData>();

            try
            {
                string[] files = Directory.GetFiles(_savePath, "*.json");
                foreach (string file in files)
                {
                    string key = Path.GetFileNameWithoutExtension(file);
                    SaveData data = await LoadData<SaveData>(key);
                    if (data != null)
                    {
                        allData[key] = data;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load all save data: {e.Message}");
            }

            return allData;
        }
    }
}