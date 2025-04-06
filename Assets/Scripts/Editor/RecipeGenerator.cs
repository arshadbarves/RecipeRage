using System.Collections.Generic;
using System.IO;
using RecipeRage.Gameplay.Cooking;
using UnityEditor;
using UnityEngine;

namespace RecipeRage.Editor
{
    /// <summary>
    /// Generates recipe scriptable objects for the RecipeRage game.
    /// </summary>
    public class RecipeGenerator
    {
        // List of recipe definitions
        private readonly List<RecipeDefinition> _recipeDefinitions = new List<RecipeDefinition>
        {
            new RecipeDefinition
            {
                Name = "Tomato Salad",
                PointValue = 100,
                BaseTimeLimit = 30f,
                Difficulty = RecipeDifficulty.Easy,
                IngredientRequirements = new List<IngredientRequirement>
                {
                    new IngredientRequirement { IngredientName = "Tomato", RequireCut = true, RequireCooked = false },
                    new IngredientRequirement { IngredientName = "Lettuce", RequireCut = true, RequireCooked = false }
                }
            },
            new RecipeDefinition
            {
                Name = "Cheese Burger",
                PointValue = 200,
                BaseTimeLimit = 45f,
                Difficulty = RecipeDifficulty.Medium,
                IngredientRequirements = new List<IngredientRequirement>
                {
                    new IngredientRequirement { IngredientName = "Meat", RequireCut = false, RequireCooked = true },
                    new IngredientRequirement { IngredientName = "Cheese", RequireCut = true, RequireCooked = false },
                    new IngredientRequirement { IngredientName = "Lettuce", RequireCut = true, RequireCooked = false }
                }
            },
            new RecipeDefinition
            {
                Name = "Stir Fry",
                PointValue = 250,
                BaseTimeLimit = 60f,
                Difficulty = RecipeDifficulty.Medium,
                IngredientRequirements = new List<IngredientRequirement>
                {
                    new IngredientRequirement { IngredientName = "Rice", RequireCut = false, RequireCooked = true },
                    new IngredientRequirement { IngredientName = "Onion", RequireCut = true, RequireCooked = true },
                    new IngredientRequirement { IngredientName = "Meat", RequireCut = false, RequireCooked = true }
                }
            },
            new RecipeDefinition
            {
                Name = "Loaded Potato",
                PointValue = 150,
                BaseTimeLimit = 50f,
                Difficulty = RecipeDifficulty.Easy,
                IngredientRequirements = new List<IngredientRequirement>
                {
                    new IngredientRequirement { IngredientName = "Potato", RequireCut = false, RequireCooked = true },
                    new IngredientRequirement { IngredientName = "Cheese", RequireCut = true, RequireCooked = false }
                }
            },
            new RecipeDefinition
            {
                Name = "Deluxe Breakfast",
                PointValue = 300,
                BaseTimeLimit = 70f,
                Difficulty = RecipeDifficulty.Hard,
                IngredientRequirements = new List<IngredientRequirement>
                {
                    new IngredientRequirement { IngredientName = "Egg", RequireCut = false, RequireCooked = true },
                    new IngredientRequirement { IngredientName = "Potato", RequireCut = true, RequireCooked = true },
                    new IngredientRequirement { IngredientName = "Cheese", RequireCut = true, RequireCooked = false },
                    new IngredientRequirement { IngredientName = "Tomato", RequireCut = true, RequireCooked = false }
                }
            }
        };
        
