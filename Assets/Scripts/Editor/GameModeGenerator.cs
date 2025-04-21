using System.Collections.Generic;
using Core.GameModes;
using UnityEditor;
using UnityEngine;

namespace RecipeRage.Editor
{
    /// <summary>
    /// Generates game mode scriptable objects for the RecipeRage game.
    /// </summary>
    public class GameModeGenerator
    {
        // List of game mode definitions
        private readonly List<GameModeDefinition> _gameModeDefinitions = new List<GameModeDefinition>
        {
            // Classic Mode
            new GameModeDefinition
            {
                Id = "classic",
                DisplayName = "Classic",
                Description = "Work together to complete as many orders as possible before time runs out. The team with the most points wins!",
                TeamCount = 2,
                PlayersPerTeam = 2,
                FriendlyFire = false,
                GameTime = 300f,
                TargetScore = 1000,
                HasTimeLimit = true,
                HasScoreLimit = true,
                AllowJoinInProgress = false,
                AvailableMaps = new List<string> { "Kitchen", "Restaurant", "Diner" },
                DefaultMap = "Kitchen",
                RecipeDifficultyMultiplier = 1.0f,
                OrderFrequency = 30f,
                MaxActiveOrders = 5,
                BaseCoinsReward = 50,
                WinBonusCoins = 25,
                CoinsPerOrder = 10,
                ExperienceReward = 100,
                PowerUpsEnabled = true,
                SpecialAbilitiesEnabled = true,
                RestrictCharacterClasses = false,
                AllowedCharacterClasses = new List<int>(),
                CustomParameters = ""
            },
            
            // Time Attack Mode
            new GameModeDefinition
            {
                Id = "timeattack",
                DisplayName = "Time Attack",
                Description = "Race against the clock! Complete orders as quickly as possible to add time to the clock. The game ends when time runs out.",
                TeamCount = 1,
                PlayersPerTeam = 4,
                FriendlyFire = false,
                GameTime = 180f,
                TargetScore = 0,
                HasTimeLimit = true,
                HasScoreLimit = false,
                AllowJoinInProgress = false,
                AvailableMaps = new List<string> { "Kitchen", "FastFood" },
                DefaultMap = "FastFood",
                RecipeDifficultyMultiplier = 1.2f,
                OrderFrequency = 20f,
                MaxActiveOrders = 3,
                BaseCoinsReward = 75,
                WinBonusCoins = 0,
                CoinsPerOrder = 15,
                ExperienceReward = 150,
                PowerUpsEnabled = true,
                SpecialAbilitiesEnabled = true,
                RestrictCharacterClasses = false,
                AllowedCharacterClasses = new List<int>(),
                CustomParameters = "{\"timePerOrder\": 15, \"bonusTimePerOrder\": 10}"
            },
            
            // Team Battle Mode
            new GameModeDefinition
            {
                Id = "teambattle",
                DisplayName = "Team Battle",
                Description = "Compete against the other team to complete orders and sabotage your opponents. The first team to reach the target score wins!",
                TeamCount = 2,
                PlayersPerTeam = 2,
                FriendlyFire = false,
                GameTime = 600f,
                TargetScore = 1500,
                HasTimeLimit = true,
                HasScoreLimit = true,
                AllowJoinInProgress = false,
                AvailableMaps = new List<string> { "BattleKitchen", "DualRestaurant" },
                DefaultMap = "BattleKitchen",
                RecipeDifficultyMultiplier = 1.5f,
                OrderFrequency = 25f,
                MaxActiveOrders = 4,
                BaseCoinsReward = 100,
                WinBonusCoins = 50,
                CoinsPerOrder = 20,
                ExperienceReward = 200,
                PowerUpsEnabled = true,
                SpecialAbilitiesEnabled = true,
                RestrictCharacterClasses = false,
                AllowedCharacterClasses = new List<int>(),
                CustomParameters = "{\"sabotageEnabled\": true, \"powerUpSpawnRate\": 45}"
            },
            
            // Practice Mode
            new GameModeDefinition
            {
                Id = "practice",
                DisplayName = "Practice",
                Description = "Practice your cooking skills without time pressure. Perfect for learning the game mechanics.",
                TeamCount = 1,
                PlayersPerTeam = 1,
                FriendlyFire = false,
                GameTime = 0f,
                TargetScore = 0,
                HasTimeLimit = false,
                HasScoreLimit = false,
                AllowJoinInProgress = true,
                AvailableMaps = new List<string> { "Kitchen", "Tutorial" },
                DefaultMap = "Tutorial",
                RecipeDifficultyMultiplier = 0.8f,
                OrderFrequency = 45f,
                MaxActiveOrders = 2,
                BaseCoinsReward = 25,
                WinBonusCoins = 0,
                CoinsPerOrder = 5,
                ExperienceReward = 50,
                PowerUpsEnabled = false,
                SpecialAbilitiesEnabled = true,
                RestrictCharacterClasses = false,
                AllowedCharacterClasses = new List<int>(),
                CustomParameters = "{\"tutorialEnabled\": true, \"infiniteIngredients\": true}"
            }
        };
        
