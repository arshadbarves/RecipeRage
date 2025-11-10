# Firebase SDK Integration Code

## Complete FirebaseConfigProvider Implementation

Replace the TODO comments in `Assets/Scripts/Core/RemoteConfig/Providers/FirebaseConfigProvider.cs` with this code:

### Initialize Method (Line ~40)
```csharp
// Replace this comment:
// TODO: Initialize Firebase Remote Config SDK

// With this code:
await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
var remoteConfig = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance;

var configSettings = new Firebase.RemoteConfig.ConfigSettings
{
    MinimumFetchInternalInMilliseconds = 3600000 // 1 hour cache
};
await remoteConfig.SetConfigSettingsAsync(configSettings);

await remoteConfig.FetchAsync(TimeSpan.FromHours(1));
await remoteConfig.ActivateAsync();
```

### FetchConfig Method (Line ~73)
```csharp
// Replace this comment:
// TODO: Fetch from Firebase Remote Config

// With this code:
var remoteConfig = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance;
var jsonString = remoteConfig.GetValue(key).StringValue;

if (string.IsNullOrEmpty(jsonString))
{
    GameLogger.LogWarning($"Firebase returned empty value for key: {key}");
    return default(T);
}

var config = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonString);
return config;
```

## Complete Updated File

Here's the complete `FirebaseConfigProvider.cs` with Firebase integrated:

```csharp
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Core.Logging;
using Core.RemoteConfig.Models;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.RemoteConfig.Providers
{
    public class FirebaseConfigProvider : IConfigProvider
    {
        public string ProviderName => "Firebase";
        
        private bool _isInitialized;
        
        public FirebaseConfigProvider()
        {
            _isInitialized = false;
        }
        
        public bool IsAvailable()
        {
            return _isInitialized;
        }
        
        public async UniTask<bool> Initialize()
        {
            try
            {
                GameLogger.Log("Initializing Firebase Remote Config provider...");
                
                // Initialize Firebase
                await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
                var remoteConfig = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance;
                
                // Configure settings
                var configSettings = new Firebase.RemoteConfig.ConfigSettings
                {
                    MinimumFetchInternalInMilliseconds = 3600000 // 1 hour
                };
                await remoteConfig.SetConfigSettingsAsync(configSettings);
                
                // Fetch and activate
                await remoteConfig.FetchAsync(TimeSpan.FromHours(1));
                await remoteConfig.ActivateAsync();
                
                // Detect platform and environment
                string platform = GetPlatform();
                string environment = GetEnvironment();
                
                GameLogger.Log($"Firebase provider initialized for platform: {platform}, environment: {environment}");
                
                _isInitialized = true;
                return true;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to initialize Firebase provider: {ex.Message}");
                _isInitialized = false;
                return false;
            }
        }
        
        public async UniTask<T> FetchConfig<T>(string key) where T : IConfigModel
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Firebase provider not initialized");
            }
            
            try
            {
                GameLogger.Log($"Fetching config '{key}' from Firebase...");
                
                // Fetch from Firebase Remote Config
                var remoteConfig = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance;
                var jsonString = remoteConfig.GetValue(key).StringValue;
                
                if (string.IsNullOrEmpty(jsonString))
                {
                    GameLogger.LogWarning($"Firebase returned empty value for key: {key}");
                    await UniTask.Yield();
                    return default(T);
                }
                
                // Deserialize JSON to config model
                var config = JsonConvert.DeserializeObject<T>(jsonString);
                
                await UniTask.Yield();
                return config;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to fetch config '{key}' from Firebase: {ex.Message}");
                throw;
            }
        }
        
        public async UniTask<Dictionary<string, IConfigModel>> FetchAllConfigs()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Firebase provider not initialized");
            }
            
            try
            {
                GameLogger.Log("Fetching all configs from Firebase...");
                
                var configs = new Dictionary<string, IConfigModel>();
                
                // Fetch all config domains
                var gameSettings = await FetchConfig<GameSettingsConfig>("GameSettings");
                if (gameSettings != null) configs["GameSettings"] = gameSettings;
                
                var shopConfig = await FetchConfig<ShopConfig>("ShopConfig");
                if (shopConfig != null) configs["ShopConfig"] = shopConfig;
                
                var characterConfig = await FetchConfig<CharacterConfig>("CharacterConfig");
                if (characterConfig != null) configs["CharacterConfig"] = characterConfig;
                
                var mapConfig = await FetchConfig<MapConfig>("MapConfig");
                if (mapConfig != null) configs["MapConfig"] = mapConfig;
                
                var skinsConfig = await FetchConfig<SkinsConfig>("SkinsConfig");
                if (skinsConfig != null) configs["SkinsConfig"] = skinsConfig;
                
                var maintenanceConfig = await FetchConfig<MaintenanceConfig>("MaintenanceConfig");
                if (maintenanceConfig != null) configs["MaintenanceConfig"] = maintenanceConfig;
                
                var forceUpdateConfig = await FetchConfig<ForceUpdateConfig>("ForceUpdateConfig");
                if (forceUpdateConfig != null) configs["ForceUpdateConfig"] = forceUpdateConfig;
                
                GameLogger.Log($"Fetched {configs.Count} configs from Firebase");
                return configs;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to fetch all configs from Firebase: {ex.Message}");
                throw;
            }
        }
        
        private string GetPlatform()
        {
#if UNITY_IOS
            return "iOS";
#elif UNITY_ANDROID
            return "Android";
#elif UNITY_STANDALONE
            return "PC";
#else
            return "Unknown";
#endif
        }
        
        private string GetEnvironment()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            return "Development";
#elif STAGING
            return "Staging";
#else
            return "Production";
#endif
        }
    }
}
```

## Required Using Statements

Make sure these are at the top of the file:
```csharp
using Firebase;
using Firebase.RemoteConfig;
using Firebase.Extensions;
```

## Testing

After integration, test with:
```csharp
var remoteConfig = GameBootstrap.Services.RemoteConfigService;
await remoteConfig.Initialize();

if (remoteConfig.TryGetConfig<GameSettingsConfig>(out var settings))
{
    Debug.Log($"Max Players: {settings.MaxPlayers}");
    Debug.Log($"Ranked Mode: {settings.EnableRankedMode}");
}
```

## Troubleshooting

**Compilation Errors**
- Import Firebase Unity SDK packages
- Add Firebase namespace using statements

**Runtime Errors**
- Check google-services.json is in Assets/
- Check GoogleService-Info.plist is in Assets/
- Verify internet connection
- Check Firebase Console has parameters configured

**Empty Configs**
- Verify parameter keys match exactly (case-sensitive)
- Ensure "Publish changes" was clicked in Firebase Console
- Check JSON is valid

Done! 🚀
