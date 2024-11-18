using System;
using UnityEngine;

namespace Core.SceneManagement
{
    /// <summary>
    ///     Defines scene configuration data with network synchronization support
    /// </summary>
    [CreateAssetMenu(fileName = "SceneData", menuName = "RecipeRage/Scene/SceneData")]
    public class SceneData : ScriptableObject
    {
        public string sceneName;
        public bool requiresNetworkSync;
        public LoadPriority loadPriority = LoadPriority.Normal;
        public SceneType sceneType;
        public string[] dependencies = Array.Empty<string>();

        // Network-specific settings
        public float networkTimeout = 10f;
        public bool allowLateJoin = true;
    }

    public enum LoadPriority
    {
        High,
        Normal,
        Low
    }

    public enum SceneType
    {
        Persistent, // Scenes that stay loaded (like managers)
        Gameplay, // Main gameplay scenes
        UI, // UI overlay scenes
        Loading // Loading screens
    }
}