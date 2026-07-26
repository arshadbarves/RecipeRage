using Playcenter.Services;

namespace KitchenClash.Infrastructure.Persistence
{
    /// <summary>
    /// Lazily supplies local + cloud storage providers.
    /// Cloud provider is injected via DI (ICloudStorageProvider) so this
    /// assembly never references EOS concrete types.
    /// </summary>
    public class StorageProviderFactory
    {
        private readonly ICloudStorageProvider _cloudProvider;
        private LocalStorageProvider _localProvider;

        public StorageProviderFactory(ICloudStorageProvider cloudProvider)
        {
            _cloudProvider = cloudProvider;
        }

        public IStorageProvider GetLocalProvider()
        {
            if (_localProvider == null)
            {
                _localProvider = new LocalStorageProvider();
            }

            return _localProvider;
        }

        public IStorageProvider GetCloudProvider() => _cloudProvider;
    }
}
