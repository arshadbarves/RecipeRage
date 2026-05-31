using KitchenClash.Application;
using KitchenClash.Infrastructure.EOS;

namespace KitchenClash.Infrastructure.Persistence
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
    }
}
