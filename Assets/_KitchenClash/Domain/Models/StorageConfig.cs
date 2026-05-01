namespace KitchenClash.Domain
{
    public sealed class StorageConfig
    {
        public string Key { get; }
        public StorageStrategy Strategy { get; }
        public bool EncryptData { get; }

        public StorageConfig(string key, StorageStrategy strategy, bool encryptData = false)
        {
            Key = key;
            Strategy = strategy;
            EncryptData = encryptData;
        }
    }
}
