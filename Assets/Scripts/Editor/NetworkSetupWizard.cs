using UnityEngine;
using UnityEditor;
using Unity.Netcode;
using Core.State;
using Gameplay;
using Gameplay.Cooking;
using Gameplay.Scoring;
using Gameplay.Stations;
using System.Collections.Generic;

namespace Editor
{
    /// <summary>
    /// Wizard to help set up network components in the scene.
    /// </summary>
    public class NetworkSetupWizard : EditorWindow
    {
        private GameObject _networkManagersObject;
        private NetworkManager _networkManager;
        private bool _setupComplete = false;
        
        [MenuItem("RecipeRage/Network Setup Wizard")]
        public static void ShowWindow()
        {
            GetWindow<NetworkSetupWizard>("Network Setup Wizard");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("RecipeRage Network Setup Wizard", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            EditorGUILayout.HelpBox(
                "This wizard will help you set up all network components in your Game scene.",
                MessageType.Info
            );
            
            GUILayout.Space(10);
            
            // Step 1: Create Network Managers GameObject
            if (GUILayout.Button("Step 1: Create Network Managers GameObject", GUILayout.Height(40)))
            {
                CreateNetworkManagersObject();
            }
            
            GUILayout.Space(5);
            
            // Step 2: Find/Create NetworkManager
            if (GUILayout.Button("Step 2: Setup NetworkManager", GUILayout.Height(40)))
            {
                SetupNetworkManager();
            }
            
            GUILayout.Space(5);
            
            // Step 3: Add Station Controllers
            if (GUILayout.Button("Step 3: Add StationNetworkController to Stations", GUILayout.Height(40)))
            {
                AddStationControllers();
            }
            
            GUILayout.Space(5);
            
            // Step 4: Configure Prefabs
            if (GUILayout.Button("Step 4: Add NetworkObject to Prefabs", GUILayout.Height(40)))
            {
                ConfigurePrefabs();
            }
            
            GUILayout.Space(5);
            
            // Step 5: Register Prefabs
            if (GUILayout.Button("Step 5: Register Prefabs in NetworkManager", GUILayout.Height(40)))
            {
                RegisterPrefabsInNetworkManager();
            }
            
            GUILayout.Space(20);
            
            if (_setupComplete)
            {
                EditorGUILayout.HelpBox(
                    "Setup complete! Check the console for details.",
                    MessageType.Info
                );
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Complete Setup (All Steps)", GUILayout.Height(50)))
            {
                CreateNetworkManagersObject();
                SetupNetworkManager();
                AddStationControllers();
                ConfigurePrefabs();
                RegisterPrefabsInNetworkManager();
                _setupComplete = true;
            }
        }
        
        private void CreateNetworkManagersObject()
        {
            // Check if already exists
            _networkManagersObject = GameObject.Find("NetworkManagers");
            
            if (_networkManagersObject == null)
            {
                _networkManagersObject = new GameObject("NetworkManagers");
                Undo.RegisterCreatedObjectUndo(_networkManagersObject, "Create NetworkManagers");
                Debug.Log("[NetworkSetup] Created NetworkManagers GameObject");
            }
            else
            {
                Debug.Log("[NetworkSetup] NetworkManagers GameObject already exists");
            }
            
            // Add components if they don't exist
            AddComponentIfMissing<NetworkGameStateManager>(_networkManagersObject);
            AddComponentIfMissing<NetworkScoreManager>(_networkManagersObject);
            AddComponentIfMissing<RoundTimer>(_networkManagersObject);
            AddComponentIfMissing<IngredientNetworkSpawner>(_networkManagersObject);
            
            EditorUtility.SetDirty(_networkManagersObject);
            Debug.Log("[NetworkSetup] Added all network manager components");
        }
        
        private void SetupNetworkManager()
        {
            _networkManager = FindObjectOfType<NetworkManager>();
            
            if (_networkManager == null)
            {
                GameObject nmObject = new GameObject("NetworkManager");
                _networkManager = nmObject.AddComponent<NetworkManager>();
                Undo.RegisterCreatedObjectUndo(nmObject, "Create NetworkManager");
                Debug.Log("[NetworkSetup] Created NetworkManager");
            }
            else
            {
                Debug.Log("[NetworkSetup] NetworkManager already exists");
            }
            
            // Configure NetworkManager settings
            _networkManager.NetworkConfig.TickRate = 30;
            
            EditorUtility.SetDirty(_networkManager);
            Debug.Log("[NetworkSetup] Configured NetworkManager settings");
        }
        
        private void AddStationControllers()
        {
            string[] stationPaths = new string[]
            {
                "Assets/Prefabs/Stations/CookingPot.prefab",
                "Assets/Prefabs/Stations/CuttingStation.prefab",
                "Assets/Prefabs/Stations/AssemblyStation.prefab",
                "Assets/Prefabs/Stations/ServingStation.prefab"
            };
            
            int addedCount = 0;
            
            foreach (string path in stationPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    // Load prefab for editing
                    string prefabPath = AssetDatabase.GetAssetPath(prefab);
                    GameObject prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);
                    
                    // Add StationNetworkController if missing
                    if (prefabInstance.GetComponent<StationNetworkController>() == null)
                    {
                        prefabInstance.AddComponent<StationNetworkController>();
                        addedCount++;
                        Debug.Log($"[NetworkSetup] Added StationNetworkController to {prefab.name}");
                    }
                    
                    // Save prefab
                    PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
                    PrefabUtility.UnloadPrefabContents(prefabInstance);
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"[NetworkSetup] Added StationNetworkController to {addedCount} stations");
        }
        
        private void ConfigurePrefabs()
        {
            string[] prefabPaths = new string[]
            {
                "Assets/Prefabs/Player/Player.prefab",
                "Assets/Prefabs/Stations/CookingPot.prefab",
                "Assets/Prefabs/Stations/CuttingStation.prefab",
                "Assets/Prefabs/Stations/AssemblyStation.prefab",
                "Assets/Prefabs/Stations/ServingStation.prefab",
                "Assets/Prefabs/Plate.prefab"
            };
            
            int configuredCount = 0;
            
            foreach (string path in prefabPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    // Load prefab for editing
                    string prefabPath = AssetDatabase.GetAssetPath(prefab);
                    GameObject prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);
                    
                    // Add NetworkObject if missing
                    if (prefabInstance.GetComponent<NetworkObject>() == null)
                    {
                        prefabInstance.AddComponent<NetworkObject>();
                        configuredCount++;
                        Debug.Log($"[NetworkSetup] Added NetworkObject to {prefab.name}");
                    }
                    
                    // Save prefab
                    PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
                    PrefabUtility.UnloadPrefabContents(prefabInstance);
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"[NetworkSetup] Configured {configuredCount} prefabs with NetworkObject");
        }
        
