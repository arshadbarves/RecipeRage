using System.Collections.Generic;
using Core.Characters;
using UnityEditor;
using UnityEngine;

namespace RecipeRage.Editor
{
    /// <summary>
    /// Generates character class scriptable objects for the RecipeRage game.
    /// </summary>
    public class CharacterClassGenerator
    {
        // List of character class definitions
        private readonly List<CharacterClassDefinition> _characterClassDefinitions = new List<CharacterClassDefinition>
        {
            // Chef - Balanced character with instant cook ability
            new CharacterClassDefinition
            {
                Id = 1,
                DisplayName = "Chef",
                Description = "A balanced chef with the ability to instantly cook ingredients.",
                MovementSpeedModifier = 1.0f,
                InteractionSpeedModifier = 1.0f,
                CarryingCapacityModifier = 1.0f,
                PrimaryAbilityType = AbilityType.InstantCook,
                PrimaryAbilityCooldown = 30f,
                PrimaryAbilityDuration = 0f,
                PrimaryAbilityDescription = "Instantly cook any ingredient you're holding.",
                PrimaryAbilityParameters = "",
                UnlockedByDefault = true,
                UnlockCost = 0,
                UnlockLevel = 1
            },
            
            // Speedster - Fast movement with speed boost ability
            new CharacterClassDefinition
            {
                Id = 2,
                DisplayName = "Speedster",
                Description = "A fast chef who can temporarily boost their movement speed.",
                MovementSpeedModifier = 1.2f,
                InteractionSpeedModifier = 0.9f,
                CarryingCapacityModifier = 1.0f,
                PrimaryAbilityType = AbilityType.SpeedBoost,
                PrimaryAbilityCooldown = 20f,
                PrimaryAbilityDuration = 5f,
                PrimaryAbilityDescription = "Temporarily boost your movement speed by 50%.",
                PrimaryAbilityParameters = "{\"SpeedMultiplier\": 1.5}",
                UnlockedByDefault = false,
                UnlockCost = 1000,
                UnlockLevel = 5
            },
            
            // Butcher - Fast chopping with instant chop ability
            new CharacterClassDefinition
            {
                Id = 3,
                DisplayName = "Butcher",
                Description = "A strong chef who can instantly chop ingredients.",
                MovementSpeedModifier = 0.9f,
                InteractionSpeedModifier = 1.2f,
                CarryingCapacityModifier = 1.0f,
                PrimaryAbilityType = AbilityType.InstantChop,
                PrimaryAbilityCooldown = 25f,
                PrimaryAbilityDuration = 0f,
                PrimaryAbilityDescription = "Instantly chop any ingredient you're holding.",
                PrimaryAbilityParameters = "",
                UnlockedByDefault = false,
                UnlockCost = 1000,
                UnlockLevel = 5
            },
            
            // Carrier - High carrying capacity with ingredient magnet ability
            new CharacterClassDefinition
            {
                Id = 4,
                DisplayName = "Carrier",
                Description = "A strong chef who can carry multiple ingredients and attract ingredients to them.",
                MovementSpeedModifier = 0.8f,
                InteractionSpeedModifier = 1.0f,
                CarryingCapacityModifier = 2.0f,
                PrimaryAbilityType = AbilityType.IngredientMagnet,
                PrimaryAbilityCooldown = 30f,
                PrimaryAbilityDuration = 5f,
                PrimaryAbilityDescription = "Attract nearby ingredients to you.",
                PrimaryAbilityParameters = "{\"MagnetRadius\": 5.0, \"MagnetForce\": 10.0}",
                UnlockedByDefault = false,
                UnlockCost = 2000,
                UnlockLevel = 10
            },
            
            // Timekeeper - Can freeze time temporarily
            new CharacterClassDefinition
            {
                Id = 5,
                DisplayName = "Timekeeper",
                Description = "A mystical chef who can temporarily freeze time for everyone else.",
                MovementSpeedModifier = 1.0f,
                InteractionSpeedModifier = 1.0f,
                CarryingCapacityModifier = 1.0f,
                PrimaryAbilityType = AbilityType.FreezeTime,
                PrimaryAbilityCooldown = 60f,
                PrimaryAbilityDuration = 3f,
                PrimaryAbilityDescription = "Temporarily freeze time for everyone else.",
                PrimaryAbilityParameters = "",
                UnlockedByDefault = false,
                UnlockCost = 3000,
                UnlockLevel = 15
            },
            
            // Trickster - Can push other players and steal ingredients
            new CharacterClassDefinition
            {
                Id = 6,
                DisplayName = "Trickster",
                Description = "A mischievous chef who can push other players and steal their ingredients.",
                MovementSpeedModifier = 1.1f,
                InteractionSpeedModifier = 1.0f,
                CarryingCapacityModifier = 1.0f,
                PrimaryAbilityType = AbilityType.PushOtherPlayers,
                PrimaryAbilityCooldown = 15f,
                PrimaryAbilityDuration = 0f,
                PrimaryAbilityDescription = "Push other players away from you.",
                PrimaryAbilityParameters = "{\"PushForce\": 5.0, \"PushRadius\": 3.0}",
                UnlockedByDefault = false,
                UnlockCost = 2000,
                UnlockLevel = 10
            }
        };
        
