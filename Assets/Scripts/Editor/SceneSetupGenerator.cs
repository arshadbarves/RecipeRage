using System;
using System.Collections.Generic;
using RecipeRage.Core.Characters;
using RecipeRage.Core.GameModes;
using RecipeRage.Core.Networking;
using RecipeRage.Core.Networking.EOS;
using RecipeRage.Gameplay.Cooking;
using RecipeRage.Gameplay.Stations;
using RecipeRage.UI;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using NetworkManager = Unity.Netcode.NetworkManager;

namespace RecipeRage.Editor
{
    /// <summary>
    /// Generates and sets up the game scene for RecipeRage.
    /// </summary>
    public class SceneSetupGenerator
    {
        /// <summary>
        /// Generate the main menu scene.
        /// </summary>
        public static void GenerateMainMenuScene()
        {
            Debug.Log("GenerateMainMenuScene: Starting...");

            try
            {
                // Create a new scene
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                scene.name = "MainMenu";
                Debug.Log("GenerateMainMenuScene: Created new scene");

                // Create the camera
                var mainCamera = new GameObject("Main Camera");
                var camera = mainCamera.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
                camera.transform.position = new Vector3(0, 1, -10);
                mainCamera.tag = "MainCamera";
                Debug.Log("GenerateMainMenuScene: Created camera");

                // Create the UI canvas
                var canvas = new GameObject("Canvas");
                var canvasComponent = canvas.AddComponent<Canvas>();
                canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.AddComponent<CanvasScaler>();
                canvas.AddComponent<GraphicRaycaster>();
                Debug.Log("GenerateMainMenuScene: Created canvas");

                // Create the EOS managers
                var eosManager = new GameObject("EOSManager");
                var eosManagerComponent = eosManager.AddComponent<PlayEveryWare.EpicOnlineServices.EOSManager>();

                // Create the RecipeRage session manager
                var sessionManager = new GameObject("RecipeRageSessionManager");
                var sessionManagerComponent = sessionManager.AddComponent<RecipeRageSessionManager>();

                // Create the RecipeRage lobby manager
                var lobbyManager = new GameObject("RecipeRageLobbyManager");
                var lobbyManagerComponent = lobbyManager.AddComponent<RecipeRageLobbyManager>();

                // Create the RecipeRage P2P manager
                var p2pManager = new GameObject("RecipeRageP2PManager");
                var p2pManagerComponent = p2pManager.AddComponent<RecipeRageP2PManager>();

                // Initialize the EOS managers
                try
                {
                    sessionManagerComponent.Initialize();
                    Debug.Log("GenerateMainMenuScene: Initialized session manager");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to initialize session manager: {ex.Message}");
                }

                try
                {
                    lobbyManagerComponent.Initialize();
                    Debug.Log("GenerateMainMenuScene: Initialized lobby manager");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to initialize lobby manager: {ex.Message}");
                }

                try
                {
                    p2pManagerComponent.Initialize();
                    Debug.Log("GenerateMainMenuScene: Initialized P2P manager");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to initialize P2P manager: {ex.Message}");
                }

                // Save the scene
                string scenePath = "Assets/Scenes/MainMenu.unity";
                EditorSceneManager.SaveScene(scene, scenePath);

                Debug.Log($"Main menu scene setup complete. Saved to {scenePath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"GenerateMainMenuScene: Error - {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Generate the game scene.
        /// </summary>
        public static void GenerateGameScene()
        {
            Debug.Log("GenerateGameScene: Starting...");

            try
            {
                // Create a new scene
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                scene.name = "Game";
                Debug.Log("GenerateGameScene: Created new scene");

                // Create the camera
                var mainCamera = new GameObject("Main Camera");
                var camera = mainCamera.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
                camera.transform.position = new Vector3(0, 10, -10);
                camera.transform.rotation = Quaternion.Euler(45, 0, 0);
                mainCamera.tag = "MainCamera";
                Debug.Log("GenerateGameScene: Created camera");

                // Create a directional light
                var directionalLight = new GameObject("Directional Light");
                var light = directionalLight.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.0f;
                light.shadows = LightShadows.Soft;
                directionalLight.transform.position = new Vector3(0, 10, 0);
                directionalLight.transform.rotation = Quaternion.Euler(50, -30, 0);
                Debug.Log("GenerateGameScene: Created light");

                // Create the floor
                var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                floor.name = "Floor";
                floor.transform.position = Vector3.zero;
                floor.transform.localScale = new Vector3(2, 1, 2);
                Debug.Log("GenerateGameScene: Created floor");

                // Create the network manager
                var networkManager = new GameObject("NetworkManager");
                var networkManagerComponent = networkManager.AddComponent<NetworkManager>();
                Debug.Log("GenerateGameScene: Created network manager");

                // Create the game mode manager
                var gameModeManager = new GameObject("GameModeManager");
                var gameModeManagerComponent = gameModeManager.AddComponent<GameModeManager>();
                Debug.Log("GenerateGameScene: Created game mode manager");

                // Create the character manager
                var characterManager = new GameObject("CharacterManager");
                var characterManagerComponent = characterManager.AddComponent<CharacterManager>();
                Debug.Log("GenerateGameScene: Created character manager");

                // Create the EOS managers
                var eosManager = new GameObject("EOSManager");
                var eosManagerComponent = eosManager.AddComponent<PlayEveryWare.EpicOnlineServices.EOSManager>();
                Debug.Log("GenerateGameScene: Created EOS manager");

                // Create the RecipeRage session manager
                var sessionManager = new GameObject("RecipeRageSessionManager");
                var sessionManagerComponent = sessionManager.AddComponent<RecipeRageSessionManager>();
                Debug.Log("GenerateGameScene: Created session manager");

                // Create the RecipeRage lobby manager
                var lobbyManager = new GameObject("RecipeRageLobbyManager");
                var lobbyManagerComponent = lobbyManager.AddComponent<RecipeRageLobbyManager>();
                Debug.Log("GenerateGameScene: Created lobby manager");

                // Create the RecipeRage P2P manager
                var p2pManager = new GameObject("RecipeRageP2PManager");
                var p2pManagerComponent = p2pManager.AddComponent<RecipeRageP2PManager>();
                Debug.Log("GenerateGameScene: Created P2P manager");

                // Initialize the EOS managers
                try
                {
                    sessionManagerComponent.Initialize();
                    Debug.Log("GenerateGameScene: Initialized session manager");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to initialize session manager: {ex.Message}");
                }

                try
                {
                    lobbyManagerComponent.Initialize();
                    Debug.Log("GenerateGameScene: Initialized lobby manager");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to initialize lobby manager: {ex.Message}");
                }

                try
                {
                    p2pManagerComponent.Initialize();
                    Debug.Log("GenerateGameScene: Initialized P2P manager");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to initialize P2P manager: {ex.Message}");
                }

                // Create the spawn manager
                var spawnManager = new GameObject("SpawnManager");
                spawnManager.transform.position = new Vector3(0, 0, 0);
                Debug.Log("GenerateGameScene: Created spawn manager");

                // Create spawn points
                for (int i = 0; i < 4; i++)
                {
                    var spawnPoint = new GameObject($"SpawnPoint_{i}");
                    spawnPoint.transform.SetParent(spawnManager.transform);

                    // Position the spawn points in a circle
                    float angle = i * Mathf.PI / 2; // 90 degrees apart
                    spawnPoint.transform.position = new Vector3(Mathf.Cos(angle) * 5, 0, Mathf.Sin(angle) * 5);
                    spawnPoint.transform.rotation = Quaternion.Euler(0, -angle * Mathf.Rad2Deg + 180, 0); // Face center
                }
                Debug.Log("GenerateGameScene: Created spawn points");

                // Create UI
                var canvas = new GameObject("Canvas");
                var canvasComponent = canvas.AddComponent<Canvas>();
                canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.AddComponent<CanvasScaler>();
                canvas.AddComponent<GraphicRaycaster>();
                Debug.Log("GenerateGameScene: Created canvas");

                // Create UI manager
                var uiManager = new GameObject("GameplayUIManager");
                var uiManagerComponent = uiManager.AddComponent<GameplayUIManager>();
                Debug.Log("GenerateGameScene: Created UI manager");

                // Save the scene
                string scenePath = "Assets/Scenes/Game.unity";
                EditorSceneManager.SaveScene(scene, scenePath);

                Debug.Log($"Game scene setup complete. Saved to {scenePath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"GenerateGameScene: Error - {ex.Message}\n{ex.StackTrace}");
            }
        }
        /// <summary>
        /// Register a prefab with the NetworkManager.
        /// </summary>
        /// <param name="networkManager">The NetworkManager component.</param>
        /// <param name="prefab">The prefab to register.</param>
        private void RegisterNetworkPrefab(NetworkManager networkManager, GameObject prefab)
        {
            if (networkManager == null || prefab == null)
                return;

            // Create a new NetworkPrefab entry
            var prefabEntry = new Unity.Netcode.NetworkPrefab { Prefab = prefab };

            // Add the prefab to the NetworkManager's prefab list
            // The NetworkManager will handle duplicate entries
            try
            {
                networkManager.NetworkConfig.Prefabs.Add(prefabEntry);
                Debug.Log($"Registered network prefab: {prefab.name}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to register network prefab {prefab.name}: {e.Message}");
            }
        }

        /// <summary>
        /// Setup the game scene.
        /// </summary>
        /// <param name="scenesPath"> The path to save the scene. </param>
        /// <param name="prefabsPath"> The path to the prefabs. </param>
        /// <param name="gameModesPath"> The path to the game modes. </param>
        /// <param name="characterClassesPath"> The path to the character classes. </param>
        /// <param name="stationsPath"> The path to the cooking stations. </param>
        public void SetupScene(string scenesPath, string prefabsPath, string gameModesPath = null, string characterClassesPath = null, string stationsPath = null)
        {
            // Create a new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Create the main camera
            var mainCamera = new GameObject("Main Camera");
            var camera = mainCamera.AddComponent<Camera>();
            mainCamera.AddComponent<AudioListener>();
            mainCamera.tag = "MainCamera";

            // Position the camera
            mainCamera.transform.position = new Vector3(0, 10, -10);
            mainCamera.transform.rotation = Quaternion.Euler(45, 0, 0);

            // Setup the camera
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.2f, 0.2f, 0.2f);

            // Create a directional light
            var directionalLight = new GameObject("Directional Light");
            var light = directionalLight.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.0f;
            light.shadows = LightShadows.Soft;

            // Position the light
            directionalLight.transform.position = new Vector3(0, 10, 0);
            directionalLight.transform.rotation = Quaternion.Euler(50, -30, 0);

            // Create the floor
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(2, 1, 2);

            // Create the network manager
            var networkManager = new GameObject("NetworkManager");
            var networkManagerComponent = networkManager.AddComponent<NetworkManager>();

            // Create the game mode manager
            var gameModeManager = new GameObject("GameModeManager");
            var gameModeManagerComponent = gameModeManager.AddComponent<GameModeManager>();

            // Create the character manager
            var characterManager = new GameObject("CharacterManager");
            var characterManagerComponent = characterManager.AddComponent<CharacterManager>();

            // Create the EOS managers
            var eosManager = new GameObject("EOSManager");
            var eosManagerComponent = eosManager.AddComponent<PlayEveryWare.EpicOnlineServices.EOSManager>();

            // Create the RecipeRage session manager
            var sessionManager = new GameObject("RecipeRageSessionManager");
            var sessionManagerComponent = sessionManager.AddComponent<RecipeRageSessionManager>();

            // Create the RecipeRage lobby manager
            var lobbyManager = new GameObject("RecipeRageLobbyManager");
            var lobbyManagerComponent = lobbyManager.AddComponent<RecipeRageLobbyManager>();

            // Create the RecipeRage P2P manager
            var p2pManager = new GameObject("RecipeRageP2PManager");
            var p2pManagerComponent = p2pManager.AddComponent<RecipeRageP2PManager>();

            // Initialize the EOS managers
            sessionManagerComponent.Initialize();
            lobbyManagerComponent.Initialize();
            p2pManagerComponent.Initialize();

            // Setup references between managers
            // No need to set references as each manager initializes itself

            // Create the spawn manager
            var spawnManager = new GameObject("SpawnManager");
            spawnManager.transform.position = new Vector3(0, 0, 0);

            // Create spawn points
            for (int i = 0; i < 4; i++)
            {
                var spawnPoint = new GameObject($"SpawnPoint_{i}");
                spawnPoint.transform.SetParent(spawnManager.transform);

                // Position the spawn points in a circle
                float angle = i * Mathf.PI / 2; // 90 degrees apart
                spawnPoint.transform.position = new Vector3(Mathf.Cos(angle) * 5, 0, Mathf.Sin(angle) * 5);
                spawnPoint.transform.rotation = Quaternion.Euler(0, -angle * Mathf.Rad2Deg + 180, 0); // Face center
            }

            // Load prefabs
            var orderManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabsPath}/OrderManager.prefab");
            var cookingStationPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabsPath}/Stations/CookingStation.prefab");
            var cuttingStationPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabsPath}/Stations/CuttingStation.prefab");
            var servingStationPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabsPath}/Stations/ServingStation.prefab");
            var ingredientSpawnerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabsPath}/Stations/IngredientSpawner.prefab");
            var trashBinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabsPath}/Stations/TrashBin.prefab");
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabsPath}/Player/Player.prefab");

            // Create the order manager
            if (orderManagerPrefab != null)
            {
                var orderManager = PrefabUtility.InstantiatePrefab(orderManagerPrefab) as GameObject;
                orderManager.transform.position = new Vector3(0, 0, 0);

                // Load ingredients and recipes
                var orderManagerComponent = orderManager.GetComponent<OrderManager>();
                if (orderManagerComponent != null)
                {
                    // Load recipes
                    string[] recipeGuids = AssetDatabase.FindAssets("t:Recipe", new[]
                    {
                        "Assets/ScriptableObjects/Recipes"
                    });
                    var recipes = new List<Recipe>();

                    foreach (string guid in recipeGuids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        var recipe = AssetDatabase.LoadAssetAtPath<Recipe>(path);

                        if (recipe != null)
                        {
                            recipes.Add(recipe);
                        }
                    }

                    // Set recipes
                    var serializedObject = new SerializedObject(orderManagerComponent);
                    var recipesProperty = serializedObject.FindProperty("_availableRecipes");
                    recipesProperty.ClearArray();

                    for (int i = 0; i < recipes.Count; i++)
                    {
                        recipesProperty.arraySize++;
                        recipesProperty.GetArrayElementAtIndex(i).objectReferenceValue = recipes[i];
                    }

                    serializedObject.ApplyModifiedProperties();
                }

                // Register the order manager with the network manager
                if (networkManagerComponent != null && orderManager.GetComponent<NetworkObject>() != null)
                {
                    RegisterNetworkPrefab(networkManagerComponent, orderManagerPrefab);
                }
            }

            // Create cooking stations
            if (cookingStationPrefab != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    var cookingStation = PrefabUtility.InstantiatePrefab(cookingStationPrefab) as GameObject;
                    cookingStation.transform.position = new Vector3(-3 + i * 6, 0, -3);

                    // Register with the network manager
                    if (networkManagerComponent != null)
                    {
                        RegisterNetworkPrefab(networkManagerComponent, cookingStationPrefab);
                    }
                }
            }

            // Create cutting stations
            if (cuttingStationPrefab != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    var cuttingStation = PrefabUtility.InstantiatePrefab(cuttingStationPrefab) as GameObject;
                    cuttingStation.transform.position = new Vector3(-3 + i * 6, 0, 3);

                    // Register with the network manager
                    if (networkManagerComponent != null)
                    {
                        RegisterNetworkPrefab(networkManagerComponent, cuttingStationPrefab);
                    }
                }
            }

            // Create serving station
            if (servingStationPrefab != null)
            {
                var servingStation = PrefabUtility.InstantiatePrefab(servingStationPrefab) as GameObject;
                servingStation.transform.position = new Vector3(0, 0, 6);

                // Register with the network manager
                if (networkManagerComponent != null)
                {
                    RegisterNetworkPrefab(networkManagerComponent, servingStationPrefab);
                }
            }

            // Create ingredient spawners
            if (ingredientSpawnerPrefab != null)
            {
                // Load ingredients
                string[] ingredientGuids = AssetDatabase.FindAssets("t:Ingredient", new[]
                {
                    "Assets/ScriptableObjects/Ingredients"
                });
                var ingredients = new List<Ingredient>();

                foreach (string guid in ingredientGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var ingredient = AssetDatabase.LoadAssetAtPath<Ingredient>(path);

                    if (ingredient != null)
                    {
                        ingredients.Add(ingredient);
                    }
                }

                // Create spawners for each ingredient
                for (int i = 0; i < Mathf.Min(ingredients.Count, 8); i++)
                {
                    var spawner = PrefabUtility.InstantiatePrefab(ingredientSpawnerPrefab) as GameObject;

                    // Position the spawners in a circle
                    float angle = i * Mathf.PI / 4; // 45 degrees apart
                    spawner.transform.position = new Vector3(Mathf.Cos(angle) * 8, 0, Mathf.Sin(angle) * 8);

                    // Set the ingredient
                    var spawnerComponent = spawner.GetComponent<IngredientSpawner>();
                    if (spawnerComponent != null && i < ingredients.Count)
                    {
                        var serializedObject = new SerializedObject(spawnerComponent);
                        serializedObject.FindProperty("_ingredientToSpawn").objectReferenceValue = ingredients[i];
                        serializedObject.ApplyModifiedProperties();
                    }

                    // Register with the network manager
                    if (networkManagerComponent != null)
                    {
                        RegisterNetworkPrefab(networkManagerComponent, ingredientSpawnerPrefab);
                    }
                }
            }

            // Create trash bin
            if (trashBinPrefab != null)
            {
                var trashBin = PrefabUtility.InstantiatePrefab(trashBinPrefab) as GameObject;
                trashBin.transform.position = new Vector3(0, 0, -6);

                // Register with the network manager
                if (networkManagerComponent != null)
                {
                    RegisterNetworkPrefab(networkManagerComponent, trashBinPrefab);
                }
            }

            // Register player prefab with the network manager
            if (networkManagerComponent != null && playerPrefab != null)
            {
                RegisterNetworkPrefab(networkManagerComponent, playerPrefab);
                // networkManagerComponent.PlayerPrefab = playerPrefab;
                // TODO: We spawn the player prefab in the scene using the spawn manager
            }

            // Create UI
            var canvas = new GameObject("Canvas");
            var canvasComponent = canvas.AddComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();

            // Create UI manager
            var uiManager = new GameObject("GameplayUIManager");
            var uiManagerComponent = uiManager.AddComponent<GameplayUIManager>();

            // Create UI elements
            var interactionPromptPanel = new GameObject("InteractionPromptPanel");
            interactionPromptPanel.transform.SetParent(canvas.transform, false);
            var interactionPromptRect = interactionPromptPanel.AddComponent<RectTransform>();
            interactionPromptRect.anchorMin = new Vector2(0.5f, 0);
            interactionPromptRect.anchorMax = new Vector2(0.5f, 0);
            interactionPromptRect.pivot = new Vector2(0.5f, 0);
            interactionPromptRect.anchoredPosition = new Vector2(0, 100);
            interactionPromptRect.sizeDelta = new Vector2(300, 50);

            var interactionPromptText = new GameObject("InteractionPromptText");
            interactionPromptText.transform.SetParent(interactionPromptPanel.transform, false);
            var interactionPromptTextRect = interactionPromptText.AddComponent<RectTransform>();
            interactionPromptTextRect.anchorMin = Vector2.zero;
            interactionPromptTextRect.anchorMax = Vector2.one;
            interactionPromptTextRect.offsetMin = Vector2.zero;
            interactionPromptTextRect.offsetMax = Vector2.zero;

            // Add TMPro component if available