        /// <summary>
        /// Generate game mode scriptable objects.
        /// </summary>
        /// <param name="outputPath">The output path for the generated assets.</param>
        public void GenerateGameModes(string outputPath)
        {
            // Generate each game mode
            foreach (GameModeDefinition definition in _gameModeDefinitions)
            {
                // Create the game mode scriptable object
                GameMode gameMode = ScriptableObject.CreateInstance<GameMode>();
                
                // Set the properties
                SerializedObject serializedObject = new SerializedObject(gameMode);
                serializedObject.FindProperty("_id").stringValue = definition.Id;
                serializedObject.FindProperty("_displayName").stringValue = definition.DisplayName;
                serializedObject.FindProperty("_description").stringValue = definition.Description;
                
                // Create a simple icon for the game mode
                Texture2D texture = new Texture2D(128, 128);
                Color[] colors = new Color[128 * 128];
                
                // Use different colors for different game modes
                Color iconColor = Color.white;
                switch (definition.Id)
                {
                    case "classic":
                        iconColor = new Color(0.2f, 0.6f, 1.0f); // Blue
                        break;
                    case "timeattack":
                        iconColor = new Color(1.0f, 0.6f, 0.2f); // Orange
                        break;
                    case "teambattle":
                        iconColor = new Color(1.0f, 0.2f, 0.2f); // Red
                        break;
                    case "practice":
                        iconColor = new Color(0.2f, 0.8f, 0.2f); // Green
                        break;
                }
                
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = iconColor;
                }
                texture.SetPixels(colors);
                texture.Apply();
                
                // Create a sprite from the texture
                string spritePath = $"{outputPath}/{definition.Id}_Icon.png";
                System.IO.File.WriteAllBytes(spritePath, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(spritePath);
                
                // Set the sprite import settings
                TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.SaveAndReimport();
                }
                
                // Assign the sprite to the game mode
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                serializedObject.FindProperty("_icon").objectReferenceValue = sprite;
                
                // Set team settings
                serializedObject.FindProperty("_teamCount").intValue = definition.TeamCount;
                serializedObject.FindProperty("_playersPerTeam").intValue = definition.PlayersPerTeam;
                serializedObject.FindProperty("_friendlyFire").boolValue = definition.FriendlyFire;
                
                // Set game settings
                serializedObject.FindProperty("_gameTime").floatValue = definition.GameTime;
                serializedObject.FindProperty("_targetScore").intValue = definition.TargetScore;
                serializedObject.FindProperty("_hasTimeLimit").boolValue = definition.HasTimeLimit;
                serializedObject.FindProperty("_hasScoreLimit").boolValue = definition.HasScoreLimit;
                serializedObject.FindProperty("_allowJoinInProgress").boolValue = definition.AllowJoinInProgress;
                
                // Set map settings
                SerializedProperty availableMapsProperty = serializedObject.FindProperty("_availableMaps");
                availableMapsProperty.ClearArray();
                for (int i = 0; i < definition.AvailableMaps.Count; i++)
                {
                    availableMapsProperty.arraySize++;
                    availableMapsProperty.GetArrayElementAtIndex(i).stringValue = definition.AvailableMaps[i];
                }
                serializedObject.FindProperty("_defaultMap").stringValue = definition.DefaultMap;
                
                // Set recipe settings
                serializedObject.FindProperty("_recipeDifficultyMultiplier").floatValue = definition.RecipeDifficultyMultiplier;
                serializedObject.FindProperty("_orderFrequency").floatValue = definition.OrderFrequency;
                serializedObject.FindProperty("_maxActiveOrders").intValue = definition.MaxActiveOrders;
                
                // Set reward settings
                serializedObject.FindProperty("_baseCoinsReward").intValue = definition.BaseCoinsReward;
                serializedObject.FindProperty("_winBonusCoins").intValue = definition.WinBonusCoins;
                serializedObject.FindProperty("_coinsPerOrder").intValue = definition.CoinsPerOrder;
                serializedObject.FindProperty("_experienceReward").intValue = definition.ExperienceReward;
                
                // Set special features
                serializedObject.FindProperty("_powerUpsEnabled").boolValue = definition.PowerUpsEnabled;
                serializedObject.FindProperty("_specialAbilitiesEnabled").boolValue = definition.SpecialAbilitiesEnabled;
                serializedObject.FindProperty("_restrictCharacterClasses").boolValue = definition.RestrictCharacterClasses;
                
                SerializedProperty allowedClassesProperty = serializedObject.FindProperty("_allowedCharacterClasses");
                allowedClassesProperty.ClearArray();
                for (int i = 0; i < definition.AllowedCharacterClasses.Count; i++)
                {
                    allowedClassesProperty.arraySize++;
                    allowedClassesProperty.GetArrayElementAtIndex(i).intValue = definition.AllowedCharacterClasses[i];
                }
                
                serializedObject.FindProperty("_customParameters").stringValue = definition.CustomParameters;
                
                // Apply the changes
                serializedObject.ApplyModifiedProperties();
                
                // Save the game mode asset
                string assetPath = $"{outputPath}/{definition.Id}Mode.asset";
                AssetDatabase.CreateAsset(gameMode, assetPath);
                
                Debug.Log($"Created game mode: {definition.DisplayName}");
            }
            
            // Save the changes
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Generated {_gameModeDefinitions.Count} game modes.");
        }
        
        /// <summary>
        /// Definition for a game mode.
        /// </summary>
        private class GameModeDefinition
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public int TeamCount { get; set; }
            public int PlayersPerTeam { get; set; }
            public bool FriendlyFire { get; set; }
            public float GameTime { get; set; }
            public int TargetScore { get; set; }
            public bool HasTimeLimit { get; set; }
            public bool HasScoreLimit { get; set; }
            public bool AllowJoinInProgress { get; set; }
            public List<string> AvailableMaps { get; set; }
            public string DefaultMap { get; set; }
            public float RecipeDifficultyMultiplier { get; set; }
            public float OrderFrequency { get; set; }
            public int MaxActiveOrders { get; set; }
            public int BaseCoinsReward { get; set; }
            public int WinBonusCoins { get; set; }
            public int CoinsPerOrder { get; set; }
            public int ExperienceReward { get; set; }
            public bool PowerUpsEnabled { get; set; }
            public bool SpecialAbilitiesEnabled { get; set; }
            public bool RestrictCharacterClasses { get; set; }
            public List<int> AllowedCharacterClasses { get; set; }
            public string CustomParameters { get; set; }
        }
    }
}
