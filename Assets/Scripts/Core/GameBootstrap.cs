using System.Collections;
using Core.Audio;
using Core.Characters;
using Core.GameFramework.State;
using Core.GameFramework.State.States;
using Core.GameModes;
using Core.Input;
using Core.Networking;
using Core.Patterns;
using Core.SaveSystem;
using Core.UI.Loading;
using Core.UI.SplashScreen;
using Core.Utilities;
using Gameplay.Cooking;
using Gameplay.Scoring;
using UI;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Main bootstrap class for initializing all game systems.
    /// This is the entry point for the game and should be placed in the initial scene.
    /// </summary>
    public class GameBootstrap : MonoBehaviourSingleton<GameBootstrap>
    {
        [Header("Initial State")] [SerializeField]
        private GameStateType _initialState = GameStateType.MainMenu;

        [Header("Manager Prefabs")] [SerializeField]
        private GameObject _networkManagerPrefab;

        [SerializeField] private GameObject _gameStateManagerPrefab;
        [SerializeField] private GameObject _uiManagerPrefab;
        [SerializeField] private GameObject _inputManagerPrefab;
        [SerializeField] private GameObject _gameModeManagerPrefab;
        [SerializeField] private GameObject _characterManagerPrefab;
        [SerializeField] private GameObject _scoreManagerPrefab;
        [SerializeField] private GameObject _orderManagerPrefab;
        [SerializeField] private GameObject _saveManagerPrefab;
        [SerializeField] private GameObject _audioManagerPrefab;
        [SerializeField] private GameObject _splashScreenManagerPrefab;
        [SerializeField] private GameObject _loadingScreenManagerPrefab;


        [Header("Splash Screen Settings")] [SerializeField]
        private bool _showSplashScreens = true;

        // Initialization status
        private bool _isInitialized = false;

        // Component references
        private NetworkBootstrap _networkBootstrap;
        private GameStateManager _gameStateManager;
        private UIManager _uiManager;
        private InputManager _inputManager;
        private SplashScreenManager _splashScreenManager;
        private LoadingScreenManager _loadingScreenManager;
        private GameModeManager _gameModeManager;
        private CharacterManager _characterManager;
        private ScoreManager _scoreManager;
        private OrderManager _orderManager;
        private SaveManager _saveManager;
        private AudioManager _audioManager;

        /// <summary>
        /// Initialize all game systems.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Start initialization
            StartCoroutine(InitializeGameSystems());
        }

        /// <summary>
        /// Helper method to update loading progress if splash screens are enabled.
        /// </summary>
        /// <param name="message">The loading message to display</param>
        /// <param name="progress">Progress value between 0 and 1</param>
        private void UpdateLoadingProgress(string message, float progress)
        {
            if (_showSplashScreens && _loadingScreenManager != null)
            {
                _loadingScreenManager.UpdateLoadingProgress(message, progress);
            }
        }

        /// <summary>
        /// Initialize all game systems in the correct order.
        /// </summary>
        private IEnumerator InitializeGameSystems()
        {
            Debug.Log("[GameBootstrap] Starting game initialization...");

            // Always initialize splash and loading screen managers
            yield return StartCoroutine(InitializeUIManagers());

            // Show splash and loading screens if enabled
            if (_showSplashScreens && _splashScreenManager != null)
            {
                _splashScreenManager.ShowCompanySplash();
                yield return StartCoroutine(WaitForSplashCompletion());
            }

            if (_loadingScreenManager != null)
                _loadingScreenManager.ShowLoadingScreen();

            // Initialize all core systems
            yield return StartCoroutine(InitializeAllSystems());

            // Final loading progress
            UpdateLoadingProgress("Finalizing...", 0.95f);

            // Set initial game state
            if (_gameStateManager != null)
                SetInitialGameState();

            // Hide loading screen if shown
            if (_showSplashScreens && _loadingScreenManager != null)
            {
                _loadingScreenManager.HideLoadingScreen();
            }

            _isInitialized = true;
            Debug.Log("[GameBootstrap] Game initialization complete!");
        }
        
        /// <summary>
        /// System initialization information
        /// </summary>
        private class SystemInitInfo
        {
            public string Name { get; }
            public float Progress { get; }
            public GameObject Prefab { get; }
            public System.Action<GameObject> OnInitialized { get; }
            public System.Func<bool> IsReady { get; }

            public SystemInitInfo(string name, float progress, GameObject prefab,
                System.Action<GameObject> onInitialized, System.Func<bool> isReady = null)
            {
                Name = name;
                Progress = progress;
                Prefab = prefab;
                OnInitialized = onInitialized;
                IsReady = isReady ?? (() => true); // Default to always ready
            }
        }

        /// <summary>
        /// Initialize UI managers (splash and loading screen)
        /// </summary>
        private IEnumerator InitializeUIManagers()
        {
            var uiManagers = new[]
            {
                new SystemInitInfo("SplashScreenManager", 0f, _splashScreenManagerPrefab,
                    (obj) => _splashScreenManager = obj.GetComponent<Core.UI.SplashScreen.SplashScreenManager>()),
                new SystemInitInfo("LoadingScreenManager", 0f, _loadingScreenManagerPrefab,
                    (obj) => _loadingScreenManager = Core.UI.Loading.LoadingScreenManager.Instance)
            };

            foreach (var manager in uiManagers)
            {
                if (manager.Prefab != null)
                {
                    yield return StartCoroutine(InitializeSystem(manager));
                }
            }
        }

        /// <summary>
        /// Initialize all systems using a generic approach
        /// </summary>
        private IEnumerator InitializeAllSystems()
        {
            // Define all systems to initialize
            var systems = new[]
            {
                new SystemInitInfo("Save System", 0.05f, _saveManagerPrefab, (obj) =>
                {
                    _saveManager = SaveManager.Instance;
                    _saveManager?.ImportFromPlayerPrefs();
                }),
                new SystemInitInfo("Audio System", 0.1f, _audioManagerPrefab,
                    (obj) => _audioManager = AudioManager.Instance),
                new SystemInitInfo("Networking System", 0.15f, _networkManagerPrefab,
                    (obj) => _networkBootstrap = obj.GetComponent<NetworkBootstrap>(),
                    () => _networkBootstrap?.IsInitialized ?? true),
                new SystemInitInfo("Game State System", 0.25f, _gameStateManagerPrefab,
                    (obj) => _gameStateManager = GameStateManager.Instance),
                new SystemInitInfo("UI System", 0.35f, _uiManagerPrefab,
                    (obj) => _uiManager = obj.GetComponent<UIManager>()),
                new SystemInitInfo("Input System", 0.45f, _inputManagerPrefab,
                    (obj) => _inputManager = obj.GetComponent<InputManager>()),
                new SystemInitInfo("Game Mode System", 0.55f, _gameModeManagerPrefab,
                    (obj) => _gameModeManager = obj.GetComponent<GameModeManager>()),
                new SystemInitInfo("Character System", 0.65f, _characterManagerPrefab,
                    (obj) => _characterManager = obj.GetComponent<CharacterManager>()),
                new SystemInitInfo("Scoring System", 0.75f, _scoreManagerPrefab,
                    (obj) => _scoreManager = obj.GetComponent<ScoreManager>()),
                new SystemInitInfo("Order System", 0.85f, _orderManagerPrefab,
                    (obj) => _orderManager = obj.GetComponent<OrderManager>())
            };

            // Initialize all systems
            foreach (var system in systems)
            {
                UpdateLoadingProgress($"Initializing {system.Name}...", system.Progress);
                yield return StartCoroutine(InitializeSystem(system));
            }
        }

        /// <summary>
        /// Generic system initialization method
        /// </summary>
        private IEnumerator InitializeSystem(SystemInitInfo systemInfo)
        {
            Debug.Log($"[GameBootstrap] Initializing {systemInfo.Name.ToLower()}...");

            if (systemInfo.Prefab == null)
            {
                Debug.LogError(
                    $"[GameBootstrap] {systemInfo.Name} prefab is not assigned! Please assign the prefab in the inspector.");
                yield break;
            }

            // Instantiate the system
            var systemObj = Instantiate(systemInfo.Prefab);
            systemObj.name = systemInfo.Name.Replace(" ", "");

            // Wait a frame for initialization
            yield return null;

            // Call the initialization callback
            systemInfo.OnInitialized?.Invoke(systemObj);

            // Wait for system to be ready (if it has async initialization)
            while (!systemInfo.IsReady())
            {
                yield return new WaitForSeconds(0.1f);
            }

            Debug.Log($"[GameBootstrap] {systemInfo.Name} initialized.");
        }


        /// <summary>
        /// Set the initial game state.
        /// </summary>
        private void SetInitialGameState()
        {
            Debug.Log($"[GameBootstrap] Setting initial game state to {_initialState}");

            // Create the appropriate state based on the enum
            IState initialState = null;

            switch (_initialState)
            {
                case GameStateType.MainMenu:
                    initialState = new MainMenuState();
                    break;
                case GameStateType.Lobby:
                    initialState = new LobbyState();
                    break;
                case GameStateType.Game:
                    initialState = new GameplayState();
                    break;
                case GameStateType.GameOver:
                    initialState = new GameOverState();
                    break;
                default:
                    initialState = new MainMenuState();
                    break;
            }

            // Initialize the state machine with the initial state
            _gameStateManager.Initialize(initialState);
        }

        /// <summary>
        /// Wait for splash screen completion using the event system.
        /// </summary>
        private IEnumerator WaitForSplashCompletion()
        {
            bool splashCompleted = false;
            
            // Create callback reference for proper unsubscription
            System.Action onComplete = () => splashCompleted = true;
            
            // Subscribe to the completion event
            _splashScreenManager.OnSplashComplete += onComplete;
            
            // Wait for completion
            while (!splashCompleted)
            {
                yield return null;
            }
            
            // Unsubscribe from the event
            _splashScreenManager.OnSplashComplete -= onComplete;
        }

        /// <summary>
        /// Enum for the different game states.
        /// </summary>
        public enum GameStateType
        {
            MainMenu,
            Lobby,
            Game,
            GameOver
        }
    }
}