#if UNITY_2018_1_OR_NEWER
            if (Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro") != null)
            {
                interactionPromptText.AddComponent(Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro"));
            }
            else
            {
                interactionPromptText.AddComponent<Text>();
            }
#else
            interactionPromptText.AddComponent<UnityEngine.UI.Text>();
#endif

            // Create order list panel
            var orderListPanel = new GameObject("OrderListPanel");
            orderListPanel.transform.SetParent(canvas.transform, false);
            var orderListRect = orderListPanel.AddComponent<RectTransform>();
            orderListRect.anchorMin = new Vector2(1, 1);
            orderListRect.anchorMax = new Vector2(1, 1);
            orderListRect.pivot = new Vector2(1, 1);
            orderListRect.anchoredPosition = new Vector2(-10, -10);
            orderListRect.sizeDelta = new Vector2(300, 400);

            var orderListContent = new GameObject("OrderListContent");
            orderListContent.transform.SetParent(orderListPanel.transform, false);
            var orderListContentRect = orderListContent.AddComponent<RectTransform>();
            orderListContentRect.anchorMin = Vector2.zero;
            orderListContentRect.anchorMax = Vector2.one;
            orderListContentRect.offsetMin = new Vector2(10, 10);
            orderListContentRect.offsetMax = new Vector2(-10, -10);

            // Create score text
            var scoreText = new GameObject("ScoreText");
            scoreText.transform.SetParent(canvas.transform, false);
            var scoreTextRect = scoreText.AddComponent<RectTransform>();
            scoreTextRect.anchorMin = new Vector2(0, 1);
            scoreTextRect.anchorMax = new Vector2(0, 1);
            scoreTextRect.pivot = new Vector2(0, 1);
            scoreTextRect.anchoredPosition = new Vector2(10, -10);
            scoreTextRect.sizeDelta = new Vector2(200, 50);

            // Add TMPro component if available
#if UNITY_2018_1_OR_NEWER
            if (Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro") != null)
            {
                scoreText.AddComponent(Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro"));
            }
            else
            {
                scoreText.AddComponent<Text>();
            }
#else
            scoreText.AddComponent<UnityEngine.UI.Text>();
#endif

            // Create timer
            var timerPanel = new GameObject("TimerPanel");
            timerPanel.transform.SetParent(canvas.transform, false);
            var timerPanelRect = timerPanel.AddComponent<RectTransform>();
            timerPanelRect.anchorMin = new Vector2(0.5f, 1);
            timerPanelRect.anchorMax = new Vector2(0.5f, 1);
            timerPanelRect.pivot = new Vector2(0.5f, 1);
            timerPanelRect.anchoredPosition = new Vector2(0, -10);
            timerPanelRect.sizeDelta = new Vector2(200, 50);

            var timerFill = new GameObject("TimerFill");
            timerFill.transform.SetParent(timerPanel.transform, false);
            var timerFillRect = timerFill.AddComponent<RectTransform>();
            timerFillRect.anchorMin = Vector2.zero;
            timerFillRect.anchorMax = Vector2.one;
            timerFillRect.offsetMin = Vector2.zero;
            timerFillRect.offsetMax = Vector2.zero;
            var timerFillImage = timerFill.AddComponent<Image>();
            timerFillImage.color = Color.green;
            timerFillImage.fillMethod = Image.FillMethod.Horizontal;
            timerFillImage.type = Image.Type.Filled;
            timerFillImage.fillAmount = 1.0f;

            var timerText = new GameObject("TimerText");
            timerText.transform.SetParent(timerPanel.transform, false);
            var timerTextRect = timerText.AddComponent<RectTransform>();
            timerTextRect.anchorMin = Vector2.zero;
            timerTextRect.anchorMax = Vector2.one;
            timerTextRect.offsetMin = Vector2.zero;
            timerTextRect.offsetMax = Vector2.zero;

            // Add TMPro component if available
#if UNITY_2018_1_OR_NEWER
            if (Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro") != null)
            {
                timerText.AddComponent(Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro"));
            }
            else
            {
                timerText.AddComponent<Text>();
            }
#else
            timerText.AddComponent<UnityEngine.UI.Text>();
#endif

            // Set UI manager references
            var uiSerializedObject = new SerializedObject(uiManagerComponent);
            uiSerializedObject.FindProperty("_interactionPromptPanel").objectReferenceValue = interactionPromptPanel;
            uiSerializedObject.FindProperty("_interactionPromptText").objectReferenceValue = interactionPromptText.GetComponent<Text>();
            uiSerializedObject.FindProperty("_orderListPanel").objectReferenceValue = orderListPanel;
            uiSerializedObject.FindProperty("_orderListContent").objectReferenceValue = orderListContent.transform;
            uiSerializedObject.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<Text>();
            uiSerializedObject.FindProperty("_timerText").objectReferenceValue = timerText.GetComponent<Text>();
            uiSerializedObject.FindProperty("_timerFill").objectReferenceValue = timerFillImage;

            // Try to load the order item prefab
            var orderItemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabsPath}/UI/OrderItem.prefab");
            if (orderItemPrefab != null)
            {
                uiSerializedObject.FindProperty("_orderItemPrefab").objectReferenceValue = orderItemPrefab;
            }

            uiSerializedObject.ApplyModifiedProperties();

            // Load game modes if path is provided
            if (!string.IsNullOrEmpty(gameModesPath))
            {
                // Find all game mode assets
                var gameModeGuids = AssetDatabase.FindAssets("t:GameMode", new[] { gameModesPath });
                var gameModes = new List<GameMode>();

                foreach (var guid in gameModeGuids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var gameMode = AssetDatabase.LoadAssetAtPath<GameMode>(path);

                    if (gameMode != null)
                    {
                        gameModes.Add(gameMode);
                    }
                }

                // Add game modes to the game mode manager
                if (gameModes.Count > 0)
                {
                    var gameModeManagerSerialized = new SerializedObject(gameModeManagerComponent);
                    var availableGameModesProperty = gameModeManagerSerialized.FindProperty("_availableGameModes");

                    availableGameModesProperty.ClearArray();
                    for (int i = 0; i < gameModes.Count; i++)
                    {
                        availableGameModesProperty.arraySize++;
                        availableGameModesProperty.GetArrayElementAtIndex(i).objectReferenceValue = gameModes[i];
                    }

                    // Set the default game mode
                    var defaultGameMode = gameModes.Find(gm => gm.Id == "classic") ?? gameModes[0];
                    gameModeManagerSerialized.FindProperty("_defaultGameModeId").stringValue = defaultGameMode.Id;

                    gameModeManagerSerialized.ApplyModifiedProperties();

                    Debug.Log($"Added {gameModes.Count} game modes to the GameModeManager");
                }
                else
                {
                    Debug.LogWarning("No game modes found. Make sure to generate game modes first.");
                }
            }

            // Load character classes if path is provided
            if (!string.IsNullOrEmpty(characterClassesPath))
            {
                // Find all character class assets
                var characterClassGuids = AssetDatabase.FindAssets("t:CharacterClass", new[] { characterClassesPath });
                var characterClasses = new List<CharacterClass>();

                foreach (var guid in characterClassGuids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var characterClass = AssetDatabase.LoadAssetAtPath<CharacterClass>(path);

                    if (characterClass != null)
                    {
                        characterClasses.Add(characterClass);
                    }
                }

                // Add character classes to the character manager
                if (characterClasses.Count > 0)
                {
                    var characterManagerSerialized = new SerializedObject(characterManagerComponent);
                    var availableCharacterClassesProperty = characterManagerSerialized.FindProperty("_availableCharacterClasses");

                    availableCharacterClassesProperty.ClearArray();
                    for (int i = 0; i < characterClasses.Count; i++)
                    {
                        availableCharacterClassesProperty.arraySize++;
                        availableCharacterClassesProperty.GetArrayElementAtIndex(i).objectReferenceValue = characterClasses[i];
                    }

                    // Set the default character class
                    var defaultCharacterClass = characterClasses.Find(cc => cc.Id == 1) ?? characterClasses[0];
                    characterManagerSerialized.FindProperty("_defaultCharacterClassId").intValue = defaultCharacterClass.Id;

                    characterManagerSerialized.ApplyModifiedProperties();

                    Debug.Log($"Added {characterClasses.Count} character classes to the CharacterManager");
                }
                else
                {
                    Debug.LogWarning("No character classes found. Make sure to generate character classes first.");
                }
            }

            // Add cooking stations to the scene if path is provided
            if (!string.IsNullOrEmpty(stationsPath))
            {
                // Find all station prefabs
                var stationGuids = AssetDatabase.FindAssets("t:Prefab", new[] { stationsPath });
                var stations = new List<GameObject>();

                foreach (var guid in stationGuids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var stationPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                    if (stationPrefab != null)
                    {
                        stations.Add(stationPrefab);
                    }
                }

                // Add stations to the scene
                if (stations.Count > 0)
                {
                    // Create a stations parent object
                    var stationsParent = new GameObject("Stations");

                    // Position stations in a grid layout
                    float spacing = 2.0f;
                    int columns = Mathf.CeilToInt(Mathf.Sqrt(stations.Count));

                    for (int i = 0; i < stations.Count; i++)
                    {
                        int row = i / columns;
                        int col = i % columns;

                        Vector3 position = new Vector3(col * spacing, 0, row * spacing);

                        var stationInstance = PrefabUtility.InstantiatePrefab(stations[i]) as GameObject;
                        stationInstance.transform.SetParent(stationsParent.transform);
                        stationInstance.transform.position = position;
                    }

                    Debug.Log($"Added {stations.Count} cooking stations to the scene");
                }
                else
                {
                    Debug.LogWarning("No station prefabs found. Make sure to generate stations first.");
                }
            }

            // Save the scene
            string scenePath = $"{scenesPath}/RecipeRage.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"Scene setup complete. Saved to {scenePath}");
        }
    }
}