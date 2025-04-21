using System.Collections.Generic;
using Gameplay.Cooking;
using UnityEditor;
using UnityEngine;

namespace RecipeRage.Editor
{
    /// <summary>
    /// Generates ingredient scriptable objects for the RecipeRage game.
    /// </summary>
    public class IngredientGenerator
    {
        // List of ingredient definitions
        private readonly List<IngredientDefinition> _ingredientDefinitions = new List<IngredientDefinition>
        {
            new IngredientDefinition
            {
                Name = "Tomato",
                Color = new Color(0.9f, 0.2f, 0.2f),
                RequiresCutting = true,
                RequiresCooking = false,
                PrepTime = 1.5f
            },
            new IngredientDefinition
            {
                Name = "Lettuce",
                Color = new Color(0.2f, 0.8f, 0.2f),
                RequiresCutting = true,
                RequiresCooking = false,
                PrepTime = 1.0f
            },
            new IngredientDefinition
            {
                Name = "Onion",
                Color = new Color(0.9f, 0.9f, 0.8f),
                RequiresCutting = true,
                RequiresCooking = true,
                PrepTime = 2.0f
            },
            new IngredientDefinition
            {
                Name = "Meat",
                Color = new Color(0.6f, 0.4f, 0.4f),
                RequiresCutting = false,
                RequiresCooking = true,
                PrepTime = 3.0f
            },
            new IngredientDefinition
            {
                Name = "Cheese",
                Color = new Color(0.9f, 0.9f, 0.2f),
                RequiresCutting = true,
                RequiresCooking = false,
                PrepTime = 1.0f
            },
            new IngredientDefinition
            {
                Name = "Potato",
                Color = new Color(0.8f, 0.7f, 0.5f),
                RequiresCutting = true,
                RequiresCooking = true,
                PrepTime = 2.5f
            },
            new IngredientDefinition
            {
                Name = "Rice",
                Color = new Color(1.0f, 1.0f, 0.9f),
                RequiresCutting = false,
                RequiresCooking = true,
                PrepTime = 2.0f
            },
            new IngredientDefinition
            {
                Name = "Egg",
                Color = new Color(1.0f, 0.95f, 0.8f),
                RequiresCutting = false,
                RequiresCooking = true,
                PrepTime = 1.5f
            }
        };
        
        /// <summary>
        /// Generate ingredient scriptable objects.
        /// </summary>
        /// <param name="outputPath">The output path for the generated assets.</param>
        public void GenerateIngredients(string outputPath)
        {
            // Create a simple cube prefab for ingredients if it doesn't exist
            string ingredientPrefabPath = "Assets/Prefabs/Ingredients/IngredientBase.prefab";
            GameObject ingredientPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ingredientPrefabPath);
            
            if (ingredientPrefab == null)
            {
                // Create a new cube for the ingredient base
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = "IngredientBase";
                
                // Add required components
                if (!cube.GetComponent<IngredientItem>())
                {
                    cube.AddComponent<IngredientItem>();
                }
                
                if (!cube.GetComponent<Unity.Netcode.NetworkObject>())
                {
                    cube.AddComponent<Unity.Netcode.NetworkObject>();
                }
                
                // Create the prefab
                if (!System.IO.Directory.Exists("Assets/Prefabs/Ingredients"))
                {
                    System.IO.Directory.CreateDirectory("Assets/Prefabs/Ingredients");
                }
                
                ingredientPrefab = PrefabUtility.SaveAsPrefabAsset(cube, ingredientPrefabPath);
                GameObject.DestroyImmediate(cube);
            }
            
            // Generate each ingredient
            foreach (IngredientDefinition definition in _ingredientDefinitions)
            {
                // Create the ingredient scriptable object
                Ingredient ingredient = ScriptableObject.CreateInstance<Ingredient>();
                
                // Set the properties
                SerializedObject serializedObject = new SerializedObject(ingredient);
                serializedObject.FindProperty("_displayName").stringValue = definition.Name;
                serializedObject.FindProperty("_color").colorValue = definition.Color;
                serializedObject.FindProperty("_requiresCutting").boolValue = definition.RequiresCutting;
                serializedObject.FindProperty("_requiresCooking").boolValue = definition.RequiresCooking;
                serializedObject.FindProperty("_prepTime").floatValue = definition.PrepTime;
                serializedObject.FindProperty("_prefab").objectReferenceValue = ingredientPrefab;
                serializedObject.FindProperty("_id").intValue = definition.Name.GetHashCode();
                
                // Create a simple sprite for the ingredient
                Texture2D texture = new Texture2D(128, 128);
                Color[] colors = new Color[128 * 128];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = definition.Color;
                }
                texture.SetPixels(colors);
                texture.Apply();
                
                // Create a sprite from the texture
                string spritePath = $"{outputPath}/{definition.Name}_Icon.png";
                System.IO.File.WriteAllBytes(spritePath, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(spritePath);
                
                // Set the sprite import settings
                TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.SaveAndReimport();
                }
                
                // Assign the sprite to the ingredient
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                serializedObject.FindProperty("_icon").objectReferenceValue = sprite;
                
                // Apply the changes
                serializedObject.ApplyModifiedProperties();
                
                // Save the ingredient asset
                string assetPath = $"{outputPath}/{definition.Name}.asset";
                AssetDatabase.CreateAsset(ingredient, assetPath);
                
                Debug.Log($"Created ingredient: {definition.Name}");
            }
            
            // Save the changes
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Generated {_ingredientDefinitions.Count} ingredients.");
        }
        
        /// <summary>
        /// Definition for an ingredient.
        /// </summary>
        private class IngredientDefinition
        {
            public string Name { get; set; }
            public Color Color { get; set; }
            public bool RequiresCutting { get; set; }
            public bool RequiresCooking { get; set; }
            public float PrepTime { get; set; }
        }
    }
}