        /// <summary>
        /// Generate recipe scriptable objects.
        /// </summary>
        /// <param name="outputPath">The output path for the generated assets.</param>
        /// <param name="ingredientsPath">The path to the ingredient assets.</param>
        public void GenerateRecipes(string outputPath, string ingredientsPath)
        {
            // Load all ingredients
            Dictionary<string, Ingredient> ingredients = LoadIngredients(ingredientsPath);
            
            if (ingredients.Count == 0)
            {
                Debug.LogError("No ingredients found. Please generate ingredients first.");
                return;
            }
            
            // Generate each recipe
            foreach (RecipeDefinition definition in _recipeDefinitions)
            {
                // Create the recipe scriptable object
                Recipe recipe = ScriptableObject.CreateInstance<Recipe>();
                
                // Set the properties
                SerializedObject serializedObject = new SerializedObject(recipe);
                serializedObject.FindProperty("_displayName").stringValue = definition.Name;
                serializedObject.FindProperty("_pointValue").intValue = definition.PointValue;
                serializedObject.FindProperty("_baseTimeLimit").floatValue = definition.BaseTimeLimit;
                serializedObject.FindProperty("_difficulty").enumValueIndex = (int)definition.Difficulty;
                serializedObject.FindProperty("_id").intValue = definition.Name.GetHashCode();
                
                // Create a simple sprite for the recipe
                Texture2D texture = new Texture2D(128, 128);
                Color[] colors = new Color[128 * 128];
                for (int i = 0; i < colors.Length; i++)
                {
                    // Create a gradient based on the recipe difficulty
                    float hue = ((int)definition.Difficulty) / 4.0f;
                    colors[i] = Color.HSVToRGB(hue, 0.7f, 0.9f);
                }
                texture.SetPixels(colors);
                texture.Apply();
                
                // Create a sprite from the texture
                string spritePath = $"{outputPath}/{definition.Name}_Icon.png";
                File.WriteAllBytes(spritePath, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(spritePath);
                
                // Set the sprite import settings
                TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.SaveAndReimport();
                }
                
                // Assign the sprite to the recipe
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                serializedObject.FindProperty("_icon").objectReferenceValue = sprite;
                
                // Set the ingredients
                SerializedProperty ingredientsProperty = serializedObject.FindProperty("_ingredients");
                ingredientsProperty.ClearArray();
                
                foreach (IngredientRequirement requirement in definition.IngredientRequirements)
                {
                    if (ingredients.TryGetValue(requirement.IngredientName, out Ingredient ingredient))
                    {
                        // Add the ingredient to the recipe
                        ingredientsProperty.arraySize++;
                        SerializedProperty elementProperty = ingredientsProperty.GetArrayElementAtIndex(ingredientsProperty.arraySize - 1);
                        
                        elementProperty.FindPropertyRelative("_ingredient").objectReferenceValue = ingredient;
                        elementProperty.FindPropertyRelative("_requireCut").boolValue = requirement.RequireCut;
                        elementProperty.FindPropertyRelative("_requireCooked").boolValue = requirement.RequireCooked;
                    }
                    else
                    {
                        Debug.LogWarning($"Ingredient '{requirement.IngredientName}' not found for recipe '{definition.Name}'.");
                    }
                }
                
                // Apply the changes
                serializedObject.ApplyModifiedProperties();
                
                // Save the recipe asset
                string assetPath = $"{outputPath}/{definition.Name}.asset";
                AssetDatabase.CreateAsset(recipe, assetPath);
                
                Debug.Log($"Created recipe: {definition.Name}");
            }
            
            // Save the changes
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Generated {_recipeDefinitions.Count} recipes.");
        }
        
        /// <summary>
        /// Load all ingredients from the specified path.
        /// </summary>
        /// <param name="ingredientsPath">The path to the ingredient assets.</param>
        /// <returns>A dictionary of ingredient name to ingredient.</returns>
        private Dictionary<string, Ingredient> LoadIngredients(string ingredientsPath)
        {
            Dictionary<string, Ingredient> ingredients = new Dictionary<string, Ingredient>();
            
            // Find all ingredient assets
            string[] guids = AssetDatabase.FindAssets("t:Ingredient", new[] { ingredientsPath });
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Ingredient ingredient = AssetDatabase.LoadAssetAtPath<Ingredient>(path);
                
                if (ingredient != null)
                {
                    ingredients[ingredient.DisplayName] = ingredient;
                }
            }
            
            return ingredients;
        }
        
        /// <summary>
        /// Definition for a recipe.
        /// </summary>
        private class RecipeDefinition
        {
            public string Name { get; set; }
            public int PointValue { get; set; }
            public float BaseTimeLimit { get; set; }
            public RecipeDifficulty Difficulty { get; set; }
            public List<IngredientRequirement> IngredientRequirements { get; set; }
        }
        
        /// <summary>
        /// Definition for an ingredient requirement in a recipe.
        /// </summary>
        private class IngredientRequirement
        {
            public string IngredientName { get; set; }
            public bool RequireCut { get; set; }
            public bool RequireCooked { get; set; }
        }
    }
}
