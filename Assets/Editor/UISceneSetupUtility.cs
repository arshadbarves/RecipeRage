using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RecipeRage.Editor
{
    /// <summary>
    /// Utility for setting up UI scenes
    /// </summary>
    public class UISceneSetupUtility : EditorWindow
    {
        [MenuItem("RecipeRage/Setup UI Scene")]
        public static void SetupUIScene()
        {
            // Create a new scene
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Add UI manager to the scene
            AddUIManagerToScene();
            
            // Add a camera to the scene
            AddCameraToScene();
            
            // Add an event system to the scene
            AddEventSystemToScene();
            
            // Save the scene
            string scenePath = "Assets/Scenes/UI.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            
            Debug.Log($"[UISceneSetupUtility] UI scene setup complete: {scenePath}");
        }
        
        /// <summary>
        /// Add UI manager to the scene
        /// </summary>
        private static void AddUIManagerToScene()
        {
            // Load the UI manager prefab
            GameObject uiManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/UIManager.prefab");
            
            if (uiManagerPrefab != null)
            {
                // Instantiate the prefab
                GameObject uiManager = PrefabUtility.InstantiatePrefab(uiManagerPrefab) as GameObject;
                
                if (uiManager != null)
                {
                    // Set position
                    uiManager.transform.position = Vector3.zero;
                    
                    Debug.Log("[UISceneSetupUtility] Added UI manager to scene");
                }
                else
                {
                    Debug.LogError("[UISceneSetupUtility] Failed to instantiate UI manager prefab");
                }
            }
            else
            {
                Debug.LogError("[UISceneSetupUtility] Failed to load UI manager prefab. Make sure to run 'Create UI Prefabs' first.");
            }
        }
        
        /// <summary>
        /// Add a camera to the scene
        /// </summary>
        private static void AddCameraToScene()
        {
            // Create a camera game object
            GameObject cameraObject = new GameObject("Main Camera");
            
            // Add camera component
            Camera camera = cameraObject.AddComponent<Camera>();
            
            // Set camera properties
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.2f);
            camera.orthographic = true;
            camera.orthographicSize = 5;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;
            camera.depth = -1;
            
            // Set position
            cameraObject.transform.position = new Vector3(0, 0, -10);
            
            // Add tag
            cameraObject.tag = "MainCamera";
            
            Debug.Log("[UISceneSetupUtility] Added camera to scene");
        }
        
        /// <summary>
        /// Add an event system to the scene
        /// </summary>
        private static void AddEventSystemToScene()
        {
            // Create an event system game object
            GameObject eventSystemObject = new GameObject("EventSystem");
            
            // Add event system component
            eventSystemObject.AddComponent<UnityEngine.EventSystems.EventSystem>();
            
            // Add input module component
            eventSystemObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            
            Debug.Log("[UISceneSetupUtility] Added event system to scene");
        }
    }
}
