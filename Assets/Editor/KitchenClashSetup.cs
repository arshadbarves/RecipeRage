using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Automates all manual Unity Editor setup for Kitchen Clash.
/// Menu: Tools > Kitchen Clash > Full Setup
/// Idempotent — safe to run multiple times.
/// </summary>
public static class KitchenClashSetup
{
    // ── Map data mirroring MapRegistry (editor code can't instantiate runtime DI types) ──
    private struct MapInfo
    {
        public string MapId;
        public string DisplayName;
        public StationInfo[] Stations;
    }

    private struct StationInfo
    {
        public string StationId;
        public string Type; // matches StationType enum name
        public int GridX;
        public int GridY;
    }

    private static readonly MapInfo[] Maps =
    {
        new MapInfo
        {
            MapId = "rookie_kitchen", DisplayName = "Rookie Kitchen",
            Stations = new[]
            {
                new StationInfo { StationId = "rk_ingredient", Type = "Ingredient", GridX = 0, GridY = 0 },
                new StationInfo { StationId = "rk_cooking",    Type = "Cooking",    GridX = 1, GridY = 0 },
                new StationInfo { StationId = "rk_serving",    Type = "Serving",    GridX = 2, GridY = 0 },
            }
        },
        new MapInfo
        {
            MapId = "sushi_shuffle", DisplayName = "Sushi Shuffle",
            Stations = new[]
            {
                new StationInfo { StationId = "ss_ingredient", Type = "Ingredient", GridX = 0, GridY = 0 },
                new StationInfo { StationId = "ss_prep",       Type = "Prep",       GridX = 1, GridY = 0 },
                new StationInfo { StationId = "ss_cooking",    Type = "Cooking",    GridX = 2, GridY = 0 },
                new StationInfo { StationId = "ss_serving",    Type = "Serving",    GridX = 3, GridY = 0 },
            }
        },
        new MapInfo
        {
            MapId = "burger_boulevard", DisplayName = "Burger Boulevard",
            Stations = new[]
            {
                new StationInfo { StationId = "bb_ingredient", Type = "Ingredient", GridX = 0, GridY = 0 },
                new StationInfo { StationId = "bb_prep",       Type = "Prep",       GridX = 1, GridY = 0 },
                new StationInfo { StationId = "bb_cooking",    Type = "Cooking",    GridX = 2, GridY = 0 },
                new StationInfo { StationId = "bb_serving",    Type = "Serving",    GridX = 3, GridY = 0 },
            }
        },
        new MapInfo
        {
            MapId = "pirate_pot", DisplayName = "Pirate Pot",
            Stations = new[]
            {
                new StationInfo { StationId = "pp_ingredient", Type = "Ingredient", GridX = 0, GridY = 0 },
                new StationInfo { StationId = "pp_cooking",    Type = "Cooking",    GridX = 1, GridY = 0 },
                new StationInfo { StationId = "pp_serving",    Type = "Serving",    GridX = 2, GridY = 0 },
            }
        },
        new MapInfo
        {
            MapId = "taco_truck", DisplayName = "Taco Truck",
            Stations = new[]
            {
                new StationInfo { StationId = "tt_ingredient1", Type = "Ingredient", GridX = 0, GridY = 0 },
                new StationInfo { StationId = "tt_ingredient2", Type = "Ingredient", GridX = 0, GridY = 1 },
                new StationInfo { StationId = "tt_prep",        Type = "Prep",       GridX = 1, GridY = 0 },
                new StationInfo { StationId = "tt_cooking",     Type = "Cooking",    GridX = 2, GridY = 0 },
                new StationInfo { StationId = "tt_serving",     Type = "Serving",    GridX = 3, GridY = 0 },
            }
        },
        new MapInfo
        {
            MapId = "space_station", DisplayName = "Space Station",
            Stations = new[]
            {
                new StationInfo { StationId = "ss2_ingredient", Type = "Ingredient", GridX = 0, GridY = 0 },
                new StationInfo { StationId = "ss2_prep1",      Type = "Prep",       GridX = 1, GridY = 0 },
                new StationInfo { StationId = "ss2_prep2",      Type = "Prep",       GridX = 1, GridY = 1 },
                new StationInfo { StationId = "ss2_cooking",    Type = "Cooking",    GridX = 2, GridY = 0 },
                new StationInfo { StationId = "ss2_serving",    Type = "Serving",    GridX = 3, GridY = 0 },
            }
        },
        new MapInfo
        {
            MapId = "volcano_kitchen", DisplayName = "Volcano Kitchen",
            Stations = new[]
            {
                new StationInfo { StationId = "vk_ingredient", Type = "Ingredient", GridX = 0, GridY = 0 },
                new StationInfo { StationId = "vk_prep",       Type = "Prep",       GridX = 1, GridY = 0 },
                new StationInfo { StationId = "vk_cooking1",   Type = "Cooking",    GridX = 2, GridY = 0 },
                new StationInfo { StationId = "vk_cooking2",   Type = "Cooking",    GridX = 2, GridY = 1 },
                new StationInfo { StationId = "vk_serving",    Type = "Serving",    GridX = 3, GridY = 0 },
            }
        },
        new MapInfo
        {
            MapId = "clash_kitchen", DisplayName = "Clash Kitchen",
            Stations = new[]
            {
                new StationInfo { StationId = "ck_ingredient1", Type = "Ingredient", GridX = 0, GridY = 0 },
                new StationInfo { StationId = "ck_ingredient2", Type = "Ingredient", GridX = 0, GridY = 1 },
                new StationInfo { StationId = "ck_prep",        Type = "Prep",       GridX = 1, GridY = 0 },
                new StationInfo { StationId = "ck_cooking1",    Type = "Cooking",    GridX = 2, GridY = 0 },
                new StationInfo { StationId = "ck_cooking2",    Type = "Cooking",    GridX = 2, GridY = 1 },
                new StationInfo { StationId = "ck_serving",     Type = "Serving",    GridX = 3, GridY = 0 },
            }
        },
        new MapInfo
        {
            MapId = "haunted_kitchen", DisplayName = "Haunted Kitchen",
            Stations = new[]
            {
                new StationInfo { StationId = "hk_ingredient", Type = "Ingredient", GridX = 0, GridY = 0 },
                new StationInfo { StationId = "hk_prep",       Type = "Prep",       GridX = 1, GridY = 0 },
                new StationInfo { StationId = "hk_cooking",    Type = "Cooking",    GridX = 2, GridY = 0 },
                new StationInfo { StationId = "hk_serving",    Type = "Serving",    GridX = 3, GridY = 0 },
                new StationInfo { StationId = "hk_sink",       Type = "Sink",       GridX = 1, GridY = 1 },
            }
        },
    };

