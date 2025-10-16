using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UI;

namespace Editor
{
    /// <summary>
    /// Editor utility to quickly setup the Debug Console in a scene
    /// </summary>
    public static class DebugConsoleSetup
    {
        [MenuItem("Tools/Debug Console/Add to Scene")]
        public static void AddDebugConsoleToScene()
        {
            // Check if already exists
            var existing = Object.FindObjectOfType<DebugConsoleUI>();
            if (existing != null)
            {
                Debug.LogWarning("Debug Console already exists in the scene!");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            // Create GameObject
            var consoleObj = new GameObject("DebugConsole");
            
            // Add UIDocument
            var uiDocument = consoleObj.AddComponent<UIDocument>();
            uiDocument.panelSettings = GetOrCreatePanelSettings();
            
            // Add DebugConsoleUI
            consoleObj.AddComponent<DebugConsoleUI>();
            
            // Mark as DontDestroyOnLoad
            Object.DontDestroyOnLoad(consoleObj);
            
            // Select it
            Selection.activeGameObject = consoleObj;
            
            Debug.Log("✅ Debug Console added to scene!\n" +
                     "Desktop: Press ` (backtick) to toggle\n" +
                     "Mobile: Hold 3 fingers for 1 second to toggle\n" +
                     "Note: Only available in Development Builds and Editor");
        }

        [MenuItem("Tools/Debug Console/Create Panel Settings")]
        public static PanelSettings CreatePanelSettings()
        {
            var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            panelSettings.name = "DebugConsolePanelSettings";
            
            // Configure panel settings
            panelSettings.scaleMode = PanelScaleMode.ConstantPixelSize;
            panelSettings.scale = 1.0f;
            panelSettings.fallbackDpi = 96;
            panelSettings.referenceDpi = 96;
            panelSettings.referenceResolution = new Vector2Int(1920, 1080);
            panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            panelSettings.match = 0.5f;
            panelSettings.sortingOrder = 1000; // High sorting order to appear on top
            
            // Save to Resources folder
            string path = "Assets/Resources/UI/DebugConsolePanelSettings.asset";
            AssetDatabase.CreateAsset(panelSettings, path);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"Panel Settings created at: {path}");
            return panelSettings;
        }

        private static PanelSettings GetOrCreatePanelSettings()
        {
            // Try to find existing panel settings
            var existing = AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/Resources/UI/DebugConsolePanelSettings.asset");
            if (existing != null)
                return existing;

            // Try to find any panel settings
            var guids = AssetDatabase.FindAssets("t:PanelSettings");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            }

            // Create new one
            return CreatePanelSettings();
        }

        [MenuItem("Tools/Debug Console/Test Logging")]
        public static void TestLogging()
        {
            Debug.Log("Test Info Log");
            Debug.LogWarning("Test Warning Log");
            Debug.LogError("Test Error Log");
            
            Debug.Log("✅ Test logs sent! Open the debug console to view them.");
        }
    }
}
