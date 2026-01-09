using Core.Core.Persistence.Interfaces;
using Core.Core.Persistence.Models;
using Core.Core.Persistence.Providers;

namespace Core.Core.Persistence.Factory
{
    /// <summary>
    /// Factory for creating storage providers.
    /// Follows Factory Pattern for extensibility.
    /// </summary>
    public class StorageProviderFactory
    {
        private LocalStorageProvider _localProvider;
        private EOSCloudStorageProvider _cloudProvider;

        /// <summary>
        /// Get or create local storage provider (singleton per factory).
        /// </summary>
        public IStorageProvider GetLocalProvider()
        {
            if (_localProvider == null)
            {
                _localProvider = new LocalStorageProvider();
            }
            return _localProvider;
        }

        /// <summary>
        /// Get or create cloud storage provider (singleton per factory).
        /// </summary>
        public IStorageProvider GetCloudProvider()
        {
            if (_cloudProvider == null)
            {
                _cloudProvider = new EOSCloudStorageProvider();
            }
            return _cloudProvider;
        }

        /// <summary>
        /// Get provider based on strategy.
        /// </summary>
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
