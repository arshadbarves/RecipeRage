using UnityEngine;
using UnityEditor;

namespace RecipeRage.Editor
{
    /// <summary>
    /// DEPRECATED: This class is kept for backward compatibility.
    /// Please use PrefabCreationManager instead for all prefab creation.
    /// </summary>
    [System.Obsolete("Use PrefabCreationManager instead")]
    public class GameBootstrapPrefabCreator
    {
        [MenuItem("RecipeRage/Create/GameBootstrap Prefab (Legacy)", false, 100)]
        public static void CreateGameBootstrapPrefab()
        {
            Debug.LogWarning("GameBootstrapPrefabCreator is deprecated. Using PrefabCreationManager instead.");
            PrefabCreationManager.CreateGameBootstrapPrefab();
        }

        [MenuItem("RecipeRage/Create/All Game Assets (Legacy)", false, 100)]
        public static void CreateAllGameAssets()
        {
            Debug.LogWarning("GameBootstrapPrefabCreator is deprecated. Using PrefabCreationManager instead.");
            PrefabCreationManager.CreateAllGameAssets();
        }

        [MenuItem("RecipeRage/Audio/Create Audio Manager Prefab (Legacy)", false, 100)]
        public static void CreateAudioManagerPrefab()
        {
            Debug.LogWarning("GameBootstrapPrefabCreator is deprecated. Using PrefabCreationManager instead.");
            PrefabCreationManager.CreateAudioManagerPrefab();
        }

        [MenuItem("RecipeRage/Save System/Create Save Manager Prefab (Legacy)", false, 100)]
        public static void CreateSaveManagerPrefab()
        {
            Debug.LogWarning("GameBootstrapPrefabCreator is deprecated. Using PrefabCreationManager instead.");
            PrefabCreationManager.CreateSaveManagerPrefab();
        }

        [MenuItem("RecipeRage/UI/Create Splash Screen Manager Prefab (Legacy)", false, 200)]
        public static void CreateSplashScreenManagerPrefab()
        {
            Debug.LogWarning("GameBootstrapPrefabCreator is deprecated. Using PrefabCreationManager instead.");
            PrefabCreationManager.CreateSplashScreenManagerPrefab();
        }
    }
}