        /// <summary>
        /// Generate character class scriptable objects.
        /// </summary>
        /// <param name="outputPath">The output path for the generated assets.</param>
        public void GenerateCharacterClasses(string outputPath)
        {
            // Generate each character class
            foreach (CharacterClassDefinition definition in _characterClassDefinitions)
            {
                // Create the character class scriptable object
                CharacterClass characterClass = ScriptableObject.CreateInstance<CharacterClass>();
                
                // Set the properties
                SerializedObject serializedObject = new SerializedObject(characterClass);
                serializedObject.FindProperty("_id").intValue = definition.Id;
                serializedObject.FindProperty("_displayName").stringValue = definition.DisplayName;
                serializedObject.FindProperty("_description").stringValue = definition.Description;
                
                // Create a simple icon for the character class
                Texture2D texture = new Texture2D(128, 128);
                Color[] colors = new Color[128 * 128];
                
                // Use different colors for different character classes
                Color iconColor = Color.white;
                switch (definition.DisplayName)
                {
                    case "Chef":
                        iconColor = new Color(1.0f, 0.8f, 0.2f); // Gold
                        break;
                    case "Speedster":
                        iconColor = new Color(0.2f, 0.8f, 1.0f); // Light Blue
                        break;
                    case "Butcher":
                        iconColor = new Color(0.8f, 0.2f, 0.2f); // Red
                        break;
                    case "Carrier":
                        iconColor = new Color(0.6f, 0.4f, 0.2f); // Brown
                        break;
                    case "Timekeeper":
                        iconColor = new Color(0.5f, 0.2f, 0.8f); // Purple
                        break;
                    case "Trickster":
                        iconColor = new Color(0.2f, 0.8f, 0.4f); // Green
                        break;
                }
                
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = iconColor;
                }
                texture.SetPixels(colors);
                texture.Apply();
                
                // Create a sprite from the texture
                string spritePath = $"{outputPath}/{definition.DisplayName}_Icon.png";
                System.IO.File.WriteAllBytes(spritePath, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(spritePath);
                
                // Set the sprite import settings
                TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.SaveAndReimport();
                }
                
                // Assign the sprite to the character class
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                serializedObject.FindProperty("_icon").objectReferenceValue = sprite;
                
                // Set stat modifiers
                serializedObject.FindProperty("_movementSpeedModifier").floatValue = definition.MovementSpeedModifier;
                serializedObject.FindProperty("_interactionSpeedModifier").floatValue = definition.InteractionSpeedModifier;
                serializedObject.FindProperty("_carryingCapacityModifier").floatValue = definition.CarryingCapacityModifier;
                
                // Set ability settings
                serializedObject.FindProperty("_primaryAbilityType").enumValueIndex = (int)definition.PrimaryAbilityType;
                serializedObject.FindProperty("_primaryAbilityCooldown").floatValue = definition.PrimaryAbilityCooldown;
                serializedObject.FindProperty("_primaryAbilityDuration").floatValue = definition.PrimaryAbilityDuration;
                serializedObject.FindProperty("_primaryAbilityDescription").stringValue = definition.PrimaryAbilityDescription;
                serializedObject.FindProperty("_primaryAbilityParameters").stringValue = definition.PrimaryAbilityParameters;
                
                // Set unlock settings
                serializedObject.FindProperty("_unlockedByDefault").boolValue = definition.UnlockedByDefault;
                serializedObject.FindProperty("_unlockCost").intValue = definition.UnlockCost;
                serializedObject.FindProperty("_unlockLevel").intValue = definition.UnlockLevel;
                
                // Apply the changes
                serializedObject.ApplyModifiedProperties();
                
                // Save the character class asset
                string assetPath = $"{outputPath}/{definition.DisplayName}.asset";
                AssetDatabase.CreateAsset(characterClass, assetPath);
                
                Debug.Log($"Created character class: {definition.DisplayName}");
            }
            
            // Save the changes
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Generated {_characterClassDefinitions.Count} character classes.");
        }
        
        /// <summary>
        /// Definition for a character class.
        /// </summary>
        private class CharacterClassDefinition
        {
            public int Id { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public float MovementSpeedModifier { get; set; }
            public float InteractionSpeedModifier { get; set; }
            public float CarryingCapacityModifier { get; set; }
            public AbilityType PrimaryAbilityType { get; set; }
            public float PrimaryAbilityCooldown { get; set; }
            public float PrimaryAbilityDuration { get; set; }
            public string PrimaryAbilityDescription { get; set; }
            public string PrimaryAbilityParameters { get; set; }
            public bool UnlockedByDefault { get; set; }
            public int UnlockCost { get; set; }
            public int UnlockLevel { get; set; }
        }
    }
}
