using KitchenClash.Application;
using KitchenClash.Domain;
using Epic.OnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples.Network;
using Unity.Netcode;

namespace KitchenClash.Infrastructure.EOS
{
    /// <summary>
    /// Configures EOS P2P transport host target for NGO client start.
    /// </summary>
    public sealed class EOSClientTransportConfigurator : IClientTransportConfigurator
    {
        public bool TryConfigureHostConnection(string hostUserId)
        {
            if (string.IsNullOrEmpty(hostUserId))
            {
                GameLogger.LogError("Host user id is empty — cannot configure EOS transport");
                return false;
            }

            NetworkManager networkManager = NetworkManager.Singleton;
            EOSTransport transport = networkManager != null
                ? networkManager.GetComponent<EOSTransport>()
                : null;

            if (transport == null)
            {
                GameLogger.LogError("EOSTransport component not found on NetworkManager!");
                return false;
            }

            transport.ServerUserIdToConnectTo = ProductUserId.FromString(hostUserId);
            return true;
        }
    }
}
