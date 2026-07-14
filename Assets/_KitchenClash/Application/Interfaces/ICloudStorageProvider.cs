namespace KitchenClash.Application
{
    /// <summary>
    /// Cloud-backed storage with auth lifecycle hooks.
    /// Infrastructure adapters (e.g. EOS Player Data Storage) implement this;
    /// Persistence depends only on the port — never on vendor types.
    /// </summary>
    public interface ICloudStorageProvider : IStorageProvider
    {
        void OnUserLoggedIn();
        void OnUserLoggedOut();
    }
}
