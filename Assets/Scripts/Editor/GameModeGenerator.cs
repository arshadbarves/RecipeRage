using UnityEngine;
using UnityEditor;
using Gameplay.GameModes;
using System.IO;

namespace RecipeRage.Editor
{
    /// <summary>
    /// Editor window for generating GameMode ScriptableObjects
    /// </summary>
    public class GameModeGenerator : EditorWindow
    {
        private string gameModeId = "classic";
        private string gameModeName = "Classic Mode";
        private string gameModeDescription = "Traditional cooking competition!";
        private string subtitle = "Cook, Serve, Win!";
        private Sprite icon;
        private GameModeCategory category = GameModeCategory.Trophies;

        // Team Settings
        private int teamCount = 2;
        private int playersPerTeam = 2;
        private bool friendlyFire = false;

        // Game Settings
        private float gameTime = 300f;
        private int targetScore = 1000;
        private bool hasTimeLimit = true;
        private bool hasScoreLimit = true;

        // Map Settings
        private string mapSceneName = "KitchenArena";

        // Rewards
        private int experienceReward = 100;
        private int winBonusCoins = 25;

        // Special Features
        private bool powerUpsEnabled = true;
        private bool specialAbilitiesEnabled = true;
        private string customParameters = "";

        private Vector2 scrollPosition;

        [MenuItem("RecipeRage/Generators/Game Mode Generator")]
        public static void ShowWindow()
        {
            GetWindow<GameModeGenerator>("Game Mode Generator");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Game Mode Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Basic Info
            EditorGUILayout.LabelField("Basic Information", EditorStyles.boldLabel);
            gameModeId = EditorGUILayout.TextField("Game Mode ID", gameModeId);
            gameModeName = EditorGUILayout.TextField("Display Name", gameModeName);
            subtitle = EditorGUILayout.TextField("Subtitle", subtitle);
            gameModeDescription = EditorGUILayout.TextField("Description", gameModeDescription, GUILayout.Height(60));
            icon = (Sprite)EditorGUILayout.ObjectField("Icon", icon, typeof(Sprite), false);
            category = (GameModeCategory)EditorGUILayout.EnumPopup("Category", category);

            EditorGUILayout.Space();

            // Team Settings
            EditorGUILayout.LabelField("Team Settings", EditorStyles.boldLabel);
            teamCount = EditorGUILayout.IntSlider("Number of Teams", teamCount, 1, 8);
            playersPerTeam = EditorGUILayout.IntSlider("Players Per Team", playersPerTeam, 1, 4);
            friendlyFire = EditorGUILayout.Toggle("Friendly Fire", friendlyFire);

            int maxPlayers = teamCount * playersPerTeam;
            EditorGUILayout.HelpBox($"Max Players: {maxPlayers}", MessageType.Info);

            EditorGUILayout.Space();

            // Game Settings
            EditorGUILayout.LabelField("Game Settings", EditorStyles.boldLabel);
            hasTimeLimit = EditorGUILayout.Toggle("Has Time Limit", hasTimeLimit);
            if (hasTimeLimit)
            {
                gameTime = EditorGUILayout.Slider("Game Time (seconds)", gameTime, 60f, 900f);
            }

            hasScoreLimit = EditorGUILayout.Toggle("Has Score Limit", hasScoreLimit);
            if (hasScoreLimit)
            {
                targetScore = EditorGUILayout.IntField("Target Score", targetScore);
            }

            EditorGUILayout.Space();

            // Map Settings
            EditorGUILayout.LabelField("Map Settings", EditorStyles.boldLabel);
            mapSceneName = EditorGUILayout.TextField("Map Scene Name", mapSceneName);

            EditorGUILayout.Space();

            // Rewards
            EditorGUILayout.LabelField("Rewards", EditorStyles.boldLabel);
            experienceReward = EditorGUILayout.IntField("Experience Reward", experienceReward);
            winBonusCoins = EditorGUILayout.IntField("Win Bonus Coins", winBonusCoins);

            EditorGUILayout.Space();

            // Special Features
            EditorGUILayout.LabelField("Special Features", EditorStyles.boldLabel);
            powerUpsEnabled = EditorGUILayout.Toggle("Power-Ups Enabled", powerUpsEnabled);
            specialAbilitiesEnabled = EditorGUILayout.Toggle("Special Abilities Enabled", specialAbilitiesEnabled);
            customParameters = EditorGUILayout.TextField("Custom Parameters (JSON)", customParameters, GUILayout.Height(40));

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // Generate Button
            if (GUILayout.Button("Generate Game Mode", GUILayout.Height(40)))
            {
                GenerateGameMode();
            }

            // Quick Generate Common Modes
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Generate Presets", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Classic"))
            {
                SetClassicDefaults();
            }

            if (GUILayout.Button("Team Battle"))
            {
                SetTeamBattleDefaults();
            }

