using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.RemoteConfig;

namespace Core.Core.Auth
{
    public interface IFirebaseWrapper
    {
        UniTask SetDefaultsAsync(Dictionary<string, object> defaults);
        UniTask FetchAsync(TimeSpan cacheExpiration);
        UniTask ActivateAsync();
        string GetStringValue(string key);
    }

    public class FirebaseWrapper : IFirebaseWrapper
    {
        public async UniTask SetDefaultsAsync(Dictionary<string, object> defaults)
        {
            await FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults);
        }

        public async UniTask FetchAsync(TimeSpan cacheExpiration)
        {
            await FirebaseRemoteConfig.DefaultInstance.FetchAsync(cacheExpiration);
        }

        public async UniTask ActivateAsync()
        {
            await FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
        }

        public string GetStringValue(string key)
        {
            return FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
        }
    }
}
