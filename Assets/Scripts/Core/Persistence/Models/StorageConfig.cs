using System;

namespace Core.Core.Persistence.Models
{
    /// <summary>
    /// Configuration for a specific data type's storage.
    /// </summary>
    [Serializable]
    public class StorageConfig
    {
        public string Key { get; set; }
        public StorageStrategy Strategy { get; set; }
        public bool EncryptData { get; set; }

        public StorageConfig(string key, StorageStrategy strategy, bool encryptData = false)
        {
            Key = key;
            Strategy = strategy;
            EncryptData = encryptData;
        }
    }
}
