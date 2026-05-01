using System;
using System.Threading.Tasks;
using KitchenClash.Domain;

namespace KitchenClash.Application.Services
{
    public sealed class ConfigServiceAdapter : IConfigService
    {
        private readonly IRemoteConfigService _remoteConfigService;

        public ConfigServiceAdapter(IRemoteConfigService remoteConfigService)
        {
            _remoteConfigService = remoteConfigService;
        }

        public T Get<T>(string key, T fallback)
        {
            return fallback; // TODO: implement via remote config
        }

        public Task FetchAsync()
        {
            return Task.CompletedTask; // TODO: delegate to remote config
        }
    }
}
