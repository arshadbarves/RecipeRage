using System;
using KitchenClash.Domain.Interfaces;

namespace KitchenClash.Application.Services
{
    public sealed class ConfigServiceAdapter : IConfigService
    {
        private readonly IRemoteConfigService _remoteConfigService;

        public ConfigServiceAdapter(IRemoteConfigService remoteConfigService)
        {
            _remoteConfigService = remoteConfigService;
        }

        public int GetInt(string key, int defaultValue)
        {
            return defaultValue; // TODO: implement via remote config custom rules
        }

        public float GetFloat(string key, float defaultValue)
        {
            return defaultValue;
        }

        public bool GetBool(string key, bool defaultValue)
        {
            return defaultValue;
        }

        public string GetString(string key, string defaultValue)
        {
            return defaultValue;
        }
    }
}
