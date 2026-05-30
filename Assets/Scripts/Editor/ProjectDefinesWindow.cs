using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace RecipeRage.Editor
{
    /// <summary>
    /// Editor window for managing scripting define symbols across platforms.
    /// Enables/disabling defines updates PlayerSettings automatically.
    /// </summary>
    public class ProjectDefinesWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private BuildTargetGroup selectedPlatform = BuildTargetGroup.Standalone;
        private string searchFilter = "";
        private bool showDescriptions = true;
        private bool showOnlyActive = false;

        private enum DefinePreset
        {
            Debug,
            Release,
            Development,
            Production,
            Custom
        }
        private DefinePreset currentPreset = DefinePreset.Custom;

        private static readonly Dictionary<string, DefineInfo> defineRegistry = new Dictionary<string, DefineInfo>
        {
            // Core
            { "DOTWEEN", new DefineInfo("DOTween Animation Library", "Core", "Tweening and animation system") },
            { "UNITY_POST_PROCESSING_STACK_V2", new DefineInfo("Post Processing Stack V2", "Core", "Unity post-processing effects") },

            // Analytics
            { "SENTIS_ANALYTICS_ENABLED", new DefineInfo("Sentis Analytics", "Analytics", "Enable Unity Sentis ML analytics") },

            // Editor Only
            { "APP_UI_EDITOR_ONLY", new DefineInfo("UI Editor Only", "Editor", "Editor-only UI tools and debug features") },

            // Development
            { "ENABLE_DEBUG_LOGS", new DefineInfo("Debug Logging", "Development", "Enable detailed debug logging") },
            { "ENABLE_CHEAT_MODE", new DefineInfo("Cheat Mode", "Development", "Enable cheat/debug commands") },
            { "ENABLE_GOD_MODE", new DefineInfo("God Mode", "Development", "Enable god mode for testing") },
            { "ENABLE_FPS_COUNTER", new DefineInfo("FPS Counter", "Development", "Show FPS counter overlay") },
            { "ENABLE_NETWORK_DEBUG", new DefineInfo("Network Debug", "Development", "Enable network debugging tools") },

            // Features
            { "ENABLE_MULTIPLAYER", new DefineInfo("Multiplayer", "Features", "Enable multiplayer/networking features") },
            { "ENABLE_EOS", new DefineInfo("EOS Integration", "Features", "Enable Epic Online Services integration") },
            { "ENABLE_FIREBASE", new DefineInfo("Firebase", "Features", "Enable Firebase services") },
            { "ENABLE_ANALYTICS", new DefineInfo("Analytics", "Features", "Enable analytics tracking") },
            { "ENABLE_ADS", new DefineInfo("Ads", "Features", "Enable advertisements") },
            { "ENABLE_IAP", new DefineInfo("In-App Purchases", "Features", "Enable in-app purchase system") },
            { "ENABLE_NOTIFICATIONS", new DefineInfo("Notifications", "Features", "Enable push notifications") },

            // Platform Specific
            { "UNITY_IOS", new DefineInfo("iOS Platform", "Platform", "iOS-specific code paths") },
            { "UNITY_ANDROID", new DefineInfo("Android Platform", "Platform", "Android-specific code paths") },
            { "UNITY_WEBGL", new DefineInfo("WebGL Platform", "Platform", "WebGL-specific code paths") },
            { "UNITY_STANDALONE", new DefineInfo("Standalone Platform", "Platform", "Desktop standalone builds") },

            // Performance
            { "DISABLE_BURST", new DefineInfo("Disable Burst", "Performance", "Disable Burst compiler optimizations") },
            { "ENABLE_IL2CPP", new DefineInfo("IL2CPP Backend", "Performance", "Use IL2CPP scripting backend") },
            { "ENABLE_UWP", new DefineInfo("UWP Support", "Platform", "Universal Windows Platform support") },

            // Testing
            { "ENABLE_UNIT_TESTS", new DefineInfo("Unit Tests", "Testing", "Enable unit test code paths") },
            { "ENABLE_INTEGRATION_TESTS", new DefineInfo("Integration Tests", "Testing", "Enable integration test code paths") },
            { "ENABLE_PLAYMODE_TESTS", new DefineInfo("PlayMode Tests", "Testing", "Enable PlayMode test code paths") },
        };

        private static readonly Dictionary<DefinePreset, HashSet<string>> presetDefines = new Dictionary<DefinePreset, HashSet<string>>
        {
            { DefinePreset.Debug, new HashSet<string>
            {
                "DOTWEEN", "UNITY_POST_PROCESSING_STACK_V2", "APP_UI_EDITOR_ONLY",
                "ENABLE_DEBUG_LOGS", "ENABLE_CHEAT_MODE", "ENABLE_GOD_MODE",
                "ENABLE_FPS_COUNTER", "ENABLE_NETWORK_DEBUG"
            }},
            { DefinePreset.Release, new HashSet<string>
            {
                "DOTWEEN", "UNITY_POST_PROCESSING_STACK_V2",
                "ENABLE_MULTIPLAYER", "ENABLE_EOS"
            }},
            { DefinePreset.Development, new HashSet<string>
            {
                "DOTWEEN", "UNITY_POST_PROCESSING_STACK_V2", "APP_UI_EDITOR_ONLY",
                "ENABLE_DEBUG_LOGS", "ENABLE_FPS_COUNTER", "ENABLE_MULTIPLAYER",
                "ENABLE_EOS", "ENABLE_FIREBASE"
            }},
            { DefinePreset.Production, new HashSet<string>
            {
                "DOTWEEN", "UNITY_POST_PROCESSING_STACK_V2",
                "ENABLE_MULTIPLAYER", "ENABLE_EOS", "ENABLE_FIREBASE",
                "ENABLE_ANALYTICS", "ENABLE_ADS", "ENABLE_IAP", "ENABLE_NOTIFICATIONS"
            }},
        };

        [MenuItem("RecipeRage/Project Defines")]
        public static void ShowWindow()
        {
            var window = GetWindow<ProjectDefinesWindow>("Project Defines");
            window.minSize = new Vector2(500, 400);
        }

        private void OnGUI()
        {
            DrawHeader();
            EditorGUILayout.Space(5);
            DrawPlatformSelector();
            EditorGUILayout.Space(5);
            DrawPresetSelector();
            EditorGUILayout.Space(5);
            DrawSearchBar();
            EditorGUILayout.Space(5);
            DrawToolbar();
            EditorGUILayout.Space(5);
            DrawDefinesList();
            DrawFooter();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Project Defines Manager", EditorStyles.boldLabel);

            showDescriptions = GUILayout.Toggle(showDescriptions, "Show Descriptions", EditorStyles.toolbarButton);
            showOnlyActive = GUILayout.Toggle(showOnlyActive, "Active Only", EditorStyles.toolbarButton);

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                Repaint();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPlatformSelector()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Platform:", GUILayout.Width(60));
            selectedPlatform = (BuildTargetGroup)EditorGUILayout.EnumPopup(selectedPlatform, GUILayout.Width(150));

            GUILayout.FlexibleSpace();

            string currentDefines = GetCurrentDefines();
            int count = string.IsNullOrEmpty(currentDefines) ? 0 : currentDefines.Split(';').Length;
            GUILayout.Label($"Active Defines: {count}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPresetSelector()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Preset:", GUILayout.Width(60));

            DefinePreset newPreset = (DefinePreset)EditorGUILayout.EnumPopup(currentPreset, GUILayout.Width(150));

            if (newPreset != currentPreset)
            {
                currentPreset = newPreset;
                if (currentPreset != DefinePreset.Custom)
                {
                    ApplyPreset(currentPreset);
                }
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Save as Preset", GUILayout.Width(100)))
            {
                SaveCurrentAsPreset();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSearchBar()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Search:", GUILayout.Width(60));
            searchFilter = EditorGUILayout.TextField(searchFilter);
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                searchFilter = "";
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Enable All Visible", GUILayout.Width(120)))
            {
                EnableAllVisibleDefines(true);
            }

            if (GUILayout.Button("Disable All Visible", GUILayout.Width(120)))
            {
                EnableAllVisibleDefines(false);
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Copy Defines", GUILayout.Width(100)))
            {
                EditorGUIUtility.systemCopyBuffer = GetCurrentDefines();
                Debug.Log("[ProjectDefines] Defines copied to clipboard");
            }

            if (GUILayout.Button("Paste Defines", GUILayout.Width(100)))
            {
                if (!string.IsNullOrEmpty(EditorGUIUtility.systemCopyBuffer))
                {
                    SetCurrentDefines(EditorGUIUtility.systemCopyBuffer);
                    Debug.Log($"[ProjectDefines] Pasted defines: {EditorGUIUtility.systemCopyBuffer}");
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawDefinesList()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            var filteredDefines = GetFilteredDefines();
            var groupedDefines = filteredDefines.GroupBy(kvp => defineRegistry.ContainsKey(kvp.Key) ? defineRegistry[kvp.Key].Category : "Other")
                                               .OrderBy(g => g.Key);

            foreach (var group in groupedDefines)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"── {group.Key} ──", EditorStyles.boldLabel);

                foreach (var kvp in group.OrderBy(kvp => kvp.Key))
                {
                    DrawDefineToggle(kvp.Key, kvp.Value);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawDefineToggle(string defineName, bool isActive)
        {
            string description = "";
            if (defineRegistry.TryGetValue(defineName, out var info))
            {
                description = info.Description;
            }

            EditorGUILayout.BeginHorizontal();

            bool newState = EditorGUILayout.Toggle(isActive, GUILayout.Width(20));

            GUILayout.Label(defineName, EditorStyles.boldLabel, GUILayout.Width(200));

            if (showDescriptions && !string.IsNullOrEmpty(description))
            {
                GUILayout.Label(description, EditorStyles.miniLabel);
            }

            if (!defineRegistry.ContainsKey(defineName))
            {
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    if (EditorUtility.DisplayDialog("Remove Define",
                        $"Remove custom define '{defineName}'?", "Remove", "Cancel"))
                    {
                        RemoveDefine(defineName);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            if (newState != isActive)
            {
                if (newState)
                {
                    AddDefine(defineName);
                }
                else
                {
                    RemoveDefine(defineName);
                }
                currentPreset = DefinePreset.Custom;
            }
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Add Custom Define:", GUILayout.Width(130));
            string newDefine = EditorGUILayout.TextField("", GUILayout.Width(200));

            GUI.enabled = !string.IsNullOrEmpty(newDefine) && !newDefine.Contains(" ") && !newDefine.Contains(";");
            if (GUILayout.Button("Add", GUILayout.Width(60)))
            {
                if (!string.IsNullOrEmpty(newDefine))
                {
                    AddDefine(newDefine.ToUpper().Trim());
                    newDefine = "";
                    GUI.FocusControl(null);
                }
            }
            GUI.enabled = true;

            GUILayout.FlexibleSpace();

            string currentDefines = GetCurrentDefines();
            if (!string.IsNullOrEmpty(currentDefines))
            {
                if (GUILayout.Button("Show Raw Defines", GUILayout.Width(120)))
                {
                    Debug.Log($"[ProjectDefines] Current defines for {selectedPlatform}:\n{currentDefines}");
                    EditorGUIUtility.systemCopyBuffer = currentDefines;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        #region Define Management

        private string GetCurrentDefines()
        {
            return PlayerSettings.GetScriptingDefineSymbols(GetNamedBuildTarget());
        }

        private void SetCurrentDefines(string defines)
        {
            PlayerSettings.SetScriptingDefineSymbols(GetNamedBuildTarget(), defines);
            AssetDatabase.SaveAssets();
            Debug.Log($"[ProjectDefines] Updated defines for {selectedPlatform}");
        }

        private NamedBuildTarget GetNamedBuildTarget()
        {
            return selectedPlatform switch
            {
                BuildTargetGroup.Standalone => NamedBuildTarget.Standalone,
                BuildTargetGroup.Android => NamedBuildTarget.Android,
                BuildTargetGroup.iOS => NamedBuildTarget.iOS,
                BuildTargetGroup.WebGL => NamedBuildTarget.WebGL,
                _ => NamedBuildTarget.Standalone
            };
        }

        private HashSet<string> GetCurrentDefinesSet()
        {
            string current = GetCurrentDefines();
            if (string.IsNullOrEmpty(current))
                return new HashSet<string>();

            return new HashSet<string>(current.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private void AddDefine(string define)
        {
            var defines = GetCurrentDefinesSet();
            defines.Add(define);
            SetCurrentDefines(string.Join(";", defines.OrderBy(d => d)));
        }

        private void RemoveDefine(string define)
        {
            var defines = GetCurrentDefinesSet();
            defines.Remove(define);
            SetCurrentDefines(string.Join(";", defines.OrderBy(d => d)));
        }

        private void ApplyPreset(DefinePreset preset)
        {
            if (presetDefines.TryGetValue(preset, out var presetDefs))
            {
                var allDefines = new HashSet<string>(presetDefs);

                var currentDefines = GetCurrentDefinesSet();
                foreach (var define in currentDefines)
                {
                    if (define.StartsWith("UNITY_"))
                    {
                        allDefines.Add(define);
                    }
                }

                SetCurrentDefines(string.Join(";", allDefines.OrderBy(d => d)));
                Debug.Log($"[ProjectDefines] Applied {preset} preset");
            }
        }

        private void SaveCurrentAsPreset()
        {
            var currentDefines = GetCurrentDefinesSet();
            Debug.Log($"[ProjectDefines] Current defines saved. Copy these to customize preset:\n{string.Join("\n", currentDefines.OrderBy(d => d))}");
            EditorGUIUtility.systemCopyBuffer = string.Join(";", currentDefines.OrderBy(d => d));
        }

        private void EnableAllVisibleDefines(bool enable)
        {
            var currentDefines = GetCurrentDefinesSet();
            var filtered = GetFilteredDefines();

            foreach (var kvp in filtered)
            {
                if (enable)
                {
                    currentDefines.Add(kvp.Key);
                }
                else
                {
                    currentDefines.Remove(kvp.Key);
                }
            }

            SetCurrentDefines(string.Join(";", currentDefines.OrderBy(d => d)));
        }

        private Dictionary<string, bool> GetFilteredDefines()
        {
            var currentDefines = GetCurrentDefinesSet();
            var result = new Dictionary<string, bool>();

            var allDefines = new HashSet<string>(defineRegistry.Keys);
            allDefines.UnionWith(currentDefines);

            foreach (var define in allDefines)
            {
                if (!string.IsNullOrEmpty(searchFilter) &&
                    !define.ToLower().Contains(searchFilter.ToLower()))
                {
                    continue;
                }

                bool isActive = currentDefines.Contains(define);
                if (showOnlyActive && !isActive)
                {
                    continue;
                }

                result[define] = isActive;
            }

            return result;
        }

        #endregion

        #region Define Info

        private struct DefineInfo
        {
            public string Name;
            public string Category;
            public string Description;

            public DefineInfo(string name, string category, string description)
            {
                Name = name;
                Category = category;
                Description = description;
            }
        }

        #endregion
    }
}
