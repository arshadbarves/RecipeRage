using UnityEngine;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// MonoBehaviour wrapper for NetworkingServiceContainer
    /// </summary>
    public class NetworkingServiceUpdater : MonoBehaviour
    {
        private INetworkingServices _networkingServices;

        public void Initialize(INetworkingServices networkingServices)
        {
            _networkingServices = networkingServices;
        }

        private void Update()
        {
            if (_networkingServices is NetworkingServiceContainer container)
            {
                container.Update();
            }
        }

        private void OnDestroy()
        {
            _networkingServices = null;
        }
    }
}
