using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RecipeRage.Editor
{
    /// <summary>
    /// Main setup manager for RecipeRage game.
    /// Coordinates the generation of prefabs, scriptable objects, and scene setup.
    /// </summary>
    public class RecipeRageSetupManager : EditorWindow
    {
        // References to generator scripts
        private IngredientGenerator _ingredientGenerator;
        private RecipeGenerator _recipeGenerator;
        private PrefabGenerator _prefabGenerator;
        private SceneSetupGenerator _sceneSetupGenerator;
        
        // Setup options
        private bool _generateIngredients = true;
        private bool _generateRecipes = true;
        private bool _generatePrefabs = true;
        private bool _setupScene = true;
        
        // Paths
        private const string INGREDIENTS_PATH = "Assets/ScriptableObjects/Ingredients";
        private const string RECIPES_PATH = "Assets/ScriptableObjects/Recipes";
        private const string PREFABS_PATH = "Assets/Prefabs";
        private const string SCENES_PATH = "Assets/Scenes";
        
        [MenuItem("RecipeRage/Setup Manager")]
        public static void ShowWindow()
        {
            GetWindow<RecipeRageSetupManager>("RecipeRage Setup");
        }
        
        private void OnEnable()
        {
            // Initialize generators
            _ingredientGenerator = new IngredientGenerator();
            _recipeGenerator = new RecipeGenerator();
            _prefabGenerator = new PrefabGenerator();
            _sceneSetupGenerator = new SceneSetupGenerator();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("RecipeRage Setup Manager", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.HelpBox("This tool will generate all necessary assets for the RecipeRage game. " +
                                   "You can choose which components to generate.", MessageType.Info);
            
            EditorGUILayout.Space();
            
            // Setup options
            _generateIngredients = EditorGUILayout.Toggle("Generate Ingredients", _generateIngredients);
            _generateRecipes = EditorGUILayout.Toggle("Generate Recipes", _generateRecipes);
            _generatePrefabs = EditorGUILayout.Toggle("Generate Prefabs", _generatePrefabs);
            _setupScene = EditorGUILayout.Toggle("Setup Scene", _setupScene);
            
            EditorGUILayout.Space();
            
            // Generate button
            if (GUILayout.Button("Generate All"))
            {
                GenerateAll();
            }
            
            EditorGUILayout.Space();
            
            // Individual generation buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Generate Ingredients"))
            {
                GenerateIngredients();
            }
            
            if (GUILayout.Button("Generate Recipes"))
            {
                GenerateRecipes();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Generate Prefabs"))
            {
                GeneratePrefabs();
            }
            
            if (GUILayout.Button("Setup Scene"))
            {
                SetupScene();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// Generate all selected components.
        /// </summary>
        private void GenerateAll()
        {
            // Create necessary directories
            CreateDirectories();
            
            // Generate in sequence
            if (_generateIngredients)
            {
                GenerateIngredients();
            }
            
            if (_generateRecipes)
            {
                GenerateRecipes();
            }
            
            if (_generatePrefabs)
            {
                GeneratePrefabs();
            }
            
            if (_setupScene)
            {
                SetupScene();
            }
            
            // Refresh the asset database
            AssetDatabase.Refresh();
            
            Debug.Log("RecipeRage setup completed successfully!");
        }
        
        /// <summary>
        /// Create necessary directories for assets.
        /// </summary>
        private void CreateDirectories()
        {
            CreateDirectoryIfNotExists(INGREDIENTS_PATH);
            CreateDirectoryIfNotExists(RECIPES_PATH);
            CreateDirectoryIfNotExists(PREFABS_PATH);
            CreateDirectoryIfNotExists(SCENES_PATH);
            
            // Create subdirectories for prefabs
            CreateDirectoryIfNotExists(Path.Combine(PREFABS_PATH, "Ingredients"));
            CreateDirectoryIfNotExists(Path.Combine(PREFABS_PATH, "Stations"));
            CreateDirectoryIfNotExists(Path.Combine(PREFABS_PATH, "UI"));
            CreateDirectoryIfNotExists(Path.Combine(PREFABS_PATH, "Player"));
        }
        
        /// <summary>
        /// Create a directory if it doesn't exist.
        /// </summary>
        /// <param name="path">The directory path.</param>
        private void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log($"Created directory: {path}");
            }
        }
        
        /// <summary>
        /// Generate ingredient scriptable objects.
        /// </summary>
        private void GenerateIngredients()
        {
            _ingredientGenerator.GenerateIngredients(INGREDIENTS_PATH);
        }
        
        /// <summary>
        /// Generate recipe scriptable objects.
        /// </summary>
        private void GenerateRecipes()
        {
            _recipeGenerator.GenerateRecipes(RECIPES_PATH, INGREDIENTS_PATH);
        }
        
        /// <summary>
        /// Generate prefabs for the game.
        /// </summary>
        private void GeneratePrefabs()
        {
            _prefabGenerator.GeneratePrefabs(PREFABS_PATH, INGREDIENTS_PATH);
        }
        
        /// <summary>
        /// Setup the game scene.
        /// </summary>
        private void SetupScene()
        {
            _sceneSetupGenerator.SetupScene(SCENES_PATH, PREFABS_PATH);
        }
    }
}
