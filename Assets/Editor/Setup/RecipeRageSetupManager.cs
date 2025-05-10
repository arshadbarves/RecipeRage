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
        private GameModeGenerator _gameModeGenerator;
        private CharacterClassGenerator _characterClassGenerator;
        private StationGenerator _stationGenerator;

        // Setup options
        private bool _generateIngredients = true;
        private bool _generateRecipes = true;
        private bool _generatePrefabs = true;
        private bool _generateGameModes = true;
        private bool _generateCharacterClasses = true;
        private bool _generateStations = true;
        private bool _setupScene = true;

        // Paths
        private const string INGREDIENTS_PATH = "Assets/ScriptableObjects/Ingredients";
        private const string RECIPES_PATH = "Assets/ScriptableObjects/Recipes";
        private const string GAME_MODES_PATH = "Assets/ScriptableObjects/GameModes";
        private const string CHARACTER_CLASSES_PATH = "Assets/ScriptableObjects/CharacterClasses";
        private const string PREFABS_PATH = "Assets/Prefabs";
        private const string STATIONS_PATH = "Assets/Prefabs/Stations";
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
            _gameModeGenerator = new GameModeGenerator();
            _characterClassGenerator = new CharacterClassGenerator();
            _stationGenerator = new StationGenerator();
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
            _generateGameModes = EditorGUILayout.Toggle("Generate Game Modes", _generateGameModes);
            _generateCharacterClasses = EditorGUILayout.Toggle("Generate Character Classes", _generateCharacterClasses);
            _generateStations = EditorGUILayout.Toggle("Generate Stations", _generateStations);
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

            if (GUILayout.Button("Generate Game Modes"))
            {
                GenerateGameModes();
            }

            if (GUILayout.Button("Generate Character Classes"))
            {
                GenerateCharacterClasses();
            }

            if (GUILayout.Button("Generate Stations"))
            {
                GenerateStations();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

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
            // Use the centralized PrefabCreationManager for one-click setup
            RecipeRage.Editor.Prefabs.PrefabCreationManager.CreateAllGameAssets();

            Debug.Log("RecipeRage setup completed successfully!");
        }

        /// <summary>
        /// Create necessary directories for assets.
        /// </summary>
        private void CreateDirectories()
        {
            CreateDirectoryIfNotExists(INGREDIENTS_PATH);
            CreateDirectoryIfNotExists(RECIPES_PATH);
            CreateDirectoryIfNotExists(GAME_MODES_PATH);
            CreateDirectoryIfNotExists(CHARACTER_CLASSES_PATH);
            CreateDirectoryIfNotExists(PREFABS_PATH);
            CreateDirectoryIfNotExists(STATIONS_PATH);
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
            // Use the centralized PrefabCreationManager
            Debug.Log("Using centralized PrefabCreationManager for ingredient generation...");
            RecipeRage.Editor.Prefabs.PrefabCreationManager.CreateAllScriptableObjects();
        }

        /// <summary>
        /// Generate recipe scriptable objects.
        /// </summary>
        private void GenerateRecipes()
        {
            // Use the centralized PrefabCreationManager
            Debug.Log("Using centralized PrefabCreationManager for recipe generation...");
            RecipeRage.Editor.Prefabs.PrefabCreationManager.CreateAllScriptableObjects();
        }

        /// <summary>
        /// Generate prefabs for the game.
        /// </summary>
        private void GeneratePrefabs()
        {
            // Use the centralized PrefabCreationManager
            Debug.Log("Using centralized PrefabCreationManager for prefab generation...");
            RecipeRage.Editor.Prefabs.PrefabCreationManager.CreateAllManagerPrefabs();
            RecipeRage.Editor.Prefabs.PrefabCreationManager.CreateAllUIPrefabs();
            RecipeRage.Editor.Prefabs.PrefabCreationManager.CreatePlayerPrefab();
        }

        /// <summary>
        /// Generate game mode scriptable objects.
        /// </summary>
        private void GenerateGameModes()
        {
            // Use the centralized PrefabCreationManager
            Debug.Log("Using centralized PrefabCreationManager for game mode generation...");
            RecipeRage.Editor.Prefabs.PrefabCreationManager.CreateAllScriptableObjects();
        }

        /// <summary>
        /// Generate character class scriptable objects.
        /// </summary>
        private void GenerateCharacterClasses()
        {
            // Use the centralized PrefabCreationManager
            Debug.Log("Using centralized PrefabCreationManager for character class generation...");
            RecipeRage.Editor.Prefabs.PrefabCreationManager.CreateAllScriptableObjects();
        }

        /// <summary>
        /// Generate cooking station prefabs.
        /// </summary>
        private void GenerateStations()
        {
            // Use the centralized PrefabCreationManager
            Debug.Log("Using centralized PrefabCreationManager for station generation...");
            RecipeRage.Editor.Prefabs.PrefabCreationManager.CreateAllStationPrefabs();
        }

        /// <summary>
        /// Setup the game scene.
        /// </summary>
        private void SetupScene()
        {
            // Use the centralized PrefabCreationManager
            Debug.Log("Using centralized PrefabCreationManager for scene setup...");
            RecipeRage.Editor.Prefabs.PrefabCreationManager.SetupAllScenes();
        }
    }
}
