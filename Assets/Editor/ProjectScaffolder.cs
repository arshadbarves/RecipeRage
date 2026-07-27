using System.Collections.Generic;
using System.IO;
using RecipeRage;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace RecipeRage.EditorTools
{
    /// <summary>
    /// Headless project scaffolding: generates all ScriptableObject content assets
    /// (ingredients, recipes, chefs, maps) from the spec's data tables. Run via
    /// 'unity run -executeMethod ProjectScaffolder.GenerateAll'.
    /// Idempotent — existing assets are overwritten, never duplicated.
    /// </summary>
    public static class ProjectScaffolder
    {
        private const string DataRoot = "Assets/Art/Data";

        public static void GenerateAll()
        {
            GenerateIngredients();
            GenerateRecipes();
            GenerateChefs();
            GenerateMaps();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Scaffolder] All content assets generated");
        }

        // ── Network prefabs ───────────────────────────────────────────────

        public static void GenerateNetworkPrefabs()
        {
            const string prefabRoot = "Assets/Game/Network/Prefabs";
            EnsureDir(prefabRoot + "/x.prefab");

            // NetworkPlayer prefab: capsule visual + CharacterController + PlayerController + NetworkPlayer + NetworkObject + ChefSelectionSync
            var playerGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerGo.name = "NetworkPlayer";
            playerGo.AddComponent<CharacterController>();
            playerGo.AddComponent<RecipeRage.PlayerController>();
            playerGo.AddComponent<RecipeRage.Net.NetworkPlayer>();
            playerGo.AddComponent<Unity.Netcode.NetworkObject>();
            playerGo.AddComponent<RecipeRage.Net.ChefSelectionSync>();
            SavePrefab(playerGo, $"{prefabRoot}/NetworkPlayer.prefab");

            // NetworkBot prefab: capsule + NetworkTransform + NetworkBot + NetworkObject + PlayerController + BotController
            var botGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            botGo.name = "NetworkBot";
            var botCc = botGo.AddComponent<CharacterController>();
            botGo.AddComponent<RecipeRage.PlayerController>();
            botGo.AddComponent<RecipeRage.Bots.BotController>();
            botGo.AddComponent<Unity.Netcode.Components.NetworkTransform>();
            botGo.AddComponent<RecipeRage.Net.NetworkBot>();
            botGo.AddComponent<Unity.Netcode.NetworkObject>();
            SavePrefab(botGo, $"{prefabRoot}/NetworkBot.prefab");

            // NetworkMatch prefab: empty + NetworkMatch + NetworkObject
            var matchGo = new GameObject("NetworkMatch");
            matchGo.AddComponent<RecipeRage.Net.NetworkMatch>();
            matchGo.AddComponent<Unity.Netcode.NetworkObject>();
            SavePrefab(matchGo, $"{prefabRoot}/NetworkMatch.prefab");

            // NetworkTeamRoster prefab
            var rosterGo = new GameObject("NetworkTeamRoster");
            rosterGo.AddComponent<RecipeRage.Net.NetworkTeamRoster>();
            rosterGo.AddComponent<Unity.Netcode.NetworkObject>();
            SavePrefab(rosterGo, $"{prefabRoot}/NetworkTeamRoster.prefab");

            // Station prefabs (one per station type with its network wrapper)
            SaveStationPrefab<RecipeRage.CookingStation, RecipeRage.Net.NetworkCookingStation>(prefabRoot, "CookingStation");
            SaveStationPrefab<RecipeRage.CuttingStation, RecipeRage.Net.NetworkCuttingStation>(prefabRoot, "CuttingStation");
            SaveStationPrefab<RecipeRage.ServingStation, RecipeRage.Net.NetworkServingStation>(prefabRoot, "ServingStation");

            AssetDatabase.SaveAssets();
            Debug.Log("[Scaffolder] Network prefabs generated (player, bot, match, roster, 3 stations)");
        }

        private static void SaveStationPrefab<TStation, TNetwork>(string root, string name)
            where TStation : Component
            where TNetwork : Component
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.AddComponent<TStation>();
            go.AddComponent<TNetwork>();
            go.AddComponent<Unity.Netcode.NetworkObject>();
            SavePrefab(go, $"{root}/{name}.prefab");
        }

        private static void SavePrefab(GameObject go, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        // ── Tutorial scene full wiring (player + steps + arrow + UI) ───────

        public static void WireTutorialScene()
        {
            const string scenePath = "Assets/Scenes/Tutorial.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Find stations
            var crate = Object.FindFirstObjectByType<RecipeRage.IngredientCrate>();
            var cutting = Object.FindFirstObjectByType<RecipeRage.CuttingStation>();
            var cooking = Object.FindFirstObjectByType<RecipeRage.CookingStation>();
            var plate = Object.FindFirstObjectByType<RecipeRage.PlateStation>();
            var serving = Object.FindFirstObjectByType<RecipeRage.ServingStation>();

            // Build the 9 rich steps (instruction, button hint, station color, target, condition, progress)
            var steps = new RecipeRage.TutorialStep[]
            {
                Step("Move around your kitchen.", "Press WASD / left stick to Move", "#8B5CF6", null, RecipeRage.TutorialCondition.MovedDistance, false),
                Step("Fetch an ingredient from the crate.", "Press LEFT-CLICK to Fetch", "#F59E0B", crate, RecipeRage.TutorialCondition.FetchedIngredient, false),
                Step("Chop the ingredient on the cutting board.", "Place it, then press RIGHT-CLICK rapidly to Chop", "#EC4899", cutting, RecipeRage.TutorialCondition.ChoppedIngredient, true),
                Step("Start cooking on the stove.", "Press LEFT-CLICK to place & start cooking", "#DC2626", cooking, RecipeRage.TutorialCondition.CookingStarted, false),
                Step("Collect it when ready — don't let it burn!", "Press LEFT-CLICK to Collect", "#22C55E", cooking, RecipeRage.TutorialCondition.CookingCollected, false),
                Step("Take a plate from the plate station.", "Press LEFT-CLICK to Take a Plate", "#8B5CF6", plate, RecipeRage.TutorialCondition.PlateTaken, false),
                Step("Arrange the cooked food onto your plate.", "Press LEFT-CLICK to Arrange", "#F59E0B", plate, RecipeRage.TutorialCondition.IngredientPlated, false),
                Step("Serve the dish at the serving counter!", "Press LEFT-CLICK to Serve", "#22C55E", serving, RecipeRage.TutorialCondition.RecipeServed, false),
                Step("Careful: food left on the stove too long burns.", "Watch the burn timer on the stove", "#DC2626", cooking, RecipeRage.TutorialCondition.BurnWarningShown, false),
            };

            // Wire TutorialController (steps only — the HUD is a UIDocument)
            var controller = Object.FindFirstObjectByType<RecipeRage.TutorialController>();
            if (controller == null)
            {
                var go = new GameObject("TutorialController");
                controller = go.AddComponent<RecipeRage.TutorialController>();
            }

            var so = new SerializedObject(controller);
            var stepsProp = so.FindProperty("_steps");
            stepsProp.arraySize = steps.Length;
            for (int i = 0; i < steps.Length; i++)
            {
                var el = stepsProp.GetArrayElementAtIndex(i);
                el.FindPropertyRelative("Instruction").stringValue = steps[i].Instruction;
                el.FindPropertyRelative("ButtonHint").stringValue = steps[i].ButtonHint;
                el.FindPropertyRelative("StationColorHex").stringValue = steps[i].StationColorHex;
                el.FindPropertyRelative("Condition").enumValueIndex = (int)steps[i].Condition;
                el.FindPropertyRelative("TrackProgress").boolValue = steps[i].TrackProgress;
                el.FindPropertyRelative("HighlightTarget").objectReferenceValue = steps[i].HighlightTarget;
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Scaffolder] Tutorial wired: {steps.Length} rich steps (UI Toolkit HUD)");
        }

        private static RecipeRage.TutorialStep Step(string instruction, string buttonHint, string colorHex, Component target, RecipeRage.TutorialCondition condition, bool trackProgress)
        {
            return new RecipeRage.TutorialStep
            {
                Instruction = instruction,
                ButtonHint = buttonHint,
                StationColorHex = colorHex,
                HighlightTarget = target != null ? target.transform : null,
                Condition = condition,
                TrackProgress = trackProgress
            };
        }

        // ── Wire chef model into player prefab ──────────────────────────────

        public static void WireChefModel()
        {
            const string prefabPath = "Assets/Game/Network/Prefabs/NetworkPlayer.prefab";
            const string chefModelPath = "Assets/Art/Characters/SK_Chef.glb";

            var prefab = PrefabUtility.LoadPrefabContents(prefabPath);
            var chefModel = AssetDatabase.LoadAssetAtPath<GameObject>(chefModelPath);
            if (chefModel == null)
            {
                Debug.LogError($"[Scaffolder] Chef model not found at {chefModelPath}");
                PrefabUtility.UnloadPrefabContents(prefab);
                return;
            }

            // Remove primitive capsule visuals
            foreach (var renderer in prefab.GetComponentsInChildren<MeshRenderer>(true))
            {
                Object.DestroyImmediate(renderer.gameObject.GetComponent<MeshFilter>());
                Object.DestroyImmediate(renderer);
            }

            // Add chef model if not already present
            if (prefab.transform.Find("Model") == null)
            {
                var modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(chefModel, prefab.transform);
                modelInstance.name = "Model";
                modelInstance.transform.localPosition = Vector3.zero;
            }

            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefab);
            Debug.Log("[Scaffolder] Wired chef model into NetworkPlayer prefab");
        }

        public static void WireBlenderArt()
        {
            var models = new System.Collections.Generic.Dictionary<string, string>
            {
                { "CookingStation", "Assets/Art/Stations/SM_Stove.glb" },
                { "CuttingStation", "Assets/Art/Stations/SM_CuttingBoard.glb" },
                { "ServingStation", "Assets/Art/Stations/SM_ServingCounter.glb" },
                { "CounterStation", "Assets/Art/Stations/SM_Counter.glb" },
                { "PlateStation", "Assets/Art/Stations/SM_PlateStation.glb" },
                { "IngredientCrate", "Assets/Art/Stations/SM_IngredientCrate.glb" },
            };

            // 1. Update network station prefabs
            var prefabPaths = new[]
            {
                "Assets/Game/Network/Prefabs/CookingStation.prefab",
                "Assets/Game/Network/Prefabs/CuttingStation.prefab",
                "Assets/Game/Network/Prefabs/ServingStation.prefab",
            };

            foreach (var prefabPath in prefabPaths)
            {
                var prefab = PrefabUtility.LoadPrefabContents(prefabPath);
                var stationName = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
                if (models.TryGetValue(stationName, out var modelPath))
                {
                    var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                    if (model != null)
                    {
                        foreach (var renderer in prefab.GetComponentsInChildren<MeshRenderer>(true))
                        {
                            Object.DestroyImmediate(renderer.gameObject.GetComponent<MeshFilter>());
                            Object.DestroyImmediate(renderer);
                        }

                        var modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(model, prefab.transform);
                        modelInstance.name = "Model";
                        modelInstance.transform.localPosition = Vector3.zero;

                        PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
                        Debug.Log($"[Scaffolder] Wired {stationName} model into {prefabPath}");
                    }
                }
                PrefabUtility.UnloadPrefabContents(prefab);
            }

            // 2. Update map + tutorial scenes
            var scenePaths = new[]
            {
                "Assets/Scenes/Maps/MapBeachBBQ.unity",
                "Assets/Scenes/Maps/MapForestCampfire.unity",
                "Assets/Scenes/Maps/MapPirateShip.unity",
                "Assets/Scenes/Tutorial.unity",
            };

            foreach (var scenePath in scenePaths)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                WireSceneStations(models);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[Scaffolder] Wired station models in {scenePath}");
            }

            AssetDatabase.SaveAssets();
        }

        private static void WireSceneStations(System.Collections.Generic.Dictionary<string, string> models)
        {
            foreach (var kvp in models)
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(kvp.Value);
                if (model == null)
                {
                    continue;
                }

                var type = FindStationType(kvp.Key);
                if (type == null)
                {
                    continue;
                }

                var stations = Object.FindObjectsByType(type, FindObjectsSortMode.None);
                foreach (var stationObj in stations)
                {
                    var go = ((Component)stationObj).gameObject;

                    if (go.transform.Find("Model") != null)
                    {
                        continue;
                    }

                    var renderer = go.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        Object.DestroyImmediate(renderer);
                    }
                    var meshFilter = go.GetComponent<MeshFilter>();
                    if (meshFilter != null)
                    {
                        Object.DestroyImmediate(meshFilter);
                    }

                    var modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(model, go.transform);
                    modelInstance.name = "Model";
                    modelInstance.transform.localPosition = Vector3.zero;
                }
            }
        }

        private static System.Type FindStationType(string name)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType($"RecipeRage.{name}");
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }

        public static void AssignIngredientsToCrates()
        {
            var scenePaths = new[]
            {
                "Assets/Scenes/Maps/MapBeachBBQ.unity",
                "Assets/Scenes/Maps/MapForestCampfire.unity",
                "Assets/Scenes/Maps/MapPirateShip.unity",
                "Assets/Scenes/Tutorial.unity",
            };

            var ingredientField = typeof(RecipeRage.IngredientCrate).GetField("_ingredient",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            foreach (var scenePath in scenePaths)
            {
                // Open scene FIRST, then load ingredients (scene unload destroys
                // previously-loaded SO references — same lesson as PanelSettings).
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                var ingredients = new System.Collections.Generic.List<RecipeRage.IngredientDefinition>();
                foreach (var guid in AssetDatabase.FindAssets("t:IngredientDefinition", new[] { "Assets/Art/Data/Ingredients" }))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<RecipeRage.IngredientDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                    if (asset != null)
                    {
                        ingredients.Add(asset);
                    }
                }

                if (ingredients.Count == 0)
                {
                    Debug.LogError("[Scaffolder] No IngredientDefinition assets found");
                    return;
                }

                var crates = Object.FindObjectsByType<RecipeRage.IngredientCrate>(FindObjectsSortMode.None);
                for (int i = 0; i < crates.Length; i++)
                {
                    var ingredient = ingredients[i % ingredients.Count];
                    ingredientField.SetValue(crates[i], ingredient);
                    EditorUtility.SetDirty(crates[i]);
                }

                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[Scaffolder] Assigned ingredients to {crates.Length} crates in {scenePath}");
            }

            AssetDatabase.SaveAssets();
        }

        public static void AddPlayerSpawnersToScenes()
        {
            // Copy NetworkPlayer prefab to Resources for runtime fallback loading
            const string srcPrefab = "Assets/Game/Network/Prefabs/NetworkPlayer.prefab";
            const string dstDir = "Assets/Resources/Prefabs";
            EnsureDir(dstDir + "/x.txt");
            if (AssetDatabase.LoadAssetAtPath<GameObject>($"{dstDir}/NetworkPlayer.prefab") == null)
            {
                AssetDatabase.CopyAsset(srcPrefab, $"{dstDir}/NetworkPlayer.prefab");
            }

            var scenePaths = new[]
            {
                "Assets/Scenes/Maps/MapBeachBBQ.unity",
                "Assets/Scenes/Maps/MapForestCampfire.unity",
                "Assets/Scenes/Maps/MapPirateShip.unity",
                "Assets/Scenes/Tutorial.unity",
            };

            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(srcPrefab);

            foreach (var scenePath in scenePaths)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                if (GameObject.Find("MatchPlayerSpawner") != null)
                {
                    continue; // already present
                }

                var spawnerGo = new GameObject("MatchPlayerSpawner");
                var spawner = spawnerGo.AddComponent<RecipeRage.MatchPlayerSpawner>();
                var so = new SerializedObject(spawner);
                so.FindProperty("_playerPrefab").objectReferenceValue = playerPrefab;
                so.FindProperty("_teamIndex").intValue = 0;
                so.FindProperty("_spawnSlot").intValue = 1;
                so.ApplyModifiedPropertiesWithoutUndo();

                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[Scaffolder] Player spawner added to {scenePath}");
            }

            AssetDatabase.SaveAssets();
        }

        public static void GenerateTutorialScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera (top-down)
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.AddComponent<Camera>();
            camGo.transform.position = new Vector3(0f, 12f, -8f);
            camGo.transform.rotation = Quaternion.Euler(60f, 0f, 0f);

            var lightGo = new GameObject("Directional Light");
            lightGo.AddComponent<Light>().type = LightType.Directional;

            // Runtime registry
            var registryGo = new GameObject("MatchRuntimeRegistry");
            var registry = registryGo.AddComponent<RecipeRage.Net.MatchRuntimeRegistry>();

            // Floor
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.position = Vector3.zero;

            // Player spawn
            var playerSpawn = new GameObject("PlayerSpawn");
            playerSpawn.transform.position = new Vector3(0f, 0.5f, 5f);

            // One of each station in a simple guided arc (per spec tutorial map)
            var layout = new MapLayout { SceneName = "Tutorial" };
            var root = new GameObject("TutorialStations");
            PlaceStations<RecipeRage.IngredientCrate>(root, 1, -3f, -3f, 2.5f, "Crate");
            PlaceStations<RecipeRage.CuttingStation>(root, 1, 0f, -3f, 2.5f, "Cutting");
            PlaceStations<RecipeRage.CookingStation>(root, 1, 3f, -3f, 2.5f, "Cooking");
            PlaceStations<RecipeRage.PlateStation>(root, 1, -1.5f, 0f, 2.5f, "Plate");
            PlaceStations<RecipeRage.ServingStation>(root, 1, 1.5f, 0f, 2.5f, "Serving");

            // TutorialController (steps wired in inspector later; component present)
            var tutorialGo = new GameObject("TutorialController");
            tutorialGo.AddComponent<RecipeRage.TutorialController>();

            var scenePath = "Assets/Scenes/Tutorial.unity";
            EnsureDir(scenePath);
            EditorSceneManager.SaveScene(scene, scenePath);

            // Add to Build Settings (preserve existing)
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!scenes.Exists(s => s.path == scenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[Scaffolder] Tutorial scene generated (1 of each station + controller)");
        }

        private sealed class MapLayout
        {
            public string SceneName;
            public int Crates = 2;
            public int Cutting = 2;
            public int Cooking = 2;
            public int Serving = 1;
            public string Theme;
        }

        public static void GenerateMapScenes()
        {
            var layouts = new[]
            {
                new MapLayout { SceneName = "MapBeachBBQ", Crates = 2, Cutting = 2, Cooking = 2, Serving = 1, Theme = "Beach BBQ" },
                new MapLayout { SceneName = "MapForestCampfire", Crates = 3, Cutting = 2, Cooking = 3, Serving = 2, Theme = "Forest Campfire" },
                new MapLayout { SceneName = "MapPirateShip", Crates = 2, Cutting = 2, Cooking = 2, Serving = 1, Theme = "Pirate Ship" },
            };

            foreach (var layout in layouts)
            {
                BuildMapScene(layout);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Scaffolder] {layouts.Length} map scenes generated");
        }

        private static void BuildMapScene(MapLayout layout)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera (top-down, angled)
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            camGo.transform.position = new Vector3(0f, 16f, -10f);
            camGo.transform.rotation = Quaternion.Euler(60f, 0f, 0f);

            var lightGo = new GameObject("Directional Light");
            lightGo.AddComponent<Light>().type = LightType.Directional;

            // Runtime registry (stations register here)
            var registryGo = new GameObject("MatchRuntimeRegistry");
            registryGo.AddComponent<RecipeRage.Net.MatchRuntimeRegistry>();

            // Floor (two team halves)
            var floorA = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floorA.name = "Floor_TeamA";
            floorA.transform.position = new Vector3(-11f, 0f, 0f);
            var floorB = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floorB.name = "Floor_TeamB";
            floorB.transform.position = new Vector3(11f, 0f, 0f);

            // Mirrored team kitchens
            BuildTeamKitchen(-11f, 0, layout); // Team A (left)
            BuildTeamKitchen(11f, 1, layout);  // Team B (right)

            // Team spawn points
            var spawns = new GameObject("SpawnPoints");
            for (int i = 0; i < 3; i++)
            {
                var a = new GameObject($"Spawn_TeamA_{i}");
                a.transform.SetParent(spawns.transform, false);
                a.transform.position = new Vector3(-11f + (i - 1) * 2f, 0.5f, 6f);
                var b = new GameObject($"Spawn_TeamB_{i}");
                b.transform.SetParent(spawns.transform, false);
                b.transform.position = new Vector3(11f + (i - 1) * 2f, 0.5f, 6f);
            }

            var scenePath = $"Assets/Scenes/Maps/{layout.SceneName}.unity";
            EnsureDir(scenePath);
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[Scaffolder] Map scene: {layout.SceneName} ({layout.Theme})");
        }

        private static void BuildTeamKitchen(float centerX, int teamId, MapLayout layout)
        {
            var root = new GameObject(teamId == 0 ? "Kitchen_TeamA" : "Kitchen_TeamB");
            root.transform.position = Vector3.zero;

            // Station layout grid (relative to team center)
            // Row 1 (back): crates    Row 2: cutting    Row 3: plate+counter    Row 4: cooking    Row 5 (front): serving
            PlaceStations<RecipeRage.IngredientCrate>(root, layout.Crates, centerX, -4f, 2.5f, "Crate");
            PlaceStations<RecipeRage.CuttingStation>(root, layout.Cutting, centerX, -1.5f, 2.5f, "Cutting");
            PlaceStations<RecipeRage.PlateStation>(root, 1, centerX - 1.25f, 1f, 2.5f, "Plate");
            PlaceStations<RecipeRage.CounterStation>(root, 1, centerX + 1.25f, 1f, 2.5f, "Counter");
            PlaceStations<RecipeRage.CookingStation>(root, layout.Cooking, centerX, 3.5f, 2.5f, "Cooking");
            PlaceStations<RecipeRage.ServingStation>(root, layout.Serving, centerX, 6f, 2.5f, "Serving");
        }

        private static void PlaceStations<T>(GameObject root, int count, float centerX, float z, float spacing, string name) where T : Component
        {
            for (int i = 0; i < count; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"{name}_{i}";
                go.transform.SetParent(root.transform, false);
                var offset = (i - (count - 1) / 2f) * spacing;
                go.transform.position = new Vector3(centerX + offset, 0.5f, z);
                go.AddComponent<T>();

                // Register with scene registry
                var registry = Object.FindFirstObjectByType<RecipeRage.Net.MatchRuntimeRegistry>();
                if (registry != null && go.GetComponent<T>() is RecipeRage.StationBase station)
                {
                    registry.RegisterStation(station);
                }
            }
        }

        public static void GenerateBootScene()
        {
            // Reuse the existing MainMixer.mixer (Master/Music/SFX + 3 exposed volumes).
            // AudioMixerController is internal API — mixer is authored once and reused.
            var mixerPath = "Assets/Art/Audio/MainMixer.mixer";
            var mixer = AssetDatabase.LoadAssetAtPath<UnityEngine.Audio.AudioMixer>(mixerPath);
            if (mixer == null)
            {
                Debug.LogError($"[Scaffolder] Mixer not found at {mixerPath} — copy MainMixer.mixer there first");
                return;
            }

            // AudioClipMap (empty entries — clips assigned later)
            var clipMapPath = "Assets/Art/Audio/AudioClipMap.asset";
            var clipMap = AssetDatabase.LoadAssetAtPath<Playcenter.Services.AudioClipMap>(clipMapPath);
            if (clipMap == null)
            {
                clipMap = ScriptableObject.CreateInstance<Playcenter.Services.AudioClipMap>();
                AssetDatabase.CreateAsset(clipMap, clipMapPath);
            }

            // Boot scene with both composition roots
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var sdkRoot = new GameObject("PlaycenterCompositionRoot");
            var sdkComp = sdkRoot.AddComponent<Playcenter.PlaycenterCompositionRoot>();
            var so = new SerializedObject(sdkComp);
            so.FindProperty("_mainMixer").objectReferenceValue = mixer;
            so.FindProperty("_clipMap").objectReferenceValue = clipMap;
            so.ApplyModifiedPropertiesWithoutUndo();

            var gameRoot = new GameObject("GameplayCompositionRoot");
            var gameComp = gameRoot.AddComponent<RecipeRage.GameplayCompositionRoot>();
            var gso = new SerializedObject(gameComp);
            WireArray(gso, "_allRecipes", LoadAll<RecipeDefinition>($"{DataRoot}/Recipes"));
            WireArray(gso, "_allChefs", LoadAll<ChefDefinition>($"{DataRoot}/Chefs"));
            WireArray(gso, "_allMaps", LoadAll<MapDefinition>($"{DataRoot}/Maps"));
            gso.ApplyModifiedPropertiesWithoutUndo();

            // Camera + light for boot
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.AddComponent<Camera>();
            camGo.transform.position = new Vector3(0f, 12f, -8f);
            camGo.transform.rotation = Quaternion.Euler(60f, 0f, 0f);

            var lightGo = new GameObject("Directional Light");
            lightGo.AddComponent<Light>().type = LightType.Directional;

            EnsureDir("Assets/Scenes/Boot.unity");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Boot.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Boot.unity", true) };

            AssetDatabase.SaveAssets();
            Debug.Log("[Scaffolder] Boot scene generated (composition roots + mixer + clip map + camera)");
        }

        private static T[] LoadAll<T>(string folder) where T : Object
        {
            var list = new List<T>();
            if (!Directory.Exists(folder))
            {
                return list.ToArray();
            }
            foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    list.Add(asset);
                }
            }
            return list.ToArray();
        }

        private static void WireArray(SerializedObject so, string propName, Object[] items)
        {
            var prop = so.FindProperty(propName);
            if (prop == null || !prop.isArray)
            {
                Debug.LogWarning($"[Scaffolder] Property {propName} not found or not array");
                return;
            }
            prop.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            }
        }

        private static void EnsureDir(string assetPath)
        {
            var dir = Path.GetDirectoryName(assetPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        // ── Ingredients (10) ────────────────────────────────────────────────

        private sealed class IngredientSpec
        {
            public IngredientType Type;
            public string Name;
            public bool Chop = true;
            public bool Cook = true;
            public int Taps = 8;
            public float CookSec = 12f;
        }

        private static void GenerateIngredients()
        {
            var specs = new[]
            {
                new IngredientSpec { Type = IngredientType.Tomato, Name = "Tomato", Taps = 8, CookSec = 12f },
                new IngredientSpec { Type = IngredientType.Onion, Name = "Onion", Taps = 8, CookSec = 12f },
                new IngredientSpec { Type = IngredientType.Garlic, Name = "Garlic", Chop = false, CookSec = 12f },
                new IngredientSpec { Type = IngredientType.Lettuce, Name = "Lettuce", Taps = 8, Cook = false },
                new IngredientSpec { Type = IngredientType.Mushroom, Name = "Mushroom", Taps = 10, CookSec = 15f },
                new IngredientSpec { Type = IngredientType.Chicken, Name = "Chicken", Taps = 10, CookSec = 18f },
                new IngredientSpec { Type = IngredientType.Beef, Name = "Beef", Taps = 12, CookSec = 18f },
                new IngredientSpec { Type = IngredientType.Fish, Name = "Fish", Taps = 10, CookSec = 15f },
                new IngredientSpec { Type = IngredientType.Rice, Name = "Rice", Chop = false, CookSec = 15f },
                new IngredientSpec { Type = IngredientType.Pasta, Name = "Pasta", Chop = false, CookSec = 12f },
            };

            foreach (var spec in specs)
            {
                var asset = ScriptableObject.CreateInstance<IngredientDefinition>();
                SetField(asset, "_type", spec.Type);
                SetField(asset, "_displayName", spec.Name);
                SetField(asset, "_requiresChopping", spec.Chop);
                SetField(asset, "_requiresCooking", spec.Cook);
                SetField(asset, "_chopTaps", spec.Taps);
                SetField(asset, "_cookSeconds", spec.CookSec);
                Save(asset, $"{DataRoot}/Ingredients/{spec.Name}.asset");
            }
            Debug.Log($"[Scaffolder] Ingredients: {specs.Length}");
        }

        // ── Recipes (12: 4 easy, 4 medium, 4 hard) ──────────────────────────

        private sealed class Req
        {
            public IngredientType Type;
            public bool Chopped = true;
            public bool Cooked = true;
            public Req(IngredientType t, bool ch = true, bool co = true) { Type = t; Chopped = ch; Cooked = co; }
        }

        private sealed class RecipeSpec
        {
            public string Id;
            public string Name;
            public RecipeTier Tier;
            public Req[] Ingredients;
        }

        private static void GenerateRecipes()
        {
            var specs = new[]
            {
                // Easy (2 ingredients)
                new RecipeSpec { Id = "tomato_soup", Name = "Tomato Soup", Tier = RecipeTier.Easy, Ingredients = new[] { new Req(IngredientType.Tomato), new Req(IngredientType.Onion) } },
                new RecipeSpec { Id = "garden_salad", Name = "Garden Salad", Tier = RecipeTier.Easy, Ingredients = new[] { new Req(IngredientType.Lettuce, true, false), new Req(IngredientType.Tomato, true, false) } },
                new RecipeSpec { Id = "garlic_rice", Name = "Garlic Rice", Tier = RecipeTier.Easy, Ingredients = new[] { new Req(IngredientType.Rice, false), new Req(IngredientType.Garlic, false) } },
                new RecipeSpec { Id = "onion_pasta", Name = "Onion Pasta", Tier = RecipeTier.Easy, Ingredients = new[] { new Req(IngredientType.Pasta, false), new Req(IngredientType.Onion) } },
                // Medium (3 ingredients)
                new RecipeSpec { Id = "mushroom_pasta", Name = "Mushroom Pasta", Tier = RecipeTier.Medium, Ingredients = new[] { new Req(IngredientType.Pasta, false), new Req(IngredientType.Mushroom), new Req(IngredientType.Garlic, false) } },
                new RecipeSpec { Id = "chicken_rice", Name = "Chicken Rice", Tier = RecipeTier.Medium, Ingredients = new[] { new Req(IngredientType.Chicken), new Req(IngredientType.Rice, false), new Req(IngredientType.Onion) } },
                new RecipeSpec { Id = "fish_salad", Name = "Fish Salad", Tier = RecipeTier.Medium, Ingredients = new[] { new Req(IngredientType.Fish), new Req(IngredientType.Lettuce, true, false), new Req(IngredientType.Tomato, true, false) } },
                new RecipeSpec { Id = "beef_soup", Name = "Beef Soup", Tier = RecipeTier.Medium, Ingredients = new[] { new Req(IngredientType.Beef), new Req(IngredientType.Onion), new Req(IngredientType.Garlic, false) } },
                // Hard (3 ingredients, tougher ingredients)
                new RecipeSpec { Id = "beef_stew", Name = "Beef Stew", Tier = RecipeTier.Hard, Ingredients = new[] { new Req(IngredientType.Beef), new Req(IngredientType.Mushroom), new Req(IngredientType.Onion) } },
                new RecipeSpec { Id = "chicken_pasta", Name = "Chicken Pasta", Tier = RecipeTier.Hard, Ingredients = new[] { new Req(IngredientType.Chicken), new Req(IngredientType.Pasta, false), new Req(IngredientType.Mushroom) } },
                new RecipeSpec { Id = "fish_rice", Name = "Fish Rice", Tier = RecipeTier.Hard, Ingredients = new[] { new Req(IngredientType.Fish), new Req(IngredientType.Rice, false), new Req(IngredientType.Garlic, false) } },
                new RecipeSpec { Id = "full_platter", Name = "Full Platter", Tier = RecipeTier.Hard, Ingredients = new[] { new Req(IngredientType.Beef), new Req(IngredientType.Chicken), new Req(IngredientType.Mushroom) } },
            };

            foreach (var spec in specs)
            {
                var asset = ScriptableObject.CreateInstance<RecipeDefinition>();
                SetField(asset, "_id", spec.Id);
                SetField(asset, "_displayName", spec.Name);
                SetField(asset, "_tier", spec.Tier);

                var reqs = new IngredientRequirement[spec.Ingredients.Length];
                for (int i = 0; i < reqs.Length; i++)
                {
                    var r = new IngredientRequirement();
                    SetFieldObject(r, "_type", spec.Ingredients[i].Type);
                    SetFieldObject(r, "_requiresChopped", spec.Ingredients[i].Chopped);
                    SetFieldObject(r, "_requiresCooked", spec.Ingredients[i].Cooked);
                    reqs[i] = r;
                }
                SetField(asset, "_requiredIngredients", reqs);
                Save(asset, $"{DataRoot}/Recipes/{spec.Name.Replace(" ", "")}.asset");
            }
            Debug.Log($"[Scaffolder] Recipes: {specs.Length}");
        }

        // ── Chefs (4 + 2 locked) ────────────────────────────────────────────

        private sealed class ChefSpec
        {
            public ChefId Id;
            public string Name;
            public ChefRarity Rarity;
            public int Cost;
            public ChefAbilityType Ability;
            public float[] PerLevel;
        }

        private static void GenerateChefs()
        {
            var specs = new[]
            {
                new ChefSpec { Id = ChefId.Gordon, Name = "Gordon", Rarity = ChefRarity.Common, Cost = 0, Ability = ChefAbilityType.MoveSpeed, PerLevel = new[] { 0.01f, 0.02f, 0.03f, 0.04f, 0.05f, 0.06f, 0.07f, 0.08f, 0.09f, 0.10f } },
                new ChefSpec { Id = ChefId.Julia, Name = "Julia", Rarity = ChefRarity.Common, Cost = 0, Ability = ChefAbilityType.PickupDropSpeed, PerLevel = new[] { 0.015f, 0.03f, 0.045f, 0.06f, 0.075f, 0.09f, 0.105f, 0.12f, 0.135f, 0.15f } },
                new ChefSpec { Id = ChefId.Marco, Name = "Marco", Rarity = ChefRarity.Rare, Cost = 500, Ability = ChefAbilityType.CarryCapacity, PerLevel = new[] { 0f, 0f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 2f } },
                new ChefSpec { Id = ChefId.Gustavo, Name = "Gustavo", Rarity = ChefRarity.Epic, Cost = 2000, Ability = ChefAbilityType.Dash, PerLevel = new[] { 30f, 28f, 26f, 24f, 22f, 20f, 18f, 16f, 14f, 10f } },
                new ChefSpec { Id = ChefId.Locked5, Name = "Coming Soon", Rarity = ChefRarity.Rare, Cost = 0, Ability = ChefAbilityType.MoveSpeed, PerLevel = new float[10] },
                new ChefSpec { Id = ChefId.Locked6, Name = "Coming Soon", Rarity = ChefRarity.Legendary, Cost = 0, Ability = ChefAbilityType.MoveSpeed, PerLevel = new float[10] },
            };

            foreach (var spec in specs)
            {
                var asset = ScriptableObject.CreateInstance<ChefDefinition>();
                SetField(asset, "_id", spec.Id);
                SetField(asset, "_displayName", spec.Name);
                SetField(asset, "_rarity", spec.Rarity);
                SetField(asset, "_unlockCost", spec.Cost);
                SetField(asset, "_abilityType", spec.Ability);
                SetField(asset, "_abilityPerLevel", spec.PerLevel);
                Save(asset, $"{DataRoot}/Chefs/{spec.Id}.asset");
            }
            Debug.Log($"[Scaffolder] Chefs: {specs.Length}");
        }

        // ── Maps (3) ────────────────────────────────────────────────────────

        private static void GenerateMaps()
        {
            var specs = new[]
            {
                new { Id = "beach_bbq", Name = "Beach BBQ", Scene = "MapBeachBBQ" },
                new { Id = "forest_campfire", Name = "Forest Campfire", Scene = "MapForestCampfire" },
                new { Id = "pirate_ship", Name = "Pirate Ship", Scene = "MapPirateShip" },
            };

            foreach (var spec in specs)
            {
                var asset = ScriptableObject.CreateInstance<MapDefinition>();
                SetField(asset, "_id", spec.Id);
                SetField(asset, "_displayName", spec.Name);
                SetField(asset, "_sceneName", spec.Scene);
                Save(asset, $"{DataRoot}/Maps/{spec.Name.Replace(" ", "")}.asset");
            }
            Debug.Log($"[Scaffolder] Maps: {specs.Length}");
        }

        // ── Panel settings (proper theme + scale) ───────────────────────────

        public static void ConfigurePanelSettings()
        {
            const string themePath = "Assets/Game/UI/Styles/ClayKitchenTheme.tss";
            const string panelSettingsPath = "Assets/Game/UI/PanelSettings.asset";

            // Theme asset should exist (written as text); if not, create it
            var theme = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.ThemeStyleSheet>(themePath);
            if (theme == null)
            {
                Debug.LogError($"[Scaffolder] ClayKitchenTheme.tss not found at {themePath} — write it first");
                return;
            }

            // Configure PanelSettings: theme + landscape reference resolution
            var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(panelSettingsPath);
            if (panelSettings == null)
            {
                panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(panelSettings, panelSettingsPath);
            }

            var so = new SerializedObject(panelSettings);
            so.FindProperty("themeUss").objectReferenceValue = theme;
            so.FindProperty("m_ScaleMode").intValue = 1; // ScaleWithScreenSize
            so.FindProperty("m_ReferenceResolution").vector2Value = new Vector2(1920f, 1080f); // landscape
            so.FindProperty("m_ScreenMatchMode").intValue = 0; // MatchWidthOrHeight
            so.FindProperty("m_Match").floatValue = 0.5f; // balance width/height
            so.FindProperty("m_ReferenceDpi").floatValue = 96f;
            so.FindProperty("m_FallbackDpi").floatValue = 96f;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(panelSettings);
            AssetDatabase.SaveAssets();
            Debug.Log("[Scaffolder] PanelSettings configured (ClayKitchenTheme + 1920x1080 landscape)");
        }

        private sealed class ScreenSpec
        {
            public string Name;
            public string UxmlPath;
            public string ComponentType;
        }

        public static void GenerateUIWiring()
        {
            const string uiRoot = "Assets/Game/UI";
            var screens = new[]
            {
                new ScreenSpec { Name = "SplashScreen", UxmlPath = $"{uiRoot}/UXML/SplashScreen.uxml", ComponentType = "RecipeRage.UI.SplashScreen" },
                new ScreenSpec { Name = "TutorialHUD", UxmlPath = $"{uiRoot}/UXML/TutorialHUD.uxml", ComponentType = "RecipeRage.UI.TutorialHUD" },
                new ScreenSpec { Name = "LoginScreen", UxmlPath = $"{uiRoot}/UXML/LoginScreen.uxml", ComponentType = "RecipeRage.UI.LoginScreen" },
                new ScreenSpec { Name = "MainMenuScreen", UxmlPath = $"{uiRoot}/UXML/MainMenuScreen.uxml", ComponentType = "RecipeRage.UI.MainMenuScreen" },
                new ScreenSpec { Name = "LobbyScreen", UxmlPath = $"{uiRoot}/UXML/LobbyScreen.uxml", ComponentType = "RecipeRage.UI.LobbyScreen" },
                new ScreenSpec { Name = "MatchmakingScreen", UxmlPath = $"{uiRoot}/UXML/MatchmakingScreen.uxml", ComponentType = "RecipeRage.UI.MatchmakingScreen" },
                new ScreenSpec { Name = "TeamCompositionScreen", UxmlPath = $"{uiRoot}/UXML/TeamCompositionScreen.uxml", ComponentType = "RecipeRage.UI.TeamCompositionScreen" },
                new ScreenSpec { Name = "CountdownScreen", UxmlPath = $"{uiRoot}/UXML/CountdownScreen.uxml", ComponentType = "RecipeRage.UI.CountdownScreen" },
                new ScreenSpec { Name = "HUDScreen", UxmlPath = $"{uiRoot}/UXML/HUDScreen.uxml", ComponentType = "RecipeRage.UI.HUDScreen" },
                new ScreenSpec { Name = "ResultsScreen", UxmlPath = $"{uiRoot}/UXML/ResultsScreen.uxml", ComponentType = "RecipeRage.UI.ResultsScreen" },
                new ScreenSpec { Name = "ChefsScreen", UxmlPath = $"{uiRoot}/UXML/ChefsScreen.uxml", ComponentType = "RecipeRage.UI.ChefsScreen" },
                new ScreenSpec { Name = "ShopScreen", UxmlPath = $"{uiRoot}/UXML/ShopScreen.uxml", ComponentType = "RecipeRage.UI.ShopScreen" },
                new ScreenSpec { Name = "FriendsScreen", UxmlPath = $"{uiRoot}/UXML/FriendsScreen.uxml", ComponentType = "RecipeRage.UI.FriendsScreen" },
                new ScreenSpec { Name = "SettingsScreen", UxmlPath = $"{uiRoot}/UXML/SettingsScreen.uxml", ComponentType = "RecipeRage.UI.SettingsScreen" },
                new ScreenSpec { Name = "DailyRewardsScreen", UxmlPath = $"{uiRoot}/UXML/DailyRewardsScreen.uxml", ComponentType = "RecipeRage.UI.DailyRewardsScreen" },
                new ScreenSpec { Name = "GameModeScreen", UxmlPath = $"{uiRoot}/UXML/GameModeScreen.uxml", ComponentType = "RecipeRage.UI.GameModeScreen" },
                new ScreenSpec { Name = "ProfileScreen", UxmlPath = $"{uiRoot}/UXML/ProfileScreen.uxml", ComponentType = "RecipeRage.UI.ProfileScreen" },
            };

            // Open Boot scene FIRST (loading assets after scene open avoids the
            // scene-unload invalidating the loaded PanelSettings reference).
            var bootScene = EditorSceneManager.OpenScene("Assets/Scenes/Boot.unity", OpenSceneMode.Single);
            var existingRoot = GameObject.Find("UIRoot");
            if (existingRoot != null)
            {
                Object.DestroyImmediate(existingRoot);
            }
            var uiRootGo = new GameObject("UIRoot");
            var screenComponents = new List<Component>();

            // PanelSettings (shared across all screens) — load AFTER scene open
            var panelSettingsPath = $"{uiRoot}/PanelSettings.asset";
            var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(panelSettingsPath);
            if (panelSettings == null)
            {
                panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(panelSettings, panelSettingsPath);
            }

            foreach (var spec in screens)
            {
                var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(spec.UxmlPath);
                if (uxml == null)
                {
                    Debug.LogWarning($"[Scaffolder] UXML not found: {spec.UxmlPath}");
                    continue;
                }

                var screenGo = new GameObject(spec.Name);
                screenGo.transform.SetParent(uiRootGo.transform, false);

                var doc = screenGo.AddComponent<UIDocument>();
                doc.panelSettings = panelSettings;
                doc.visualTreeAsset = uxml;
                EditorUtility.SetDirty(doc);
                if (doc.panelSettings == null)
                {
                    Debug.LogWarning($"[Scaffolder] panelSettings assignment failed on {spec.Name} (panelSettings={(panelSettings == null ? "null" : panelSettings.name)})");
                }

                var compType = FindType(spec.ComponentType);
                if (compType != null)
                {
                    var comp = screenGo.AddComponent(compType);
                    screenComponents.Add(comp);
                }
                else
                {
                    Debug.LogWarning($"[Scaffolder] Component type not found: {spec.ComponentType}");
                }
            }

            // UIScreenRegistry with all screens
            var registry = uiRootGo.AddComponent<Playcenter.UI.UIScreenRegistry>();
            var so = new SerializedObject(registry);
            var prop = so.FindProperty("_screens");
            prop.arraySize = screenComponents.Count;
            for (int i = 0; i < screenComponents.Count; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = screenComponents[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(bootScene);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Scaffolder] UI wiring complete: {screenComponents.Count} screens registered");
        }

        private static System.Type FindType(string fullName)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }

        private static void Save(Object asset, string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var existing = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(asset, existing);
                EditorUtility.SetDirty(existing);
            }
            else
            {
                AssetDatabase.CreateAsset(asset, path);
            }
        }

        private static void SetField(Object target, string fieldName, object value)
        {
            SetFieldObject((object)target, fieldName, value);
        }

        private static void SetFieldObject(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }
    }
}
