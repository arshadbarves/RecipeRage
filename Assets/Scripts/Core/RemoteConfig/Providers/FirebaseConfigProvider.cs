using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Core.Logging;
using Core.RemoteConfig.Models;
using PlayEveryWare.EpicOnlineServices.Utility;

namespace Core.RemoteConfig.Providers
{
    /// <summary>
    /// Configuration provider that fetches data from Firebase Remote Config
    /// </summary>
    public class FirebaseConfigProvider : IConfigProvider
    {
        public string ProviderName => "Firebase";

        private bool _isInitialized;

        // Firebase Remote Config would be initialized here
        // For now, this is a placeholder for Firebase SDK integration

        public FirebaseConfigProvider()
        {
            _isInitialized = false;
        }

        public bool IsAvailable()
        {
            // Check if Firebase is available and initialized
            // This would check Firebase.RemoteConfig availability
            return _isInitialized;
        }

        public async UniTask<bool> Initialize()
        {
            try
            {
                GameLogger.Log("Initializing Firebase Remote Config provider...");

                // Detect platform and environment
                string platform = GetPlatform();
                string environment = GetEnvironment();

                var remoteConfig = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance;

                // Firebase will automatically match conditions based on device.os
                // No need to set anything - it's automatic!

                GameLogger.Log($"Firebase will use device.os for condition matching");

                // Set default config values
                var defaults = new Dictionary<string, object>
                {
                    // Add your default config values here if needed
                };

                await remoteConfig.SetDefaultsAsync(defaults);

                // Fetch and activate configs
                await remoteConfig.FetchAsync(TimeSpan.Zero);
                await remoteConfig.ActivateAsync();

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

                var jsonString = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;

                return JsonUtility.FromJson<T>(jsonString);

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
            return "ios";
#elif UNITY_ANDROID
            return "android";
#elif UNITY_STANDALONE
            return "pc";
#else
            return "unknown";
#endif
        }

        private string GetEnvironment()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            return "development";
#elif STAGING
            return "staging";
#else
            return "production";
#endif
        }
    }
}
