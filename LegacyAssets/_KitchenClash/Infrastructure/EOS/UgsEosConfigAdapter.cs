using KitchenClash.Infrastructure.Configuration;
using Playcenter.EOS;

namespace KitchenClash.Infrastructure.EOS
{
    /// <summary>
    /// Maps KitchenClash <see cref="UGSConfig"/> ScriptableObject to <see cref="IEOSConfig"/>.
    /// </summary>
    public sealed class UgsEosConfigAdapter : IEOSConfig
    {
        private readonly UGSConfig _config;

        public UgsEosConfigAdapter(UGSConfig config)
        {
            _config = config;
        }

        public string UgsProjectId => _config != null ? _config.projectId : string.Empty;

        public string AuthenticationProfile =>
            _config != null && !string.IsNullOrEmpty(_config.authenticationProfile)
                ? _config.authenticationProfile
                : "default";

        public bool EnableUgsBridge => _config != null;
    }
}
