/// <summary>
/// Editor script that creates stub scenes for the three v2 game modes.
/// Run once via Tools → RecipeRage → Create Mode Scenes.
///
/// Each scene gets:
///   • A Main Camera
///   • A Directional Light
///   • A MatchRuntimeSceneBinder placeholder (empty, for wiring in MatchContext)
///   • SpawnPoint markers (A_Spawn_1, A_Spawn_2, B_Spawn_1, B_Spawn_2)
///   • A placeholder AutonomousCookingStation cluster parent
///
/// Scenes are saved to Assets/Scenes/ and must be added to Build Settings manually.
/// </summary>
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using KitchenClash.Infrastructure.Configuration;

namespace KitchenClash.Editor
{
    public static class MapSceneGenerator
    {
        [MenuItem("Tools/RecipeRage/Create v2 Mode Scenes")]
        public static void CreateModeScenes()
        {
            CreateStubScene(GameConstants.Scenes.RushService,       "Rush Service — 2v2 Tug-of-War");
            CreateStubScene(GameConstants.Scenes.HellsKitchen,      "Hell's Kitchen — 3v3 Race to Score");
            CreateStubScene(GameConstants.Scenes.LastPlateStanding, "Last Plate Standing — 2v2 Best-of-3");

            AssetDatabase.SaveAssets();
            Debug.Log("[MapSceneGenerator] Created 3 stub map scenes. Add them to Build Settings.");
        }

        private static void CreateStubScene(string sceneName, string description)
        {
            string path = $"Assets/Scenes/{sceneName}.unity";

            // Skip if already exists
            if (System.IO.File.Exists(System.IO.Path.Combine(UnityEngine.Application.dataPath.Replace("Assets", ""), path)))
            {
                Debug.Log($"[MapSceneGenerator] Scene already exists, skipping: {path}");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
            scene.name = sceneName;

            // --- Description object ---
            var descObj = new GameObject($"[MAP] {description}");
            SceneManager.MoveGameObjectToScene(descObj, scene);

            // --- Spawn points parent ---
            var spawnsParent = new GameObject("SpawnPoints");
            SceneManager.MoveGameObjectToScene(spawnsParent, scene);

            CreateSpawnPoint("TeamA_Spawn_1", new Vector3(-8f, 0, -4f), spawnsParent);
            CreateSpawnPoint("TeamA_Spawn_2", new Vector3(-8f, 0,  4f), spawnsParent);
            CreateSpawnPoint("TeamB_Spawn_1", new Vector3( 8f, 0, -4f), spawnsParent);
            CreateSpawnPoint("TeamB_Spawn_2", new Vector3( 8f, 0,  4f), spawnsParent);

            // --- Station cluster placeholder ---
            var stationsParent = new GameObject("Stations [PLACEHOLDER — wire AutonomousCookingStation prefabs here]");
            SceneManager.MoveGameObjectToScene(stationsParent, scene);

            // --- MatchRuntimeSceneBinder placeholder ---
            var binderObj = new GameObject("MatchRuntimeSceneBinder [PLACEHOLDER]");
            SceneManager.MoveGameObjectToScene(binderObj, scene);

            // Save
            EditorSceneManager.SaveScene(scene, path);
            EditorSceneManager.CloseScene(scene, true);

            Debug.Log($"[MapSceneGenerator] Created stub scene: {path}");
        }

        private static void CreateSpawnPoint(string name, Vector3 position, GameObject parent)
        {
            var go = new GameObject(name);
            go.transform.parent        = parent.transform;
            go.transform.localPosition = position;

            // Visual marker in editor
            #if UNITY_EDITOR
            var icon = EditorGUIUtility.IconContent("sv_icon_dot8_pix16_gizmo");
            #endif
        }
    }
}
#endif
