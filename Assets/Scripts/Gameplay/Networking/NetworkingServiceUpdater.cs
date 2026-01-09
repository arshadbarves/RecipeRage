using Gameplay.App.Networking;
using UnityEngine;

namespace Core.Networking
{
    /// <summary>
    /// MonoBehaviour wrapper for NetworkingServiceContainer
    /// Handles Update loop for services that need it (Matchmaking, P2P)
    /// </summary>
    public class NetworkingServiceUpdater : MonoBehaviour
    {
        private INetworkingServices _networkingServices;

        /// <summary>
        /// Initialize with networking services
        /// </summary>
        public void Initialize(INetworkingServices networkingServices)
        {
            _networkingServices = networkingServices;
        }

        /// <summary>
        /// Update networking services
        /// </summary>
        private void Update()
        {
            if (_networkingServices is NetworkingServiceContainer container)
            {
                container.Update();
            }
        }

        /// <summary>
        /// Cleanup on destroy
        /// </summary>
        private void OnDestroy()
        {
            _networkingServices = null;
        }
    }
}
