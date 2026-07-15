namespace Playcenter.Services
{
    public interface ICloudStorageProvider : IStorageProvider
    {
        void OnUserLoggedIn();
        void OnUserLoggedOut();
    }
}
