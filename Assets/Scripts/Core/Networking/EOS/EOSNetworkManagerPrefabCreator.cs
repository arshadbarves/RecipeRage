using UnityEngine;
using UnityEditor;

namespace RecipeRage.Core.Networking.EOS
{
#if UNITY_EDITOR
    /// <summary>
    /// Editor script to create the EOSNetworkManager prefab.
    /// </summary>
    public static class EOSNetworkManagerPrefabCreator
    {
        [MenuItem("RecipeRage/Create/EOS Network Manager")]
        public static void CreateEOSNetworkManagerPrefab()
        {
            // Create a new game object
            GameObject go = new GameObject("EOSNetworkManager");

            // Add the EOSNetworkManager component
            go.AddComponent<EOSNetworkManager>();

            // Create the prefab
            string prefabPath = "Assets/Prefabs/Networking/EOSNetworkManager.prefab";

            // Ensure the directory exists
            string directory = System.IO.Path.GetDirectoryName(prefabPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            // Create the prefab
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);

            // Destroy the temporary game object
            Object.DestroyImmediate(go);

            Debug.Log($"EOSNetworkManager prefab created at {prefabPath}");

            // Select the prefab in the Project window
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }
    }
#endif
}