        private void RegisterPrefabsInNetworkManager()
        {
            if (_networkManager == null)
            {
                _networkManager = FindObjectOfType<NetworkManager>();
            }
            
            if (_networkManager == null)
            {
                Debug.LogError("[NetworkSetup] NetworkManager not found! Run Step 2 first.");
                return;
            }
            
            // Load prefabs
            List<GameObject> prefabsToRegister = new List<GameObject>();
            
            string[] prefabPaths = new string[]
            {
                "Assets/Prefabs/Player/Player.prefab",
                "Assets/Prefabs/Stations/CookingPot.prefab",
                "Assets/Prefabs/Stations/CuttingStation.prefab",
                "Assets/Prefabs/Stations/AssemblyStation.prefab",
                "Assets/Prefabs/Stations/ServingStation.prefab",
                "Assets/Prefabs/Plate.prefab"
            };
            
            foreach (string path in prefabPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null && prefab.GetComponent<NetworkObject>() != null)
                {
                    prefabsToRegister.Add(prefab);
                }
            }
            
            // Get existing prefab list
            var networkPrefabs = _networkManager.NetworkConfig.Prefabs;
            
            // Add prefabs that aren't already registered
            int addedCount = 0;
            foreach (GameObject prefab in prefabsToRegister)
            {
                bool alreadyRegistered = false;
                foreach (var networkPrefab in networkPrefabs.Prefabs)
                {
                    if (networkPrefab.Prefab == prefab)
                    {
                        alreadyRegistered = true;
                        break;
                    }
                }
                
                if (!alreadyRegistered)
                {
                    networkPrefabs.Add(new NetworkPrefab { Prefab = prefab });
                    addedCount++;
                    Debug.Log($"[NetworkSetup] Registered {prefab.name} in NetworkManager");
                }
            }
            
            // Set player prefab
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/Player.prefab");
            if (playerPrefab != null)
            {
                _networkManager.NetworkConfig.PlayerPrefab = playerPrefab;
                Debug.Log("[NetworkSetup] Set Player prefab as PlayerPrefab in NetworkManager");
            }
            
            EditorUtility.SetDirty(_networkManager);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"[NetworkSetup] Registered {addedCount} new prefabs in NetworkManager");
        }
        
        private T AddComponentIfMissing<T>(GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component == null)
            {
                component = obj.AddComponent<T>();
                Debug.Log($"[NetworkSetup] Added {typeof(T).Name} to {obj.name}");
            }
            return component;
        }
    }
}
