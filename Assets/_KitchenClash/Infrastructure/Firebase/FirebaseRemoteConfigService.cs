using System;
using System.Threading.Tasks;
using KitchenClash.Domain;
using Firebase.RemoteConfig;

namespace KitchenClash.Infrastructure.Firebase
{
    /// <summary>
    /// GDD Section 9: FirebaseRemoteConfigService implements IConfigService.
    /// Every tunable value is an IConfigService key with fallback.
    /// </summary>
    public sealed class FirebaseRemoteConfigService : IConfigService
    {
        public T Get<T>(string key, T fallback)
        {
            var v = FirebaseRemoteConfig.DefaultInstance.GetValue(key);
            if (v.Source == ValueSource.DefaultValue) return fallback;

            if (typeof(T) == typeof(int)) return (T)(object)(int)v.LongValue;
            if (typeof(T) == typeof(float)) return (T)(object)(float)v.DoubleValue;
            if (typeof(T) == typeof(bool)) return (T)(object)v.BooleanValue;
            if (typeof(T) == typeof(string)) return (T)(object)v.StringValue;

            return fallback;
        }

        public async Task FetchAsync()
        {
            await FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.FromHours(4));
            await FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
        }
    }
}
