using RecipeRage.Core.GameFramework.State;
using RecipeRage.Core.GameFramework.State.States;
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
            
            // TODO: Initialize other core systems (logging, input, etc.)
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
            
            // TODO: Initialize EOS SDK
            
            _eosInitialized = true;
            Debug.Log("[GameBootstrap] Epic Online Services initialized");
        }

        /// <summary>
        /// Initialize the game state manager
        /// </summary>
        private void InitializeGameStateManager()
        {
            Debug.Log("[GameBootstrap] Initializing game state manager");
            
            // Ensure the game state manager exists
            GameStateManager gameStateManager = GameStateManager.Instance;
            
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
