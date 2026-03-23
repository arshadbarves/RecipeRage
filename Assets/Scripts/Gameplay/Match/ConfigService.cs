using System;
using Core.RemoteConfig.Interfaces;
using Core.RemoteConfig.Models;

namespace Gameplay.Match
{
    /// <summary>
    /// Reads match tuning values from GameSettingsConfig custom rules with safe fallbacks.
    /// </summary>
    public sealed class ConfigService : IConfigService
    {
        private readonly IRemoteConfigService _remoteConfigService;

        public ConfigService(IRemoteConfigService remoteConfigService)
        {
            _remoteConfigService = remoteConfigService;
        }

        public int GetInt(string key, int defaultValue)
        {
            return TryGetRule(key, out object value) && TryConvertToInt(value, out int parsed)
                ? parsed
                : defaultValue;
        }

        public float GetFloat(string key, float defaultValue)
        {
            return TryGetRule(key, out object value) && TryConvertToFloat(value, out float parsed)
                ? parsed
                : defaultValue;
        }

        public bool GetBool(string key, bool defaultValue)
        {
            return TryGetRule(key, out object value) && TryConvertToBool(value, out bool parsed)
                ? parsed
                : defaultValue;
        }

        public string GetString(string key, string defaultValue)
        {
            return TryGetRule(key, out object value) && value != null
                ? value.ToString()
                : defaultValue;
        }

        private bool TryGetRule(string key, out object value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(key) || _remoteConfigService == null)
            {
                return false;
            }

            if (!_remoteConfigService.TryGetConfig<GameSettingsConfig>(out GameSettingsConfig config) ||
                config?.CustomGameRules == null)
            {
                return false;
            }

            return config.CustomGameRules.TryGetValue(key, out value);
        }

        private static bool TryConvertToInt(object value, out int parsed)
        {
            switch (value)
            {
                case null:
                    parsed = default;
                    return false;
                case int intValue:
                    parsed = intValue;
                    return true;
                case long longValue:
                    parsed = (int)longValue;
                    return true;
                case float floatValue:
                    parsed = (int)Math.Round(floatValue);
                    return true;
                case double doubleValue:
                    parsed = (int)Math.Round(doubleValue);
                    return true;
                case decimal decimalValue:
                    parsed = (int)Math.Round(decimalValue);
                    return true;
                case bool boolValue:
                    parsed = boolValue ? 1 : 0;
                    return true;
                default:
                    return int.TryParse(value.ToString(), out parsed);
            }
        }

        private static bool TryConvertToFloat(object value, out float parsed)
        {
            switch (value)
            {
                case null:
                    parsed = default;
                    return false;
                case float floatValue:
                    parsed = floatValue;
                    return true;
                case double doubleValue:
                    parsed = (float)doubleValue;
                    return true;
                case decimal decimalValue:
                    parsed = (float)decimalValue;
                    return true;
                case int intValue:
                    parsed = intValue;
                    return true;
                case long longValue:
                    parsed = longValue;
                    return true;
                default:
                    return float.TryParse(value.ToString(), out parsed);
            }
        }

        private static bool TryConvertToBool(object value, out bool parsed)
        {
            switch (value)
            {
                case null:
                    parsed = default;
                    return false;
                case bool boolValue:
                    parsed = boolValue;
                    return true;
                case int intValue:
                    parsed = intValue != 0;
                    return true;
                case long longValue:
                    parsed = longValue != 0L;
                    return true;
                default:
                    return bool.TryParse(value.ToString(), out parsed);
            }
        }
    }
}
