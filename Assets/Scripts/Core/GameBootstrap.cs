using System.Collections;
using System.Threading.Tasks;
using Core.Audio;
using Core.Characters;
using Core.GameFramework.State;
using Core.GameFramework.State.States;
using Core.GameModes;
using Core.Input;
using Core.Networking;
using Core.Patterns;
using Core.SaveSystem;

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
        [Header("Initialization Settings")]
        [SerializeField] private bool _initializeNetworking = true;
        [SerializeField] private bool _initializeGameState = true;
        [SerializeField] private bool _initializeUI = true;
        [SerializeField] private bool _initializeInput = true;
        [SerializeField] private bool _initializeGameMode = true;
        [SerializeField] private bool _initializeCharacters = true;
        [SerializeField] private bool _initializeScoring = true;
        [SerializeField] private bool _initializeOrderSystem = true;
        [SerializeField] private bool _initializeSaveSystem = true;
        [SerializeField] private bool _initializeAudioSystem = true;

        [Header("Initial State")]
        [SerializeField] private GameStateType _initialState = GameStateType.MainMenu;

        [Header("References")]
        [SerializeField] private GameObject _networkManagerPrefab;
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
        [SerializeField] private AudioDatabase _audioDatabase;

        [Header("Splash Screen Settings")]
        [SerializeField] private bool _showSplashScreens = true;

        // Initialization status
        private bool _isInitialized = false;

        // Component references
        private NetworkBootstrap _networkBootstrap;
        private GameStateManager _gameStateManager;
        private UIManager _uiManager;
        private InputManager _inputManager;
        private Core.UI.SplashScreen.SplashScreenManager _splashScreenManager;
        private Core.UI.Loading.LoadingScreenManager _loadingScreenManager;
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
        /// Initialize all game systems in the correct order.
        /// </summary>
        private IEnumerator InitializeGameSystems()
        {
            Debug.Log("[GameBootstrap] Starting game initialization...");

            // Initialize splash screen manager first if we're showing splash screens
            if (_showSplashScreens)
            {
                yield return StartCoroutine(InitializeSplashScreenManager());
                yield return StartCoroutine(InitializeLoadingScreenManager());

                // Show company splash screen
                if (_splashScreenManager != null)
                {
                    yield return _splashScreenManager.ShowCompanySplash().AsCoroutine();

                    // Show game logo splash screen
                    yield return _splashScreenManager.ShowGameLogoSplash().AsCoroutine();

                    // Show loading screen
                    if (_loadingScreenManager != null)
                    {
                        _loadingScreenManager.ShowLoadingScreen();
                    }
                }
            }

            // Initialize save system first
            if (_initializeSaveSystem)
            {
                if (_showSplashScreens && _loadingScreenManager != null)
                {
                    _loadingScreenManager.UpdateLoadingProgress("Initializing Save System...", 0.05f);
                }

                yield return StartCoroutine(InitializeSaveSystem());
            }

            // Initialize audio system early
            if (_initializeAudioSystem)
            {
                if (_showSplashScreens && _loadingScreenManager != null)
                {
                    _loadingScreenManager.UpdateLoadingProgress("Initializing Audio System...", 0.1f);
                }

                yield return StartCoroutine(InitializeAudioSystem());
            }

            // Initialize networking
            if (_initializeNetworking)
            {
                if (_showSplashScreens && _loadingScreenManager != null)
                {
                    _loadingScreenManager.UpdateLoadingProgress("Initializing Networking System...", 0.15f);
                }

                yield return StartCoroutine(InitializeNetworking());
            }

            // Initialize game state system
            if (_initializeGameState)
            {
                if (_showSplashScreens && _loadingScreenManager != null)
                {
                    _loadingScreenManager.UpdateLoadingProgress("Initializing Game State System...", 0.25f);
                }

                yield return StartCoroutine(InitializeGameState());
            }

            // Initialize UI system
            if (_initializeUI)
            {
                if (_showSplashScreens && _loadingScreenManager != null)
                {
                    _loadingScreenManager.UpdateLoadingProgress("Initializing UI System...", 0.35f);
                }

                yield return StartCoroutine(InitializeUI());
            }

            // Initialize input system
            if (_initializeInput)
            {
                if (_showSplashScreens && _loadingScreenManager != null)
                {
                    _loadingScreenManager.UpdateLoadingProgress("Initializing Input System...", 0.45f);
                }

                yield return StartCoroutine(InitializeInput());
            }

            // Initialize game mode system
            if (_initializeGameMode)
            {
                if (_showSplashScreens && _loadingScreenManager != null)
                {
                    _loadingScreenManager.UpdateLoadingProgress("Initializing Game Mode System...", 0.55f);
                }

                yield return StartCoroutine(InitializeGameMode());
            }

            // Initialize character system
            if (_initializeCharacters)
            {
                if (_showSplashScreens && _loadingScreenManager != null)
                {
                    _loadingScreenManager.UpdateLoadingProgress("Initializing Character System...", 0.65f);
                }

                yield return StartCoroutine(InitializeCharacters());
            }

            // Initialize scoring system
            if (_initializeScoring)
            {
                if (_showSplashScreens && _loadingScreenManager != null)
                {
                    _loadingScreenManager.UpdateLoadingProgress("Initializing Scoring System...", 0.75f);
                }

                yield return StartCoroutine(InitializeScoring());
            }

            // Initialize order system
            if (_initializeOrderSystem)
            {
                if (_showSplashScreens && _loadingScreenManager != null)
                {
                    _loadingScreenManager.UpdateLoadingProgress("Initializing Order System...", 0.85f);
                }

                yield return StartCoroutine(InitializeOrderSystem());
            }

            // Final loading progress
            if (_showSplashScreens && _loadingScreenManager != null)
            {
                _loadingScreenManager.UpdateLoadingProgress("Finalizing...", 0.95f);
            }

            // Set initial game state
            if (_initializeGameState && _gameStateManager != null)
            {
                SetInitialGameState();
            }

            // Hide loading screen if shown
            if (_showSplashScreens && _loadingScreenManager != null)
            {
                yield return _loadingScreenManager.HideLoadingScreen().AsCoroutine();
            }

            // Mark as initialized
            _isInitialized = true;

            Debug.Log("[GameBootstrap] Game initialization complete!");
        }

        /// <summary>
        /// Initialize the networking system.
        /// </summary>
        private IEnumerator InitializeNetworking()
        {
            Debug.Log("[GameBootstrap] Initializing networking system...");

            // Check if NetworkBootstrap already exists
            _networkBootstrap = FindFirstObjectByType<NetworkBootstrap>();

            if (_networkBootstrap == null && _networkManagerPrefab != null)
            {
                // Instantiate the network manager prefab
                var networkManagerObj = Instantiate(_networkManagerPrefab);
                networkManagerObj.name = "NetworkManager";
                _networkBootstrap = networkManagerObj.GetComponent<NetworkBootstrap>();

                if (_networkBootstrap == null)
                {
                    _networkBootstrap = networkManagerObj.AddComponent<NetworkBootstrap>();
                }
            }
            else if (_networkBootstrap == null)
            {
                // Create a new GameObject for the network manager
                var networkManagerObj = new GameObject("NetworkManager");
                _networkBootstrap = networkManagerObj.AddComponent<NetworkBootstrap>();
            }

            // Wait for networking to initialize
            while (_networkBootstrap != null && !_networkBootstrap.IsInitialized)
            {
                yield return new WaitForSeconds(0.1f);
            }

            Debug.Log("[GameBootstrap] Networking system initialized.");
        }

        /// <summary>
        /// Initialize the game state system.
        /// </summary>
        private IEnumerator InitializeGameState()
        {
            Debug.Log("[GameBootstrap] Initializing game state system...");

            // Check if GameStateManager already exists
            _gameStateManager = GameStateManager.Instance;

            if (_gameStateManager == null && _gameStateManagerPrefab != null)
            {
                // Instantiate the game state manager prefab
                var gameStateManagerObj = Instantiate(_gameStateManagerPrefab);
                gameStateManagerObj.name = "GameStateManager";

                // Wait a frame for the singleton to initialize
                yield return null;

                _gameStateManager = GameStateManager.Instance;
            }

            Debug.Log("[GameBootstrap] Game state system initialized.");
        }

        /// <summary>
        /// Initialize the UI system.
        /// </summary>
        private IEnumerator InitializeUI()
        {
            Debug.Log("[GameBootstrap] Initializing UI system...");

            // Check if UIManager already exists
            _uiManager = FindFirstObjectByType<UIManager>();

            if (_uiManager == null && _uiManagerPrefab != null)
            {
                // Instantiate the UI manager prefab
                var uiManagerObj = Instantiate(_uiManagerPrefab);
                uiManagerObj.name = "UIManager";
                _uiManager = uiManagerObj.GetComponent<UIManager>();
            }
            else if (_uiManager == null)
            {
                // Create a new GameObject for the UI manager
                var uiManagerObj = new GameObject("UIManager");
                _uiManager = uiManagerObj.AddComponent<UIManager>();
            }

            // Wait a frame for the UI to initialize
            yield return null;

            Debug.Log("[GameBootstrap] UI system initialized.");
        }

        /// <summary>
        /// Initialize the input system.
        /// </summary>
        private IEnumerator InitializeInput()
        {
            Debug.Log("[GameBootstrap] Initializing input system...");

            // Check if InputManager already exists
            _inputManager = FindFirstObjectByType<InputManager>();

            if (_inputManager == null && _inputManagerPrefab != null)
            {
                // Instantiate the input manager prefab
                var inputManagerObj = Instantiate(_inputManagerPrefab);
                inputManagerObj.name = "InputManager";
                _inputManager = inputManagerObj.GetComponent<InputManager>();
            }
            else if (_inputManager == null)
            {
                // Create a new GameObject for the input manager
                var inputManagerObj = new GameObject("InputManager");
                _inputManager = inputManagerObj.AddComponent<InputManager>();
            }

            // Wait a frame for the input system to initialize
            yield return null;

            Debug.Log("[GameBootstrap] Input system initialized.");
        }

        /// <summary>
        /// Initialize the game mode system.
        /// </summary>
        private IEnumerator InitializeGameMode()
        {
            Debug.Log("[GameBootstrap] Initializing game mode system...");

            // Check if GameModeManager already exists
            _gameModeManager = FindFirstObjectByType<GameModeManager>();

            if (_gameModeManager == null && _gameModeManagerPrefab != null)
            {
                // Instantiate the game mode manager prefab
                var gameModeManagerObj = Instantiate(_gameModeManagerPrefab);
                gameModeManagerObj.name = "GameModeManager";
                _gameModeManager = gameModeManagerObj.GetComponent<GameModeManager>();
            }
            else if (_gameModeManager == null)
            {
                // Create a new GameObject for the game mode manager
                var gameModeManagerObj = new GameObject("GameModeManager");
                _gameModeManager = gameModeManagerObj.AddComponent<GameModeManager>();
            }

            // Wait a frame for the game mode system to initialize
            yield return null;

            Debug.Log("[GameBootstrap] Game mode system initialized.");
        }

        /// <summary>
        /// Initialize the character system.
        /// </summary>
        private IEnumerator InitializeCharacters()
        {
            Debug.Log("[GameBootstrap] Initializing character system...");

            // Check if CharacterManager already exists
            _characterManager = FindFirstObjectByType<CharacterManager>();

            if (_characterManager == null && _characterManagerPrefab != null)
            {
                // Instantiate the character manager prefab
                var characterManagerObj = Instantiate(_characterManagerPrefab);
                characterManagerObj.name = "CharacterManager";
                _characterManager = characterManagerObj.GetComponent<CharacterManager>();
            }
            else if (_characterManager == null)
            {
                // Create a new GameObject for the character manager
                var characterManagerObj = new GameObject("CharacterManager");
                _characterManager = characterManagerObj.AddComponent<CharacterManager>();
            }

            // Wait a frame for the character system to initialize
            yield return null;

            Debug.Log("[GameBootstrap] Character system initialized.");
        }

        /// <summary>
        /// Initialize the scoring system.
        /// </summary>
        private IEnumerator InitializeScoring()
        {
            Debug.Log("[GameBootstrap] Initializing scoring system...");

            // Check if ScoreManager already exists
            _scoreManager = FindFirstObjectByType<ScoreManager>();

            if (_scoreManager == null && _scoreManagerPrefab != null)
            {
                // Instantiate the score manager prefab
                var scoreManagerObj = Instantiate(_scoreManagerPrefab);
                scoreManagerObj.name = "ScoreManager";
                _scoreManager = scoreManagerObj.GetComponent<ScoreManager>();
            }
            else if (_scoreManager == null)
            {
                // Create a new GameObject for the score manager
                var scoreManagerObj = new GameObject("ScoreManager");
                _scoreManager = scoreManagerObj.AddComponent<ScoreManager>();
            }

            // Wait a frame for the scoring system to initialize
            yield return null;

            Debug.Log("[GameBootstrap] Scoring system initialized.");
        }

        /// <summary>
        /// Initialize the order system.
        /// </summary>
        private IEnumerator InitializeOrderSystem()
        {
            Debug.Log("[GameBootstrap] Initializing order system...");

            // Check if OrderManager already exists
            _orderManager = FindFirstObjectByType<OrderManager>();

            if (_orderManager == null && _orderManagerPrefab != null)
            {
                // Instantiate the order manager prefab
                var orderManagerObj = Instantiate(_orderManagerPrefab);
                orderManagerObj.name = "OrderManager";
                _orderManager = orderManagerObj.GetComponent<OrderManager>();
            }
            else if (_orderManager == null)
            {
                // Create a new GameObject for the order manager
                var orderManagerObj = new GameObject("OrderManager");
                _orderManager = orderManagerObj.AddComponent<OrderManager>();
            }

            // Wait a frame for the order system to initialize
            yield return null;

            Debug.Log("[GameBootstrap] Order system initialized.");
        }

        /// <summary>
        /// Initialize the save system.
        /// </summary>
        private IEnumerator InitializeSaveSystem()
        {
            Debug.Log("[GameBootstrap] Initializing save system...");

            // Check if SaveManager already exists
            _saveManager = SaveManager.Instance;

            if (_saveManager == null && _saveManagerPrefab != null)
            {
                // Instantiate the save manager prefab
                var saveManagerObj = Instantiate(_saveManagerPrefab);
                saveManagerObj.name = "SaveManager";

                // Wait a frame for the singleton to initialize
                yield return null;

                _saveManager = SaveManager.Instance;
            }
            else if (_saveManager == null)
            {
                // Create a new GameObject for the save manager
                var saveManagerObj = new GameObject("SaveManager");
                _saveManager = saveManagerObj.AddComponent<SaveManager>();

                // Wait a frame for the singleton to initialize
                yield return null;
            }

            // Import settings from PlayerPrefs if needed
            if (_saveManager != null)
            {
                _saveManager.ImportFromPlayerPrefs();
            }

            Debug.Log("[GameBootstrap] Save system initialized.");
        }

        /// <summary>
        /// Initialize the audio system.
        /// </summary>
        private IEnumerator InitializeAudioSystem()
        {
            Debug.Log("[GameBootstrap] Initializing audio system...");

            // Check if AudioManager already exists
            _audioManager = AudioManager.Instance;

            if (_audioManager == null && _audioManagerPrefab != null)
            {
                // Instantiate the audio manager prefab
                var audioManagerObj = Instantiate(_audioManagerPrefab);
                audioManagerObj.name = "AudioManager";

                // Wait a frame for the singleton to initialize
                yield return null;

                _audioManager = AudioManager.Instance;
            }
            else if (_audioManager == null)
            {
                // Create a new GameObject for the audio manager
                var audioManagerObj = new GameObject("AudioManager");
                _audioManager = audioManagerObj.AddComponent<AudioManager>();

                // Wait a frame for the singleton to initialize
                yield return null;
            }

            Debug.Log("[GameBootstrap] Audio system initialized.");
        }

        /// <summary>
        /// Initialize the splash screen manager.
        /// </summary>
        private IEnumerator InitializeSplashScreenManager()
        {
            Debug.Log("[GameBootstrap] Initializing splash screen manager...");

            // Check if SplashScreenManager already exists
            _splashScreenManager = FindFirstObjectByType<Core.UI.SplashScreen.SplashScreenManager>();

            if (_splashScreenManager == null && _splashScreenManagerPrefab != null)
            {
                // Instantiate the splash screen manager prefab
                var splashScreenManagerObj = Instantiate(_splashScreenManagerPrefab);
                splashScreenManagerObj.name = "SplashScreenManager";

                // Wait a frame for the singleton to initialize
                yield return null;

                _splashScreenManager = FindFirstObjectByType<Core.UI.SplashScreen.SplashScreenManager>();
            }
            else if (_splashScreenManager == null)
            {
                // Create a new GameObject for the splash screen manager
                var splashScreenManagerObj = new GameObject("SplashScreenManager");
                _splashScreenManager = splashScreenManagerObj.AddComponent<Core.UI.SplashScreen.SplashScreenManager>();

                // Wait a frame for the singleton to initialize
                yield return null;
            }

            Debug.Log("[GameBootstrap] Splash screen manager initialized.");
        }

        /// <summary>
        /// Initialize the loading screen manager.
        /// </summary>
        private IEnumerator InitializeLoadingScreenManager()
        {
            Debug.Log("[GameBootstrap] Initializing loading screen manager...");

            // Check if LoadingScreenManager already exists
            _loadingScreenManager = Core.UI.Loading.LoadingScreenManager.Instance;

            if (_loadingScreenManager == null && _loadingScreenManagerPrefab != null)
            {
                // Instantiate the loading screen manager prefab
                var loadingScreenManagerObj = Instantiate(_loadingScreenManagerPrefab);
                loadingScreenManagerObj.name = "LoadingScreenManager";

                // Wait a frame for the singleton to initialize
                yield return null;

                _loadingScreenManager = Core.UI.Loading.LoadingScreenManager.Instance;
            }
            else if (_loadingScreenManager == null)
            {
                // Create a new GameObject for the loading screen manager
                var loadingScreenManagerObj = new GameObject("LoadingScreenManager");
                _loadingScreenManager = loadingScreenManagerObj.AddComponent<Core.UI.Loading.LoadingScreenManager>();

                // Wait a frame for the singleton to initialize
                yield return null;
            }

            Debug.Log("[GameBootstrap] Loading screen manager initialized.");
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
            if (initialState != null)
            {
                _gameStateManager.Initialize(initialState);
            }
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
