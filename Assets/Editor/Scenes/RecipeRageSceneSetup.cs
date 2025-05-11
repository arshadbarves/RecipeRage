using UnityEditor;
using UnityEngine;
using RecipeRage.Editor.Prefabs;

namespace RecipeRage.Editor
{
    /// <summary>
    /// Provides menu items for setting up all RecipeRage scenes.
    /// </summary>
    public static class RecipeRageSceneSetup
    {
        [MenuItem("RecipeRage/Setup/Setup All Scenes")]
        public static void SetupAllScenes()
        {
            Debug.Log("Starting complete RecipeRage scene setup...");

            try
            {
                // Use the centralized PrefabCreationManager to set up all scenes
                PrefabCreationManager.SetupAllScenes();

                Debug.Log("All scenes have been set up successfully!");

                // Open the startup scene
                EditorUtility.DisplayDialog("Setup Complete",
                    "All RecipeRage scenes have been set up successfully!\n\n" +
                    "The following scenes were created:\n" +
                    "- Startup.unity (with splash and loading screens)\n" +
                    "- MainMenu.unity\n" +
                    "- Game.unity\n\n" +
                    "The Startup scene will now be opened.", "OK");

                UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Startup.unity");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error setting up scenes: {ex.Message}\n{ex.StackTrace}");
                EditorUtility.DisplayDialog("Setup Error",
                    $"An error occurred while setting up the scenes:\n\n{ex.Message}", "OK");
            }
        }

        [MenuItem("RecipeRage/Setup/Add Scenes to Build Settings")]
        public static void AddScenesToBuildSettings()
        {
            Debug.Log("Adding scenes to build settings...");

            try
            {
                // Define the scenes to add in the correct order
                string[] scenePaths = new string[]
                {
                    "Assets/Scenes/Startup.unity",
                    "Assets/Scenes/MainMenu.unity",
                    "Assets/Scenes/Game.unity"
                };

                // Create new scene list
                var sceneList = new System.Collections.Generic.List<EditorBuildSettingsScene>();

                // Add each scene
                foreach (string scenePath in scenePaths)
                {
                    if (System.IO.File.Exists(scenePath))
                    {
                        sceneList.Add(new EditorBuildSettingsScene(scenePath, true));
                        Debug.Log($"Added scene to build settings: {scenePath}");
                    }
                    else
                    {
                        Debug.LogWarning($"Scene not found: {scenePath}");
                    }
                }

                // Set the build settings
                EditorBuildSettings.scenes = sceneList.ToArray();

                Debug.Log("Scenes added to build settings successfully!");
                EditorUtility.DisplayDialog("Build Settings Updated",
                    "The following scenes have been added to the build settings in order:\n\n" +
                    "1. Startup.unity\n" +
                    "2. MainMenu.unity\n" +
                    "3. Game.unity", "OK");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error adding scenes to build settings: {ex.Message}\n{ex.StackTrace}");
                EditorUtility.DisplayDialog("Build Settings Error",
                    $"An error occurred while adding scenes to build settings:\n\n{ex.Message}", "OK");
            }
        }
    }
}
