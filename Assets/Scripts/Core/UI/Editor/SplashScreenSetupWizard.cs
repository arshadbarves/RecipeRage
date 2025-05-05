using UnityEngine;
using UnityEditor;
using System.IO;
using Core.UI.SplashScreen;
using UnityEditor.SceneManagement;
using UnityEngine.UIElements;

namespace Core.UI.Editor
{
    /// <summary>
    /// Editor wizard for setting up the splash screen system.
    /// </summary>
    public class SplashScreenSetupWizard : EditorWindow
    {
        private const string UXML_DIRECTORY = "Assets/UI/UXML";
        private const string USS_DIRECTORY = "Assets/UI/USS";
        private const string TEXTURES_DIRECTORY = "Assets/Textures/UI";
        private const string PREFABS_DIRECTORY = "Assets/Prefabs/UI";

        private Texture2D _companyLogo;
        private Texture2D _gameLogo;
        private Texture2D _loadingBackground;
        private string _companyName = "YOUR COMPANY";
        private string _gameTagline = "COOK. COMPETE. CONQUER.";
        private string[] _loadingTips = new string[]
        {
            "Combine ingredients to create special recipes!",
            "Work together with your team to complete orders faster!",
            "Different character classes have unique abilities!",
            "Don't let food burn or you'll lose points!",
            "Complete orders quickly to earn bonus points!"
        };

        [MenuItem("RecipeRage/UI/Splash Screen Setup Wizard")]
        public static void ShowWindow()
        {
            var window = GetWindow<SplashScreenSetupWizard>("Splash Screen Setup");
            window.minSize = new Vector2(400, 500);
        }

        /// <summary>
        /// Creates a SplashScreenManager prefab.
        /// This is a static method that can be called from other scripts.
        /// </summary>
        public static void CreateSplashScreenManagerPrefab()
        {
            SplashScreenManagerPrefabCreator.CreateSplashScreenManagerPrefab();
        }

