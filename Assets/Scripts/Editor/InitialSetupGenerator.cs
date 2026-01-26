using UnityEngine;
using UnityEditor;
using Gameplay.Characters;
using Gameplay.GameModes;
using System.IO;

namespace RecipeRage.Editor
{
    /// <summary>
    /// Initial setup generator - creates default characters and game modes out-of-the-box
    /// </summary>
    public class InitialSetupGenerator : EditorWindow
    {
        private bool createCharacters = true;
        private bool createGameModes = true;
        private bool createMaps = true;

        private Vector2 scrollPosition;

        [MenuItem("RecipeRage/Setup/Generate Initial Content")]
        public static void ShowWindow()
        {
            GetWindow<InitialSetupGenerator>("Initial Setup");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Initial Content Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("This will generate default characters, game modes, and maps for your game.", MessageType.Info);
            EditorGUILayout.Space();

            // Options
            EditorGUILayout.LabelField("Generate:", EditorStyles.boldLabel);
            createCharacters = EditorGUILayout.Toggle("Default Characters (4)", createCharacters);
            createGameModes = EditorGUILayout.Toggle("Default Game Modes (3)", createGameModes);
            createMaps = EditorGUILayout.Toggle("Default Maps (2)", createMaps);

            EditorGUILayout.Space();

            if (createCharacters)
            {
                EditorGUILayout.LabelField("Characters to be created:", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Chef Speedy - Fast movement specialist", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Chef Strong - High capacity carrier", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Chef Swift - Quick interactions", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Chef Balanced - All-rounder", EditorStyles.miniLabel);
                EditorGUILayout.Space();
            }

            if (createGameModes)
            {
                EditorGUILayout.LabelField("Game Modes to be created:", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Classic - Traditional 2v2", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Team Battle - 3v3 competitive", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Free For All - Solo competition", EditorStyles.miniLabel);
                EditorGUILayout.Space();
            }

            if (createMaps)
            {
                EditorGUILayout.LabelField("Maps to be created:", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Tutorial Kitchen - Easy starter map", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Kitchen Arena - Competitive map", EditorStyles.miniLabel);
                EditorGUILayout.Space();
            }

            EditorGUILayout.Space();

            // Generate Button
            if (GUILayout.Button("Generate All Initial Content", GUILayout.Height(40)))
            {
                GenerateInitialContent();
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // Individual Buttons
            EditorGUILayout.LabelField("Or generate individually:", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Characters Only"))
            {
                GenerateDefaultCharacters();
            }
            if (GUILayout.Button("Generate Game Modes Only"))
            {
                GenerateDefaultGameModes();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Generate Maps Only"))
            {
                GenerateDefaultMaps();
            }

            EditorGUILayout.EndScrollView();
        }

        private void GenerateInitialContent()
        {
            int generated = 0;

            if (createCharacters)
            {
                generated += GenerateDefaultCharacters();
            }

            if (createGameModes)
            {
                generated += GenerateDefaultGameModes();
            }

            if (createMaps)
            {
                generated += GenerateDefaultMaps();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", $"Generated {generated} initial content assets!", "OK");
        }

        #region Character Generation

        private int GenerateDefaultCharacters()
        {
            string basePath = "Assets/Resources/ScriptableObjects/CharacterClasses";
            EnsureDirectory(basePath);

            int count = 0;
            count += CreateCharacter("Chef Speedy", 1, "Lightning-fast movement specialist", 1.5f, 0.8f, 0.7f,
                AbilityType.SpeedBoost, "Burst of speed for quick deliveries", 20f, 5f, true, basePath);

            count += CreateCharacter("Chef Strong", 2, "Heavy-duty carrier with high capacity", 0.7f, 0.9f, 1.8f,
                AbilityType.DoubleIngredients, "Carry double ingredients temporarily", 30f, 10f, true, basePath);

            count += CreateCharacter("Chef Swift", 3, "Master of quick interactions", 0.9f, 1.6f, 0.8f,
                AbilityType.InstantChop, "Instantly chop ingredients", 25f, 0f, true, basePath);

            count += CreateCharacter("Chef Balanced", 4, "Well-rounded all-purpose chef", 1.0f, 1.0f, 1.0f,
                AbilityType.PreventBurning, "Prevent dishes from burning", 35f, 15f, true, basePath);

            Debug.Log($"Generated {count} default characters");
            return count;
        }

        private int CreateCharacter(string name, int id, string description, float moveSpeed, float interactSpeed,
            float capacity, AbilityType abilityType, string abilityDesc, float cooldown, float duration,
            bool unlocked, string basePath)
        {
            // Create Stats
            CharacterStats stats = ScriptableObject.CreateInstance<CharacterStats>();
            stats.name = $"{name.Replace(" ", "")}Stats";
            string statsPath = $"{basePath}/{stats.name}.asset";

            if (AssetDatabase.LoadAssetAtPath<CharacterStats>(statsPath) != null)
            {
                Debug.LogWarning($"Stats already exists: {statsPath}");
                return 0;
            }

            AssetDatabase.CreateAsset(stats, statsPath);
            SerializedObject statsObj = new SerializedObject(stats);
            statsObj.FindProperty("_movementSpeedModifier").floatValue = moveSpeed;
            statsObj.FindProperty("_interactionSpeedModifier").floatValue = interactSpeed;
            statsObj.FindProperty("_carryingCapacityModifier").floatValue = capacity;
            statsObj.ApplyModifiedProperties();

            // Create Ability
            CharacterAbilityData abilityData = ScriptableObject.CreateInstance<CharacterAbilityData>();
            abilityData.name = $"{name.Replace(" ", "")}Ability";
            string abilityPath = $"{basePath}/{abilityData.name}.asset";

            AssetDatabase.CreateAsset(abilityData, abilityPath);
            SerializedObject abilityObj = new SerializedObject(abilityData);
            abilityObj.FindProperty("_abilityType").enumValueIndex = (int)abilityType;
            abilityObj.FindProperty("_cooldown").floatValue = cooldown;
            abilityObj.FindProperty("_duration").floatValue = duration;
            abilityObj.FindProperty("_description").stringValue = abilityDesc;
            abilityObj.FindProperty("_speedMultiplier").floatValue = 1.5f;
            abilityObj.FindProperty("_range").floatValue = 5f;
            abilityObj.FindProperty("_effectMultiplier").floatValue = 2f;
            abilityObj.FindProperty("_pushForce").floatValue = 10f;
            abilityObj.ApplyModifiedProperties();

            // Create Unlock
            CharacterUnlockData unlockData = ScriptableObject.CreateInstance<CharacterUnlockData>();
            unlockData.name = $"{name.Replace(" ", "")}Unlock";
            string unlockPath = $"{basePath}/{unlockData.name}.asset";

            AssetDatabase.CreateAsset(unlockData, unlockPath);
            SerializedObject unlockObj = new SerializedObject(unlockData);
            unlockObj.FindProperty("_unlockedByDefault").boolValue = unlocked;
            unlockObj.FindProperty("_unlockCost").intValue = unlocked ? 0 : 1000;
            unlockObj.FindProperty("_unlockLevel").intValue = 1;
            unlockObj.ApplyModifiedProperties();

            // Create Character
            CharacterClass character = ScriptableObject.CreateInstance<CharacterClass>();
            character.name = name.Replace(" ", "");
            string characterPath = $"{basePath}/{character.name}.asset";

            AssetDatabase.CreateAsset(character, characterPath);
            SerializedObject characterObj = new SerializedObject(character);
            characterObj.FindProperty("_id").intValue = id;
            characterObj.FindProperty("_displayName").stringValue = name;
            characterObj.FindProperty("_description").stringValue = description;
            characterObj.FindProperty("_stats").objectReferenceValue = stats;
            characterObj.FindProperty("_primaryAbility").objectReferenceValue = abilityData;
            characterObj.FindProperty("_unlockData").objectReferenceValue = unlockData;
            characterObj.ApplyModifiedProperties();

            Debug.Log($"Created character: {name}");
            return 1;
        }

        #endregion

        #region GameMode Generation

        private int GenerateDefaultGameModes()
        {
            string basePath = "Assets/Resources/GameModes";
            EnsureDirectory(basePath);

            int count = 0;
            count += CreateGameMode("classic", "Classic Mode", "Traditional Battle", "Traditional 2v2 cooking competition",
                GameModeCategory.Trophies, 2, 2, false, 300f, 1000, true, true, "KitchenArena", 100, 25, true, true, basePath);

            count += CreateGameMode("team_battle", "Team Battle", "Coordinate & Conquer", "Work together to outscore the opposing team",
                GameModeCategory.Trophies, 2, 3, false, 360f, 1500, true, true, "TeamArena", 150, 50, true, true, basePath);

            count += CreateGameMode("free_for_all", "Free For All", "Every Chef For Themselves", "No teams - only one winner!",
                GameModeCategory.Special, 4, 1, false, 240f, 800, true, true, "FFAKitchen", 120, 30, true, true, basePath);

            Debug.Log($"Generated {count} default game modes");
            return count;
        }

        private int CreateGameMode(string id, string displayName, string subtitle, string description,
            GameModeCategory category, int teamCount, int playersPerTeam, bool friendlyFire, float gameTime,
            int targetScore, bool hasTimeLimit, bool hasScoreLimit, string mapSceneName, int expReward,
            int coinBonus, bool powerUps, bool abilities, string basePath)
        {
            GameMode gameMode = ScriptableObject.CreateInstance<GameMode>();
            gameMode.name = displayName.Replace(" ", "");
            string path = $"{basePath}/{gameMode.name}.asset";

            if (AssetDatabase.LoadAssetAtPath<GameMode>(path) != null)
            {
                Debug.LogWarning($"Game mode already exists: {path}");
                return 0;
            }

            AssetDatabase.CreateAsset(gameMode, path);
            SerializedObject gameModeObj = new SerializedObject(gameMode);

            gameModeObj.FindProperty("_id").stringValue = id;
            gameModeObj.FindProperty("_displayName").stringValue = displayName;
            gameModeObj.FindProperty("_description").stringValue = description;
            gameModeObj.FindProperty("_subtitle").stringValue = subtitle;
            gameModeObj.FindProperty("_category").enumValueIndex = (int)category;
            gameModeObj.FindProperty("_teamCount").intValue = teamCount;
            gameModeObj.FindProperty("_playersPerTeam").intValue = playersPerTeam;
            gameModeObj.FindProperty("_friendlyFire").boolValue = friendlyFire;
            gameModeObj.FindProperty("_gameTime").floatValue = gameTime;
            gameModeObj.FindProperty("_targetScore").intValue = targetScore;
            gameModeObj.FindProperty("_hasTimeLimit").boolValue = hasTimeLimit;
            gameModeObj.FindProperty("_hasScoreLimit").boolValue = hasScoreLimit;
            gameModeObj.FindProperty("_mapSceneName").stringValue = mapSceneName;
            gameModeObj.FindProperty("_experienceReward").intValue = expReward;
            gameModeObj.FindProperty("_winBonusCoins").intValue = coinBonus;
            gameModeObj.FindProperty("_powerUpsEnabled").boolValue = powerUps;
            gameModeObj.FindProperty("_specialAbilitiesEnabled").boolValue = abilities;
            gameModeObj.FindProperty("_customParameters").stringValue = "";

            gameModeObj.ApplyModifiedProperties();

            Debug.Log($"Created game mode: {displayName}");
            return 1;
        }

        #endregion

        #region Map Generation

        private int GenerateDefaultMaps()
        {
            string basePath = "Assets/Resources/ScriptableObjects/Maps";
            EnsureDirectory(basePath);

            int count = 0;
            count += CreateMap("map_tutorial", "Tutorial Kitchen", "Learn the basics of cooking in this simple kitchen",
                120, 1, 2, Gameplay.Maps.MapDifficulty.Easy, 10, 0, 50, true, 1, "Scenes/Maps/Tutorial", basePath);

            count += CreateMap("map_arena", "Kitchen Arena", "A competitive cooking arena where chefs battle!",
                180, 2, 4, Gameplay.Maps.MapDifficulty.Medium, 30, -10, 100, true, 1, "Scenes/Maps/KitchenArena", basePath);

            Debug.Log($"Generated {count} default maps");
            return count;
        }

        private int CreateMap(string mapId, string mapName, string description, int duration, int minPlayers,
            int maxPlayers, Gameplay.Maps.MapDifficulty difficulty, int trophyWin, int trophyLoss, int coinWin,
            bool unlocked, int unlockLevel, string sceneAddress, string basePath)
        {
            Gameplay.Maps.MapData mapData = ScriptableObject.CreateInstance<Gameplay.Maps.MapData>();
            mapData.name = mapName.Replace(" ", "");
            string path = $"{basePath}/{mapData.name}.asset";

            if (AssetDatabase.LoadAssetAtPath<Gameplay.Maps.MapData>(path) != null)
            {
                Debug.LogWarning($"Map already exists: {path}");
                return 0;
            }

            AssetDatabase.CreateAsset(mapData, path);
            SerializedObject mapObj = new SerializedObject(mapData);

            mapObj.FindProperty("_mapId").stringValue = mapId;
            mapObj.FindProperty("_mapName").stringValue = mapName;
            mapObj.FindProperty("_description").stringValue = description;
            mapObj.FindProperty("_matchDurationSeconds").intValue = duration;
            mapObj.FindProperty("_minPlayers").intValue = minPlayers;
            mapObj.FindProperty("_maxPlayers").intValue = maxPlayers;
            mapObj.FindProperty("_difficulty").enumValueIndex = (int)difficulty;
            mapObj.FindProperty("_trophyRewardWin").intValue = trophyWin;
            mapObj.FindProperty("_trophyRewardLoss").intValue = trophyLoss;
            mapObj.FindProperty("_coinRewardWin").intValue = coinWin;
            mapObj.FindProperty("_unlockedByDefault").boolValue = unlocked;
            mapObj.FindProperty("_unlockLevel").intValue = unlockLevel;
            mapObj.FindProperty("_sceneAddress").stringValue = sceneAddress;

            mapObj.ApplyModifiedProperties();

            Debug.Log($"Created map: {mapName}");
            return 1;
        }

        #endregion

        #region Utilities

        private void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parentFolder = Path.GetDirectoryName(path);
                string newFolderName = Path.GetFileName(path);

                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    EnsureDirectory(parentFolder);
                }

                AssetDatabase.CreateFolder(parentFolder, newFolderName);
                AssetDatabase.Refresh();
            }
        }

        #endregion
    }
}
