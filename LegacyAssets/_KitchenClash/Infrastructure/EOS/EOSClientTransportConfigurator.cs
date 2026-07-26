using System;
using KitchenClash.Application;
using Epic.OnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples.Network;
using Unity.Netcode;
using Playcenter.Shell;
using Playcenter.Services;

namespace KitchenClash.Infrastructure.EOS
{
    /// <summary>
    /// Configures EOS P2P transport host target for NGO client start.
    /// Implements both legacy <see cref="IClientTransportConfigurator"/> and Playcenter
    /// <see cref="INetTransportConfigurator"/>.
    /// </summary>
    public sealed class EOSClientTransportConfigurator : IClientTransportConfigurator, INetTransportConfigurator
    {
        public bool TryConfigureHostConnection(string hostUserId)
        {
            return TrySetServerUserId(hostUserId);
        }

        /// <inheritdoc />
        public void ConfigureForSession(NetRole role, string sessionToken)
        {
            if (role == NetRole.Host)
            {
                // Host does not dial out; clear any stale client target if transport is present.
                TryClearServerUserId();
                return;
            }

            if (!TrySetServerUserId(sessionToken ?? string.Empty))
            {
                throw new InvalidOperationException(
                    "Failed to configure EOS client transport for session (missing host user id or EOSTransport).");
            }
        }

        private static bool TrySetServerUserId(string hostUserId)
        {
            if (string.IsNullOrEmpty(hostUserId))
            {
                GameLogger.LogError("Host user id is empty — cannot configure EOS transport");
                return false;
            }

            if (!TryGetEosTransport(out EOSTransport transport))
            {
                return false;
            }

            try
            {
                transport.ServerUserIdToConnectTo = ProductUserId.FromString(hostUserId);
                return true;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to parse host user id '{hostUserId}': {ex.Message}");
                return false;
            }
        }

        private static void TryClearServerUserId()
        {
            if (!TryGetEosTransport(out EOSTransport transport))
            {
                return;
            }

            transport.ServerUserIdToConnectTo = default;
        }

        private static bool TryGetEosTransport(out EOSTransport transport)
        {
            // Prefer injected NetworkManager source when available; Singleton matches prior behavior.
            NetworkManager networkManager = NetworkManager.Singleton;
            transport = networkManager != null
                ? networkManager.GetComponent<EOSTransport>()
                : null;

            if (transport == null)
            {
                GameLogger.LogError("EOSTransport component not found on NetworkManager!");
                return false;
            }

            return true;
        }
    }
}
