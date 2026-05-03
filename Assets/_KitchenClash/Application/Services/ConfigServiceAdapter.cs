using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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
            // IRemoteConfigService exposes typed IConfigModel objects, not raw string-keyed primitives.
            // Primitive key lookups (int/float/bool/string) are handled by FirebaseRemoteConfigService
            // when FIREBASE_REMOTE_CONFIG is defined. This adapter covers the non-Firebase path and
            // correctly returns the compile-time fallback, matching FallbackConfigService behaviour.
            return fallback;
        }

        public Task FetchAsync()
        {
            // Delegate to IRemoteConfigService.RefreshConfig so any cloud provider is triggered.
            // UniTask<bool> → Task via UniTask's AsTask() extension (Cysharp.Threading.Tasks).
            return _remoteConfigService.RefreshConfig().AsTask();
        }
    }
}
