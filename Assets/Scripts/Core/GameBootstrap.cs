using RecipeRage.Core.GameFramework.State;
using RecipeRage.Core.GameFramework.State.States;
using RecipeRage.Core.Input;
using RecipeRage.Core.Networking;
using RecipeRage.Core.Patterns;
using UnityEngine;

namespace RecipeRage.Core
{
    /// <summary>
    /// Central bootstrap class for the entire game.
    /// Initializes all game subsystems.
    /// This should be the only MonoBehaviour that needs to be attached to a scene object.
    /// </summary>
    public class GameBootstrap : MonoBehaviourSingleton<GameBootstrap>
    {
        [Header("Epic Online Services")]
        [SerializeField] private bool _initializeEOS = true;
        private bool _eosInitialized;

        /// <summary>
        /// Initialize core systems on Awake
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            Debug.Log("[GameBootstrap] Initializing game bootstrap");

            // Initialize core systems
            InitializeCoreSystems();
        }

        /// <summary>
        /// Initialize remaining systems on Start
        /// </summary>
        private void Start()
        {
            // Initialize EOS if needed
            if (_initializeEOS)
            {
                InitializeEOS();
            }

            // Initialize game state manager
            InitializeGameStateManager();

            Debug.Log("[GameBootstrap] All core systems initialized");
        }

