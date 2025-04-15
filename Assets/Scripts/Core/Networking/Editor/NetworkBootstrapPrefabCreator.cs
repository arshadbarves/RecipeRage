using UnityEditor;
using UnityEngine;

namespace RecipeRage.Core.Networking.Editor
{
    /// <summary>
    /// Editor utility for creating the NetworkBootstrap prefab.
    /// </summary>
    public static class NetworkBootstrapPrefabCreator
    {
        /// <summary>
        /// Create the NetworkBootstrap prefab.
        /// </summary>
        [MenuItem("RecipeRage/Networking/Create NetworkBootstrap Prefab")]
        public static void CreateNetworkBootstrapPrefab()
        {
            // Create a new game object
            GameObject networkBootstrapObject = new GameObject("NetworkBootstrap");
            
            // Add the NetworkBootstrap component
            networkBootstrapObject.AddComponent<NetworkBootstrap>();
            
            // Create the prefab
            string prefabPath = "Assets/Prefabs/Networking/NetworkBootstrap.prefab";
            
            // Ensure the directory exists
            string directory = System.IO.Path.GetDirectoryName(prefabPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            
            // Create the prefab
            #if UNITY_2018_3_OR_NEWER
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(networkBootstrapObject, prefabPath);
            #else
            GameObject prefab = PrefabUtility.CreatePrefab(prefabPath, networkBootstrapObject);
            #endif
            
            // Destroy the temporary object
            Object.DestroyImmediate(networkBootstrapObject);
            
            // Select the prefab
            Selection.activeObject = prefab;
            
            Debug.Log($"[NetworkBootstrapPrefabCreator] Created NetworkBootstrap prefab at {prefabPath}");
        }
    }
}
