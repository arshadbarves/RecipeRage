using Core.Persistence.Interfaces;
using Core.Persistence.Models;
using Core.Persistence.Providers;

namespace Core.Persistence.Factory
{
    public class StorageProviderFactory
    {
        private LocalStorageProvider _localProvider;
        private EOSCloudStorageProvider _cloudProvider;

        public IStorageProvider GetLocalProvider()
        {
            if (_localProvider == null)
            {
                _localProvider = new LocalStorageProvider();
            }
            return _localProvider;
        }

        public IStorageProvider GetCloudProvider()
        {
            if (_cloudProvider == null)
            {
                _cloudProvider = new EOSCloudStorageProvider();
            }
            return _cloudProvider;
        }


        public IStorageProvider GetProvider(StorageStrategy strategy)
        {
            return strategy switch
            {
                StorageStrategy.LocalOnly => GetLocalProvider(),
                StorageStrategy.CloudOnly => GetCloudProvider(),
                StorageStrategy.CloudWithCache => GetCloudProvider(), // SaveService handles caching
                _ => GetLocalProvider()
            };
        }
    }
}
