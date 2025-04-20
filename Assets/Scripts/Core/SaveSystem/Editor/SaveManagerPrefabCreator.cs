using UnityEngine;
using UnityEditor;
using System.IO;

namespace Core.SaveSystem.Editor
{
    /// <summary>
    /// Editor utility for creating the SaveManager prefab.
    /// </summary>
    public static class SaveManagerPrefabCreator
    {
        private const string PrefabPath = "Assets/Prefabs/SaveSystem/SaveManager.prefab";
        
        [MenuItem("RecipeRage/SaveSystem/Create SaveManager Prefab")]
        public static void CreateSaveManagerPrefab()
        {
            // Create directory if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            
            // Create SaveManager GameObject
            GameObject saveManagerObject = new GameObject("SaveManager");
            
            // Add SaveManager component
            saveManagerObject.AddComponent<SaveManager>();
            
            // Create prefab
            bool success = false;
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            
            if (existingPrefab != null)
            {
                // Replace existing prefab
                success = PrefabUtility.SaveAsPrefabAsset(saveManagerObject, PrefabPath);
            }
            else
            {
                // Create new prefab
                success = PrefabUtility.SaveAsPrefabAsset(saveManagerObject, PrefabPath);
            }
            
            // Clean up
            Object.DestroyImmediate(saveManagerObject);
            
            if (success)
            {
                Debug.Log($"SaveManager prefab created at {PrefabPath}");
                
                // Select the prefab in the Project window
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            }
            else
            {
                Debug.LogError("Failed to create SaveManager prefab");
            }
        }
    }
}