        /// <summary>
        /// OnDestroy is called when the MonoBehaviour is being destroyed
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unregister application quit handler
            Application.quitting -= OnApplicationQuit;
        }

        /// <summary>
        /// Handle application quit to clean up resources
        /// </summary>
        private void OnApplicationQuit()
        {
            Debug.Log("[GameBootstrap] Application quitting, shutting down services");

            // Clean up any resources here
        }

        /// <summary>
        /// Initialize core systems
        /// </summary>
        private void InitializeCoreSystems()
        {
            Debug.Log("[GameBootstrap] Initializing core systems");

            // Register for application quit to properly shut down
            Application.quitting += OnApplicationQuit;

            // Initialize service locator
            ServiceLocator.Instance.Clear();

            // Initialize input manager
            InitializeInputManager();

            // Initialize network manager
            InitializeNetworkManager();
        }

        /// <summary>
        /// Initialize the Epic Online Services SDK
        /// </summary>
        private void InitializeEOS()
        {
            if (_eosInitialized)
            {
                return;
            }

            Debug.Log("[GameBootstrap] Initializing Epic Online Services");

            try
            {
                // Check if EOS Manager exists
                if (PlayEveryWare.EpicOnlineServices.EOSManager.Instance == null)
                {
                    Debug.LogError("[GameBootstrap] EOS Manager instance not found. Make sure EOSManager prefab is in the scene.");
                    return;
                }

                // Initialize EOS SDK
                PlayEveryWare.EpicOnlineServices.EOSManager.Instance.Initialize((success) =>
                {
                    if (success)
                    {
                        Debug.Log("[GameBootstrap] EOS SDK initialized successfully");

                        // Initialize EOS Platform
                        InitializeEOSPlatform();
                    }
                    else
                    {
                        Debug.LogError("[GameBootstrap] Failed to initialize EOS SDK");
                    }
                });
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameBootstrap] Error initializing EOS: {e.Message}");
            }
        }

        /// <summary>
        /// Initialize the EOS Platform
        /// </summary>
        private void InitializeEOSPlatform()
        {
            Debug.Log("[GameBootstrap] Initializing EOS Platform");

            try
            {
                // Get the EOS platform interface
                var eosPlatform = PlayEveryWare.EpicOnlineServices.EOSManager.Instance.GetEOSPlatformInterface();

                if (eosPlatform == null)
                {
                    Debug.LogError("[GameBootstrap] EOS platform interface not found");
                    return;
                }

                // Initialize auth with device ID for guest login
                InitializeAuth();

                _eosInitialized = true;
                Debug.Log("[GameBootstrap] Epic Online Services initialized");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameBootstrap] Error initializing EOS Platform: {e.Message}");
            }
        }

        /// <summary>
        /// Initialize authentication for EOS
        /// </summary>
        private void InitializeAuth()
        {
            Debug.Log("[GameBootstrap] Initializing EOS Authentication");

            // Get the EOS manager
            var eosManager = PlayEveryWare.EpicOnlineServices.EOSManager.Instance;

            // Check if already logged in
            if (eosManager.IsLoggedIn())
            {
                Debug.Log("[GameBootstrap] Already logged in to EOS");
                return;
            }

            // Set up auth login callback
            eosManager.SetLoggedInCallback((success, userId, error) =>
            {
                if (success)
                {
                    Debug.Log($"[GameBootstrap] Successfully logged in to EOS with user ID: {userId}");

                    // Initialize other EOS services after successful login
                    InitializeEOSServices();
                }
                else
                {
                    Debug.LogError($"[GameBootstrap] Failed to log in to EOS: {error}");
                }
            });

            // Login with device ID (for guest users)
            eosManager.StartLoginWithDeviceID("RecipeRage Guest", (success, error) =>
            {
                if (!success)
                {
                    Debug.LogError($"[GameBootstrap] Failed to start device ID login: {error}");
                }
            });
        }

        /// <summary>
        /// Initialize other EOS services after authentication
        /// </summary>
        private void InitializeEOSServices()
        {
            Debug.Log("[GameBootstrap] Initializing EOS Services");

            // Get the EOS manager
            var eosManager = PlayEveryWare.EpicOnlineServices.EOSManager.Instance;

            // Initialize P2P networking
            var p2pInterface = eosManager.GetEOSPlatformInterface().GetP2PInterface();
            if (p2pInterface != null)
            {
                Debug.Log("[GameBootstrap] EOS P2P interface initialized");
            }
            else
            {
                Debug.LogError("[GameBootstrap] Failed to initialize EOS P2P interface");
            }

            // Initialize sessions
            var sessionsInterface = eosManager.GetEOSPlatformInterface().GetSessionsInterface();
            if (sessionsInterface != null)
            {
                Debug.Log("[GameBootstrap] EOS Sessions interface initialized");
            }
            else
            {
                Debug.LogError("[GameBootstrap] Failed to initialize EOS Sessions interface");
            }

            // Initialize presence (for custom friend system)
            var presenceInterface = eosManager.GetEOSPlatformInterface().GetPresenceInterface();
            if (presenceInterface != null)
            {
                Debug.Log("[GameBootstrap] EOS Presence interface initialized");
            }
            else
            {
                Debug.LogError("[GameBootstrap] Failed to initialize EOS Presence interface");
            }
        }

        /// <summary>
        /// Initialize the input manager
        /// </summary>
        private void InitializeInputManager()
        {
            Debug.Log("[GameBootstrap] Initializing input manager");

            // Ensure the input manager exists
            InputManager inputManager = InputManager.Instance;

            // Register the input manager with the service locator
            ServiceLocator.Instance.Register<InputManager>(inputManager);

            Debug.Log("[GameBootstrap] Input manager initialized");
        }

        /// <summary>
        /// Initialize the network manager
        /// </summary>
        private void InitializeNetworkManager()
        {
            Debug.Log("[GameBootstrap] Initializing network manager");

            // Ensure the network manager exists
            NetworkManager networkManager = NetworkManager.Instance;

            // Register the network manager with the service locator
            ServiceLocator.Instance.Register<NetworkManager>(networkManager);

            Debug.Log("[GameBootstrap] Network manager initialized");
        }

        /// <summary>
        /// Initialize the game state manager
        /// </summary>
        private void InitializeGameStateManager()
        {
            Debug.Log("[GameBootstrap] Initializing game state manager");

            // Ensure the game state manager exists
            GameStateManager gameStateManager = GameStateManager.Instance;

            // Register the game state manager with the service locator
            ServiceLocator.Instance.Register<GameStateManager>(gameStateManager);

            // Create initial state (loading state)
            LoadingState loadingState = new LoadingState();

            // Subscribe to loading complete event
            loadingState.OnLoadingComplete += HandleLoadingComplete;

            // Initialize the game state manager with the loading state
            gameStateManager.Initialize(loadingState);

            Debug.Log("[GameBootstrap] Game state manager initialized");
        }

        /// <summary>
        /// Handle loading complete event
        /// </summary>
        private void HandleLoadingComplete()
        {
            Debug.Log("[GameBootstrap] Loading complete, transitioning to main menu");

            // Create main menu state
            MainMenuState mainMenuState = new MainMenuState();

            // Transition to main menu
            GameStateManager.Instance.ChangeState(mainMenuState);
        }
    }
}