        private void OnGUI()
        {
            GUILayout.Label("Splash Screen Setup Wizard", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            GUILayout.Label("Textures", EditorStyles.boldLabel);
            _companyLogo = (Texture2D)EditorGUILayout.ObjectField("Company Logo", _companyLogo, typeof(Texture2D), false);
            _gameLogo = (Texture2D)EditorGUILayout.ObjectField("Game Logo", _gameLogo, typeof(Texture2D), false);
            _loadingBackground = (Texture2D)EditorGUILayout.ObjectField("Loading Background", _loadingBackground, typeof(Texture2D), false);

            EditorGUILayout.Space();
            GUILayout.Label("Text", EditorStyles.boldLabel);
            _companyName = EditorGUILayout.TextField("Company Name", _companyName);
            _gameTagline = EditorGUILayout.TextField("Game Tagline", _gameTagline);

            EditorGUILayout.Space();
            GUILayout.Label("Loading Tips", EditorStyles.boldLabel);

            // Display loading tips
            for (int i = 0; i < _loadingTips.Length; i++)
            {
                _loadingTips[i] = EditorGUILayout.TextField($"Tip {i + 1}", _loadingTips[i]);
            }

            EditorGUILayout.Space();

            // Setup buttons
            if (GUILayout.Button("Create Directories"))
            {
                CreateDirectories();
            }

            if (GUILayout.Button("Save Textures"))
            {
                SaveTextures();
            }

            if (GUILayout.Button("Create SplashScreenManager Prefab"))
            {
                CreateSplashScreenManagerPrefab();
            }

            if (GUILayout.Button("Create Test Scene"))
            {
                CreateTestScene();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Setup Everything"))
            {
                CreateDirectories();
                SaveTextures();
                CreateSplashScreenManagerPrefab();
                CreateTestScene();

                EditorUtility.DisplayDialog("Setup Complete",
                    "Splash screen system setup is complete! You can now use the SplashScreenManager in your game.",
                    "OK");
            }
        }

        /// <summary>
        /// Create all necessary directories.
        /// </summary>
        private void CreateDirectories()
        {
            CreateDirectoryIfNotExists(UXML_DIRECTORY);
            CreateDirectoryIfNotExists(USS_DIRECTORY);
            CreateDirectoryIfNotExists(TEXTURES_DIRECTORY);
            CreateDirectoryIfNotExists(PREFABS_DIRECTORY);

            Debug.Log("[SplashScreenSetupWizard] Created all directories.");
        }

        /// <summary>
        /// Save textures to the appropriate directory.
        /// </summary>
        private void SaveTextures()
        {
            if (_companyLogo != null)
            {
                string path = $"{TEXTURES_DIRECTORY}/company_logo.png";
                SaveTextureAsPNG(_companyLogo, path);
            }

            if (_gameLogo != null)
            {
                string path = $"{TEXTURES_DIRECTORY}/recipe_rage_logo.png";
                SaveTextureAsPNG(_gameLogo, path);
            }

            if (_loadingBackground != null)
            {
                string path = $"{TEXTURES_DIRECTORY}/loading_background.png";
                SaveTextureAsPNG(_loadingBackground, path);
            }

            Debug.Log("[SplashScreenSetupWizard] Saved textures.");
        }

        /// <summary>
        /// Create a test scene for the splash screen system.
        /// </summary>
        private void CreateTestScene()
        {
            // Create a new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Create a SplashScreenManager GameObject
            var managerObj = new GameObject("SplashScreenManager");
            var splashManager = managerObj.AddComponent<SplashScreenManager>();

            // Create UI Documents for splash screens
            var companySplashObj = new GameObject("CompanySplashScreen");
            companySplashObj.transform.SetParent(managerObj.transform);
            var companySplashDocument = companySplashObj.AddComponent<UIDocument>();

            var gameLogoSplashObj = new GameObject("GameLogoSplashScreen");
            gameLogoSplashObj.transform.SetParent(managerObj.transform);
            var gameLogoSplashDocument = gameLogoSplashObj.AddComponent<UIDocument>();

            // Note: Loading screen is now handled by LoadingScreenManager

            // Try to find the UXML assets
            var companySplashUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{UXML_DIRECTORY}/CompanySplashScreen.uxml");
            if (companySplashUXML != null)
            {
                companySplashDocument.visualTreeAsset = companySplashUXML;
            }

            var gameLogoSplashUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{UXML_DIRECTORY}/GameLogoSplashScreen.uxml");
            if (gameLogoSplashUXML != null)
            {
                gameLogoSplashDocument.visualTreeAsset = gameLogoSplashUXML;
            }

            // Set references in SplashScreenManager
            splashManager.SetCompanySplashDocument(companySplashDocument);
            splashManager.SetGameLogoSplashDocument(gameLogoSplashDocument);

            // Note: Loading tips are now handled by LoadingScreenManager

            // No need to set references as we removed the tester

            // Add a camera
            var cameraObj = new GameObject("Main Camera");
            var camera = cameraObj.AddComponent<Camera>();
            cameraObj.AddComponent<AudioListener>();
            cameraObj.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;

            // Save the scene
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/SplashScreenTest.unity");

            Debug.Log("[SplashScreenSetupWizard] Created test scene.");
        }

        /// <summary>
        /// Create directory if it doesn't exist.
        /// </summary>
        /// <param name="path">Directory path</param>
        private void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
                Debug.Log($"[SplashScreenSetupWizard] Created directory: {path}");
            }
        }

        /// <summary>
        /// Save a texture as PNG.
        /// </summary>
        /// <param name="texture">The texture to save</param>
        /// <param name="path">The path to save to</param>
        private void SaveTextureAsPNG(Texture2D texture, string path)
        {
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
            Debug.Log($"[SplashScreenSetupWizard] Saved texture to {path}");
        }
    }
}