            if (GUILayout.Button("Free For All"))
            {
                SetFreeForAllDefaults();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Time Attack"))
            {
                SetTimeAttackDefaults();
            }

            if (GUILayout.Button("Survival"))
            {
                SetSurvivalDefaults();
            }

            if (GUILayout.Button("Ranked"))
            {
                SetRankedDefaults();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
        }

        private void SetClassicDefaults()
        {
            gameModeId = "classic";
            gameModeName = "Classic Mode";
            subtitle = "Traditional Battle";
            gameModeDescription = "Classic cooking competition with teams";
            category = GameModeCategory.Trophies;
            teamCount = 2;
            playersPerTeam = 2;
            friendlyFire = false;
            gameTime = 300f;
            targetScore = 1000;
            hasTimeLimit = true;
            hasScoreLimit = true;
            mapSceneName = "KitchenArena";
            experienceReward = 100;
            winBonusCoins = 25;
            powerUpsEnabled = true;
            specialAbilitiesEnabled = true;
        }

        private void SetTeamBattleDefaults()
        {
            gameModeId = "team_battle";
            gameModeName = "Team Battle";
            subtitle = "Coordinate & Conquer";
            gameModeDescription = "Work together to outscore the opposing team";
            category = GameModeCategory.Trophies;
            teamCount = 2;
            playersPerTeam = 3;
            friendlyFire = false;
            gameTime = 360f;
            targetScore = 1500;
            hasTimeLimit = true;
            hasScoreLimit = true;
            mapSceneName = "TeamArena";
            experienceReward = 150;
            winBonusCoins = 50;
            powerUpsEnabled = true;
            specialAbilitiesEnabled = true;
        }

        private void SetFreeForAllDefaults()
        {
            gameModeId = "free_for_all";
            gameModeName = "Free For All";
            subtitle = "Every Chef For Themselves";
            gameModeDescription = "No teams - only one winner!";
            category = GameModeCategory.Special;
            teamCount = 4;
            playersPerTeam = 1;
            friendlyFire = false;
            gameTime = 240f;
            targetScore = 800;
            hasTimeLimit = true;
            hasScoreLimit = true;
            mapSceneName = "FFAKitchen";
            experienceReward = 120;
            winBonusCoins = 30;
            powerUpsEnabled = true;
            specialAbilitiesEnabled = true;
        }

        private void SetTimeAttackDefaults()
        {
            gameModeId = "time_attack";
            gameModeName = "Time Attack";
            subtitle = "Race Against The Clock";
            gameModeDescription = "Score as many points as possible in limited time!";
            category = GameModeCategory.Special;
            teamCount = 2;
            playersPerTeam = 2;
            friendlyFire = false;
            gameTime = 180f;
            targetScore = 0;
            hasTimeLimit = true;
            hasScoreLimit = false;
            mapSceneName = "SpeedKitchen";
            experienceReward = 80;
            winBonusCoins = 20;
            powerUpsEnabled = true;
            specialAbilitiesEnabled = false;
        }

        private void SetSurvivalDefaults()
        {
            gameModeId = "survival";
            gameModeName = "Survival";
            subtitle = "Last Chef Standing";
            gameModeDescription = "Survive increasing difficulty waves!";
            category = GameModeCategory.Special;
            teamCount = 1;
            playersPerTeam = 4;
            friendlyFire = false;
            gameTime = 600f;
            targetScore = 0;
            hasTimeLimit = false;
            hasScoreLimit = false;
            mapSceneName = "SurvivalKitchen";
            experienceReward = 200;
            winBonusCoins = 75;
            powerUpsEnabled = true;
            specialAbilitiesEnabled = true;
        }

        private void SetRankedDefaults()
        {
            gameModeId = "ranked";
            gameModeName = "Ranked Match";
            subtitle = "Competitive Play";
            gameModeDescription = "Competitive ranked mode with trophy rewards";
            category = GameModeCategory.Ranked;
            teamCount = 2;
            playersPerTeam = 2;
            friendlyFire = false;
            gameTime = 300f;
            targetScore = 1200;
            hasTimeLimit = true;
            hasScoreLimit = true;
            mapSceneName = "RankedArena";
            experienceReward = 150;
            winBonusCoins = 50;
            powerUpsEnabled = false;
            specialAbilitiesEnabled = true;
        }

        private void GenerateGameMode()
        {
            // Validate
            if (string.IsNullOrEmpty(gameModeId))
            {
                EditorUtility.DisplayDialog("Error", "Game Mode ID cannot be empty!", "OK");
                return;
            }

            if (string.IsNullOrEmpty(gameModeName))
            {
                EditorUtility.DisplayDialog("Error", "Game Mode Name cannot be empty!", "OK");
                return;
            }

            // Create folder structure
            string basePath = "Assets/Resources/GameModes";
            if (!AssetDatabase.IsValidFolder(basePath))
            {
                Directory.CreateDirectory(Application.dataPath + "/Resources/GameModes");
                AssetDatabase.Refresh();
            }

            // Create Game Mode
            GameMode gameMode = ScriptableObject.CreateInstance<GameMode>();
            gameMode.name = gameModeName.Replace(" ", "");
            string path = $"{basePath}/{gameMode.name}.asset";

            // Set properties via SerializedObject
            AssetDatabase.CreateAsset(gameMode, path);
            SerializedObject gameModeObj = new SerializedObject(gameMode);

            gameModeObj.FindProperty("_id").stringValue = gameModeId;
            gameModeObj.FindProperty("_displayName").stringValue = gameModeName;
            gameModeObj.FindProperty("_description").stringValue = gameModeDescription;
            gameModeObj.FindProperty("_icon").objectReferenceValue = icon;
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
            gameModeObj.FindProperty("_experienceReward").intValue = experienceReward;
            gameModeObj.FindProperty("_winBonusCoins").intValue = winBonusCoins;
            gameModeObj.FindProperty("_powerUpsEnabled").boolValue = powerUpsEnabled;
            gameModeObj.FindProperty("_specialAbilitiesEnabled").boolValue = specialAbilitiesEnabled;
            gameModeObj.FindProperty("_customParameters").stringValue = customParameters;

            gameModeObj.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorGUIUtility.PingObject(gameMode);
            Debug.Log($"Generated Game Mode: {gameModeName} at {path}");
        }
    }
}
