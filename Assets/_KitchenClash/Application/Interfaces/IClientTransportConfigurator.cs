namespace KitchenClash.Application
{
    /// <summary>
    /// Configures the active network transport so a client can connect to a host.
    /// Infrastructure adapters bind this to the concrete transport (e.g. EOS P2P).
    /// </summary>
    public interface IClientTransportConfigurator
    {
        /// <summary>
        /// Sets the remote host identity on the transport for the current NetworkManager.
        /// Returns false when transport is missing or misconfigured.
        /// </summary>
        bool TryConfigureHostConnection(string hostUserId);
    }
}
