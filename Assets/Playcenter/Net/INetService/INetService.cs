namespace Playcenter.Net
{
    /// <summary>
    /// Network lifecycle abstraction. Game code never touches NetworkManager directly.
    /// </summary>
    public interface INetService
    {
        bool IsServer { get; }
        bool IsClient { get; }
        bool IsRunning { get; }
        int ConnectedClientCount { get; }

        void StartHost();
        void StartServer();
        void StartClient();
        void Shutdown();
    }
}
