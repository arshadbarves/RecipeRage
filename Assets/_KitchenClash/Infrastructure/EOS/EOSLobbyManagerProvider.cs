using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;

namespace KitchenClash.Infrastructure.EOS
{
    /// <summary>
    /// Provides the EOS SDK's EOSLobbyManager sub-manager.
    ///
    /// EOSLobbyManager lives in the com.playeveryware.eos.samples assembly, which
    /// only KitchenClash.Infrastructure references. Composition cannot reference
    /// that assembly directly, so it resolves EOSLobbyManager through this provider.
    /// </summary>
    public static class EOSLobbyManagerProvider
    {
        /// <summary>
        /// Returns the EOSLobbyManager singleton obtained from EOSManager.
        /// Safe to call after EOSManager has initialised (i.e. during session scope construction).
        /// </summary>
        public static EOSLobbyManager Get()
            => EOSManager.Instance.GetOrCreateManager<EOSLobbyManager>();
    }
}
