using System;
using System.Collections.Generic;
using RecipeRage.Gameplay.Cooking;
using RecipeRage.Gameplay.Interactables;
using RecipeRage.UI;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace RecipeRage.Editor
{
    /// <summary>
    /// Generates and sets up the game scene for RecipeRage.
    /// </summary>
    public class SceneSetupGenerator
    {
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
        public void SetupScene(string scenesPath, string prefabsPath)
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

            // Save the scene
            string scenePath = $"{scenesPath}/RecipeRage.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"Scene setup complete. Saved to {scenePath}");
        }
    }
}