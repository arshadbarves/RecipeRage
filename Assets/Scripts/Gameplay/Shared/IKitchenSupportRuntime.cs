namespace Gameplay.Shared
{
    /// <summary>
    /// Scene-owned kitchen bootstrap runtime used by the host to ensure required support stations exist.
    /// </summary>
    public interface IKitchenSupportRuntime
    {
        void EnsureKitchenSupportStations();
    }
}
