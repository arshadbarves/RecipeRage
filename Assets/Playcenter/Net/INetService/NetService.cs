using Unity.Netcode;
using UnityEngine;

namespace Playcenter.Net
{
    /// <summary>
    /// Wraps the injected NetworkManager instance (NOT NetworkManager.Singleton).
    /// </summary>
    public sealed class NetService : INetService
    {
        private readonly NetworkManager _networkManager;

        public bool IsServer => _networkManager.IsServer;
        public bool IsClient => _networkManager.IsClient;
        public bool IsRunning => _networkManager.IsListening;
        public int ConnectedClientCount => _networkManager.ConnectedClients.Count;

        public NetService(NetworkManager networkManager)
        {
            _networkManager = networkManager;
        }

        public void StartHost() => _networkManager.StartHost();
        public void StartServer() => _networkManager.StartServer();
        public void StartClient() => _networkManager.StartClient();
        public void Shutdown() => _networkManager.Shutdown();
    }
}
