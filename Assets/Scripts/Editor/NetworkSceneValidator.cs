using UnityEngine;
using UnityEditor;
using Unity.Netcode;
using App.State;
using Modules.Networking;
using Gameplay;
using Gameplay.Cooking;
using Gameplay.Scoring;
using System.Collections.Generic;

namespace Editor
{
    /// <summary>
    /// Validates that the scene is properly set up for networking.
    /// </summary>
    public class NetworkSceneValidator : EditorWindow
    {
        private Vector2 _scrollPosition;
        private List<ValidationResult> _results = new List<ValidationResult>();
        
        [MenuItem("RecipeRage/Validate Network Setup")]
        public static void ShowWindow()
        {
            GetWindow<NetworkSceneValidator>("Network Validator");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Network Setup Validator", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (GUILayout.Button("Run Validation", GUILayout.Height(40)))
            {
                RunValidation();
            }
            
            GUILayout.Space(10);
            
            if (_results.Count > 0)
            {
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
                
                int passCount = 0;
                int failCount = 0;
                int warningCount = 0;
                
                foreach (var result in _results)
                {
                    switch (result.Type)
                    {
                        case ValidationType.Pass:
                            passCount++;
                            EditorGUILayout.HelpBox($"✓ {result.Message}", MessageType.Info);
                            break;
                        case ValidationType.Fail:
                            failCount++;
                            EditorGUILayout.HelpBox($"✗ {result.Message}", MessageType.Error);
                            break;
                        case ValidationType.Warning:
                            warningCount++;
                            EditorGUILayout.HelpBox($"⚠ {result.Message}", MessageType.Warning);
                            break;
                    }
                    GUILayout.Space(5);
                }
                
                EditorGUILayout.EndScrollView();
                
                GUILayout.Space(10);
                GUILayout.Label($"Results: {passCount} passed, {failCount} failed, {warningCount} warnings", EditorStyles.boldLabel);
                
                if (failCount == 0)
                {
                    EditorGUILayout.HelpBox("✓ All critical checks passed! Your scene is ready for networking.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox($"✗ {failCount} critical issues found. Please fix them before testing.", MessageType.Error);
                }
            }
        }
        
        private void RunValidation()
        {
            _results.Clear();
            
            // Scene Components
            ValidateNetworkManager();
            ValidateNetworkManagers();
            ValidateNetworkInitializer();
            
            // Prefabs
            ValidatePrefabs();
            
            // Transport
            ValidateTransport();
            
            Repaint();
        }
        
        private void ValidateNetworkManager()
        {
            NetworkManager nm = FindObjectOfType<NetworkManager>();
            
            if (nm == null)
            {
                _results.Add(new ValidationResult
                {
                    Type = ValidationType.Fail,
                    Message = "NetworkManager not found in scene. Create one or run Network Setup Wizard."
                });
                return;
            }
            
            _results.Add(new ValidationResult
            {
                Type = ValidationType.Pass,
                Message = "NetworkManager found in scene"
            });
            
            // Check player prefab
            if (nm.NetworkConfig.PlayerPrefab == null)
            {
                _results.Add(new ValidationResult
                {
                    Type = ValidationType.Fail,
                    Message = "Player Prefab not set in NetworkManager"
                });
            }
            else
            {
                _results.Add(new ValidationResult
                {
                    Type = ValidationType.Pass,
                    Message = $"Player Prefab set: {nm.NetworkConfig.PlayerPrefab.name}"
                });
            }
            
            // Check network prefabs
            int prefabCount = nm.NetworkConfig.Prefabs.Prefabs.Count;
            if (prefabCount == 0)
            {
                _results.Add(new ValidationResult
                {
                    Type = ValidationType.Warning,
                    Message = "No prefabs registered in NetworkManager"
                });
            }
            else
            {
                _results.Add(new ValidationResult
                {
                    Type = ValidationType.Pass,
                    Message = $"{prefabCount} prefabs registered in NetworkManager"
                });
            }
        }
        
        private void ValidateNetworkManagers()
        {
            GameObject networkManagers = GameObject.Find("NetworkManagers");
            
            if (networkManagers == null)
            {
                _results.Add(new ValidationResult
                {
                    Type = ValidationType.Fail,
                    Message = "NetworkManagers GameObject not found. Run Network Setup Wizard Step 1."
                });
                return;
            }
            
            _results.Add(new ValidationResult
            {
                Type = ValidationType.Pass,
                Message = "NetworkManagers GameObject found"
            });
            
            // Check components
            CheckComponent<NetworkGameStateManager>(networkManagers, "NetworkGameStateManager");
            CheckComponent<NetworkScoreManager>(networkManagers, "NetworkScoreManager");
            CheckComponent<RoundTimer>(networkManagers, "RoundTimer");
            CheckComponent<IngredientNetworkSpawner>(networkManagers, "IngredientNetworkSpawner");
        }
        
        private void ValidateNetworkInitializer()
        {
            NetworkInitializer initializer = FindObjectOfType<NetworkInitializer>();
            
            if (initializer == null)
            {
                _results.Add(new ValidationResult
                {
                    Type = ValidationType.Warning,
                    Message = "NetworkInitializer not found. Add it to NetworkManagers GameObject for automatic initialization."
                });
            }
            else
            {
                _results.Add(new ValidationResult
                {
                    Type = ValidationType.Pass,
                    Message = "NetworkInitializer found"
                });
            }
        }
        
        private void ValidatePrefabs()
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
            
            foreach (string path in prefabPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    NetworkObject networkObject = prefab.GetComponent<NetworkObject>();
                    if (networkObject == null)
                    {
                        _results.Add(new ValidationResult
                        {
                            Type = ValidationType.Fail,
                            Message = $"{prefab.name} missing NetworkObject component"
                        });
                    }
                    else
                    {
                        _results.Add(new ValidationResult
                        {
                            Type = ValidationType.Pass,
                            Message = $"{prefab.name} has NetworkObject"
                        });
                    }
                }
                else
                {
                    _results.Add(new ValidationResult
                    {
                        Type = ValidationType.Warning,
                        Message = $"Prefab not found: {path}"
                    });
                }
            }
        }
        
        private void ValidateTransport()
        {
            NetworkManager nm = FindObjectOfType<NetworkManager>();
            if (nm != null)
            {
                var transport = nm.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
                if (transport == null)
                {
                    // Check for EOS Transport
                    var eosTransport = nm.GetComponent<PlayEveryWare.EpicOnlineServices.Samples.Network.EOSTransport>();
                    if (eosTransport != null)
                    {
                        _results.Add(new ValidationResult
                        {
                            Type = ValidationType.Pass,
                            Message = "EOS Transport configured"
                        });
                    }
                    else
                    {
                        _results.Add(new ValidationResult
                        {
                            Type = ValidationType.Warning,
                            Message = "No transport found on NetworkManager"
                        });
                    }
                }
                else
                {
                    _results.Add(new ValidationResult
                    {
                        Type = ValidationType.Pass,
                        Message = "Unity Transport configured"
                    });
                }
            }
        }
        
        private void CheckComponent<T>(GameObject obj, string componentName) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component == null)
            {
                _results.Add(new ValidationResult
                {
                    Type = ValidationType.Fail,
                    Message = $"{componentName} not found on {obj.name}"
                });
            }
            else
            {
                _results.Add(new ValidationResult
                {
                    Type = ValidationType.Pass,
                    Message = $"{componentName} found"
                });
            }
        }
        
        private struct ValidationResult
        {
            public ValidationType Type;
            public string Message;
        }
        
        private enum ValidationType
        {
            Pass,
            Fail,
            Warning
        }
    }
}
