using UnityEditor;
using UnityEngine;
using RecipeRage.Editor.Prefabs;

namespace RecipeRage.Editor
{
    /// <summary>
    /// Main setup manager for RecipeRage game.
    /// Coordinates the generation of prefabs, scriptable objects, and scene setup.
    /// </summary>
    public class RecipeRageSetupManager : EditorWindow
    {
        // No generator scripts or setup options needed anymore
        // All functionality is delegated to PrefabCreationManager

        [MenuItem("RecipeRage/Setup Manager")]
        public static void ShowWindow()
        {
            GetWindow<RecipeRageSetupManager>("RecipeRage Setup");
        }

        // No initialization needed

        private void OnGUI()
        {
            GUILayout.Label("RecipeRage Setup Manager", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("This tool will generate all necessary assets for the RecipeRage game using the centralized PrefabCreationManager.",
                                   MessageType.Info);

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("All assets will be created with a single click, including:\n" +
                                   "• Manager prefabs (GameBootstrap, NetworkManager, etc.)\n" +
                                   "• UI prefabs (SplashScreenManager, LoadingScreenManager, etc.)\n" +
                                   "• Player prefabs\n" +
                                   "• Station prefabs\n" +
                                   "• Scriptable objects (Character Classes, Game Modes)\n" +
                                   "• Audio assets\n" +
                                   "• Scene setup",
                                   MessageType.Info);

            EditorGUILayout.Space();

            // Generate button with a larger height to make it more prominent
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Generate All Game Assets", GUILayout.Height(40), GUILayout.Width(250)))
            {
                GenerateAll();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("Note: Individual generation options have been removed to avoid confusion. " +
                                   "All assets are now created through the centralized PrefabCreationManager.",
                                   MessageType.Warning);
        }



        /// <summary>
        /// Generate all game assets using the centralized PrefabCreationManager.
        /// </summary>
        private void GenerateAll()
        {
            // Use the centralized PrefabCreationManager for one-click setup
            PrefabCreationManager.CreateAllGameAssets();

            Debug.Log("RecipeRage setup completed successfully!");
        }
    }
}
