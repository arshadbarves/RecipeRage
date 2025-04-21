using UnityEngine;
using UnityEditor;
using System.IO;

namespace Core.SaveSystem.Editor
{
    /// <summary>
    /// Editor utility for creating the save manager prefab.
    /// </summary>
    public static class SaveManagerPrefabCreator
    {
        private const string PREFAB_PATH = "Assets/Prefabs/Managers/SaveManager.prefab";
        private const string PREFAB_DIRECTORY = "Assets/Prefabs/Managers";
        
        [MenuItem("RecipeRage/Save System/Create Save Manager Prefab")]
        public static void CreateSaveManagerPrefab()
        {
            // Create directory if it doesn't exist
            if (!Directory.Exists(PREFAB_DIRECTORY))
            {
                Directory.CreateDirectory(PREFAB_DIRECTORY);
                AssetDatabase.Refresh();
            }
            
            // Check if prefab already exists
            if (File.Exists(PREFAB_PATH))
            {
                Debug.Log($"[SaveManagerPrefabCreator] Save manager prefab already exists at {PREFAB_PATH}");
                
                // Select the existing prefab
                GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
                Selection.activeObject = existingPrefab;
                
                return;
            }
            
            // Create the save manager GameObject
            GameObject saveManagerObj = new GameObject("SaveManager");
            
            // Add SaveManager component
            SaveManager saveManager = saveManagerObj.AddComponent<SaveManager>();
            
            // Create the prefab
            PrefabUtility.SaveAsPrefabAsset(saveManagerObj, PREFAB_PATH);
            
            // Destroy the temporary GameObject
            Object.DestroyImmediate(saveManagerObj);
            
            Debug.Log($"[SaveManagerPrefabCreator] Created save manager prefab at {PREFAB_PATH}");
            
            // Select the prefab in the Project window
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            Selection.activeObject = prefab;
        }
    }
}
