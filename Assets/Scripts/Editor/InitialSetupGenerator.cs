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

            EditorGUILayout.HelpBox("This will generate default characters and game modes for your game.", MessageType.Info);
            EditorGUILayout.Space();

            // Options
            EditorGUILayout.LabelField("Generate:", EditorStyles.boldLabel);
            createCharacters = EditorGUILayout.Toggle("Default Characters (4)", createCharacters);
            createGameModes = EditorGUILayout.Toggle("Default Game Modes (3)", createGameModes);

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
            // Only Chef Balanced is unlocked by default - others must be purchased
            count += CreateCharacter("Chef Speedy", 1, "Lightning-fast movement specialist", 1.5f, 0.8f, 0.7f,
                AbilityType.SpeedBoost, "Burst of speed for quick deliveries", 20f, 5f, false, 1500, basePath);

            count += CreateCharacter("Chef Strong", 2, "Heavy-duty carrier with high capacity", 0.7f, 0.9f, 1.8f,
                AbilityType.DoubleIngredients, "Carry double ingredients temporarily", 30f, 10f, false, 2000, basePath);

            count += CreateCharacter("Chef Swift", 3, "Master of quick interactions", 0.9f, 1.6f, 0.8f,
                AbilityType.InstantChop, "Instantly chop ingredients", 25f, 0f, false, 1800, basePath);

            count += CreateCharacter("Chef Balanced", 4, "Well-rounded all-purpose chef", 1.0f, 1.0f, 1.0f,
                AbilityType.PreventBurning, "Prevent dishes from burning", 35f, 15f, true, 0, basePath);

            Debug.Log($"Generated {count} default characters");
            return count;
        }

        private int CreateCharacter(string name, int id, string description, float moveSpeed, float interactSpeed,
            float capacity, AbilityType abilityType, string abilityDesc, float cooldown, float duration,
            bool unlocked, int unlockCost, string basePath)
        {
            // Create character folder
            string characterFolderName = name.Replace(" ", "");
            string characterPath = $"{basePath}/{characterFolderName}";
            EnsureDirectory(characterPath);

            // Check if character already exists
            string characterAssetPath = $"{characterPath}/{characterFolderName}.asset";
            if (AssetDatabase.LoadAssetAtPath<CharacterClass>(characterAssetPath) != null)
            {
                Debug.LogWarning($"Character already exists: {characterAssetPath}");
                return 0;
            }

            // Create Stats
            CharacterStats stats = ScriptableObject.CreateInstance<CharacterStats>();
            stats.name = $"{characterFolderName}Stats";
            string statsPath = $"{characterPath}/{stats.name}.asset";

            AssetDatabase.CreateAsset(stats, statsPath);
            SerializedObject statsObj = new SerializedObject(stats);
            statsObj.FindProperty("_movementSpeedModifier").floatValue = moveSpeed;
            statsObj.FindProperty("_interactionSpeedModifier").floatValue = interactSpeed;
            statsObj.FindProperty("_carryingCapacityModifier").floatValue = capacity;
            statsObj.ApplyModifiedProperties();

            // Create Ability
            CharacterAbilityData abilityData = ScriptableObject.CreateInstance<CharacterAbilityData>();
            abilityData.name = $"{characterFolderName}Ability";
            string abilityPath = $"{characterPath}/{abilityData.name}.asset";

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
            unlockData.name = $"{characterFolderName}Unlock";
            string unlockPath = $"{characterPath}/{unlockData.name}.asset";

            AssetDatabase.CreateAsset(unlockData, unlockPath);
            SerializedObject unlockObj = new SerializedObject(unlockData);
            unlockObj.FindProperty("_unlockedByDefault").boolValue = unlocked;
            unlockObj.FindProperty("_unlockCost").intValue = unlockCost;
            unlockObj.FindProperty("_unlockLevel").intValue = 1;
            unlockObj.ApplyModifiedProperties();

            // Create Character
            CharacterClass character = ScriptableObject.CreateInstance<CharacterClass>();
            character.name = characterFolderName;

            AssetDatabase.CreateAsset(character, characterAssetPath);
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
            string basePath = "Assets/Resources/ScriptableObjects/GameModes";
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