    private const float StationSpacingX = 3f;
    private const float StationStartX = -6f;
    private const float StationSpacingY = 3f;

    [MenuItem("Tools/Kitchen Clash/Full Setup")]
    public static void RunFullSetup()
    {
        var log = new List<string>();

        // Save current scene so we can restore later
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        FixGameBootstrapPrefab(log);
        CreateMapScenes(log);
        SetupMainMenuScene(log);
        CreateAudioSettingsAsset(log);
        SetupBuildSettings(log);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[KitchenClashSetup] Full Setup Complete\n" + string.Join("\n", log));
    }

    // ────────────────────────────────────────────────────────────
    // 1. Fix GameBootstrap Prefab
    // ────────────────────────────────────────────────────────────
    private static void FixGameBootstrapPrefab(List<string> log)
    {
        const string prefabPath = "Assets/Prefabs/General/GameBootstrap.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            log.Add($"[SKIP] Prefab not found: {prefabPath}");
            return;
        }

        // Open prefab for editing
        var root = PrefabUtility.LoadPrefabContents(prefabPath);

        // Remove missing scripts
        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
        if (removed > 0) log.Add($"[FIX] Removed {removed} missing-script components from GameBootstrap");

        // Add RootLifetimeScope if missing
        if (root.GetComponent<RootLifetimeScope>() == null)
        {
            root.AddComponent<RootLifetimeScope>();
            log.Add("[ADD] RootLifetimeScope added to GameBootstrap");
        }
        else
        {
            log.Add("[OK] RootLifetimeScope already on GameBootstrap");
        }

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    // ────────────────────────────────────────────────────────────
    // 2. Create Map Scenes
    // ────────────────────────────────────────────────────────────
    private static void CreateMapScenes(List<string> log)
    {
        const string mapsDir = "Assets/Scenes/Maps";
        EnsureDirectory(mapsDir);

        foreach (var map in Maps)
        {
            string scenePath = $"{mapsDir}/{map.MapId}.unity";

            // Skip if scene already exists
            if (File.Exists(Path.GetFullPath(scenePath)))
            {
                log.Add($"[OK] Map scene exists: {scenePath}");
                continue;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // MapRoot
            var mapRoot = new GameObject("MapRoot");

            // MatchScope with MatchLifetimeScope
            var matchScope = new GameObject("MatchScope");
            matchScope.transform.SetParent(mapRoot.transform);
            matchScope.AddComponent<MatchLifetimeScope>();

            // Station placeholders
            foreach (var station in map.Stations)
            {
                var stationGo = new GameObject(station.StationId);
                stationGo.transform.SetParent(mapRoot.transform);
                float x = StationStartX + station.GridX * StationSpacingX;
                float z = station.GridY * StationSpacingY;
                stationGo.transform.localPosition = new Vector3(x, 0f, z);
            }

            // Directional Light
            var light = new GameObject("Directional Light");
            var lightComp = light.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Ground Plane
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(10f, 1f, 10f);

            EditorSceneManager.SaveScene(scene, scenePath);
            log.Add($"[CREATE] Map scene: {scenePath} ({map.Stations.Length} stations)");
        }
    }

    // ────────────────────────────────────────────────────────────
    // 3. Setup MainMenu Scene
    // ────────────────────────────────────────────────────────────
    private static void SetupMainMenuScene(List<string> log)
    {
        const string scenePath = "Assets/Scenes/MainMenu.unity";
        if (!File.Exists(Path.GetFullPath(scenePath)))
        {
            log.Add($"[SKIP] MainMenu scene not found: {scenePath}");
            return;
        }

        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // Find or create MenuScope
        var menuScope = GameObject.Find("MenuScope");
        if (menuScope == null)
        {
            menuScope = new GameObject("MenuScope");
            log.Add("[ADD] Created MenuScope GameObject in MainMenu");
        }

        if (menuScope.GetComponent<MenuLifetimeScope>() == null)
        {
            menuScope.AddComponent<MenuLifetimeScope>();
            log.Add("[ADD] MenuLifetimeScope added to MenuScope");
        }
        else
        {
            log.Add("[OK] MenuLifetimeScope already on MenuScope");
        }

        EditorSceneManager.SaveScene(scene);
    }

    // ────────────────────────────────────────────────────────────
    // 4. Build Settings
    // ────────────────────────────────────────────────────────────
    private static void SetupBuildSettings(List<string> log)
    {
        var scenes = new List<EditorBuildSettingsScene>();

        // Core scenes in order
        string[] coreScenes = {
            "Assets/Scenes/Bootstrap.unity",
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Game.unity",
        };

        foreach (var s in coreScenes)
        {
            if (File.Exists(Path.GetFullPath(s)))
                scenes.Add(new EditorBuildSettingsScene(s, true));
        }

        // Map scenes
        foreach (var map in Maps)
        {
            string path = $"Assets/Scenes/Maps/{map.MapId}.unity";
            if (File.Exists(Path.GetFullPath(path)))
                scenes.Add(new EditorBuildSettingsScene(path, true));
        }

        EditorBuildSettings.scenes = scenes.ToArray();
        log.Add($"[BUILD] Set {scenes.Count} scenes in Build Settings");
    }

    // ────────────────────────────────────────────────────────────
    // 5. AudioSettings ScriptableObject
    // ────────────────────────────────────────────────────────────
    private static void CreateAudioSettingsAsset(List<string> log)
    {
        const string assetPath = "Assets/_KitchenClash/ScriptableObjects/AudioSettings.asset";

        if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath) != null)
        {
            log.Add($"[OK] AudioSettings asset exists: {assetPath}");
            return;
        }

        EnsureDirectory("Assets/_KitchenClash/ScriptableObjects");

        var instance = ScriptableObject.CreateInstance<KitchenClash.Infrastructure.Audio.AudioSettings>();
        AssetDatabase.CreateAsset(instance, assetPath);
        log.Add($"[CREATE] AudioSettings asset: {assetPath}");
    }

    // ────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────
    private static void EnsureDirectory(string assetPath)
    {
        string fullPath = Path.GetFullPath(assetPath);
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
            AssetDatabase.Refresh();
        }
    }
}
