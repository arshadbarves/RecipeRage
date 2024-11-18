using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VContainer;

namespace Core.Data.Config
{
    public class ConfigManager
    {
        private readonly Dictionary<Type, ScriptableObject> _configs;

        [Inject]
        public ConfigManager()
        {
            _configs = new Dictionary<Type, ScriptableObject>();
            LoadConfigs();
        }

        private void LoadConfigs()
        {
            // Load all configs from Resources folder
            GameConfig gameConfig = Resources.Load<GameConfig>("Configs/GameConfig");
            NetworkConfig networkConfig = Resources.Load<NetworkConfig>("Configs/NetworkConfig");

            RegisterConfig(gameConfig);
            RegisterConfig(networkConfig);
        }

        public void RegisterConfig<T>(T config) where T : ScriptableObject
        {
            if (config != null)
            {
                _configs[typeof(T)] = config;
            }
            else
            {
                Debug.LogError($"Failed to register config of type {typeof(T)}");
            }
        }

        public T GetConfig<T>() where T : ScriptableObject
        {
            if (_configs.TryGetValue(typeof(T), out ScriptableObject config))
            {
                return config as T;
            }
            return null;
        }

        public void ReloadConfigs()
        {
            _configs.Clear();
            LoadConfigs();
        }

    #if UNITY_EDITOR
        public void ValidateAllConfigs()
        {
            foreach (ScriptableObject config in _configs.Values)
            {
                if (config != null)
                {
                    EditorUtility.SetDirty(config);
                }
            }
        }
    #endif
    }
}