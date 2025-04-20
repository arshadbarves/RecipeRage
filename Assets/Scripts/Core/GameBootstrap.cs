using System.Collections;
using Core.AudioSystem;
using Core.Characters;
using Core.GameFramework.State;
using Core.GameFramework.State.States;
using Core.GameModes;
using Core.Input;
using Core.Networking;
using Core.Patterns;
using Core.SaveSystem;
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
        [Header("Initialization Settings")]
        [SerializeField] private bool _initializeSaveSystem = true;
        [SerializeField] private bool _initializeAudioSystem = true;
        [SerializeField] private bool _initializeNetworking = true;
        [SerializeField] private bool _initializeGameState = true;
        [SerializeField] private bool _initializeUI = true;
        [SerializeField] private bool _initializeInput = true;
        [SerializeField] private bool _initializeGameMode = true;
        [SerializeField] private bool _initializeCharacters = true;
        [SerializeField] private bool _initializeScoring = true;
        [SerializeField] private bool _initializeOrderSystem = true;

        [Header("Initial State")]
        [SerializeField] private GameStateType _initialState = GameStateType.MainMenu;

        [Header("References")]
        [SerializeField] private GameObject _saveManagerPrefab;
        [SerializeField] private GameObject _audioManagerPrefab;
        [SerializeField] private GameObject _networkManagerPrefab;
        [SerializeField] private GameObject _gameStateManagerPrefab;
        [SerializeField] private GameObject _uiManagerPrefab;
        [SerializeField] private GameObject _inputManagerPrefab;
        [SerializeField] private GameObject _gameModeManagerPrefab;
        [SerializeField] private GameObject _characterManagerPrefab;
        [SerializeField] private GameObject _scoreManagerPrefab;
        [SerializeField] private GameObject _orderManagerPrefab;
        private CharacterManager _characterManager;
        private GameModeManager _gameModeManager;
        private GameStateManager _gameStateManager;
        private InputManager _inputManager;

        // Initialization status
        private bool _isInitialized;

        // Component references
        private SaveManager _saveManager;
        private AudioManager _audioManager;
        private NetworkBootstrap _networkBootstrap;
        private OrderManager _orderManager;
        private ScoreManager _scoreManager;
        private UIManager _uiManager;

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

            // Initialize save system first if enabled
            if (_initializeSaveSystem)
            {
                yield return StartCoroutine(InitializeSaveSystem());
            }

            // Initialize audio system
            if (_initializeAudioSystem)
            {
                yield return StartCoroutine(InitializeAudioSystem());
            }

            // Initialize networking
            if (_initializeNetworking)
            {
                yield return StartCoroutine(InitializeNetworking());
            }

            // Initialize game state system
            if (_initializeGameState)
            {
                yield return StartCoroutine(InitializeGameState());
            }

            // Initialize UI system
            if (_initializeUI)
            {
                yield return StartCoroutine(InitializeUI());
            }

            // Initialize input system
            if (_initializeInput)
            {
                yield return StartCoroutine(InitializeInput());
            }

            // Initialize game mode system
            if (_initializeGameMode)
            {
                yield return StartCoroutine(InitializeGameMode());
            }

            // Initialize character system
            if (_initializeCharacters)
            {
                yield return StartCoroutine(InitializeCharacters());
            }

            // Initialize scoring system
            if (_initializeScoring)
            {
                yield return StartCoroutine(InitializeScoring());
            }

            // Initialize order system
            if (_initializeOrderSystem)
            {
                yield return StartCoroutine(InitializeOrderSystem());
            }

            // Set initial game state
            if (_initializeGameState && _gameStateManager != null)
            {
                SetInitialGameState();
            }

            // Mark as initialized
            _isInitialized = true;

            Debug.Log("[GameBootstrap] Game initialization complete!");
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
                GameObject saveManagerObj = Instantiate(_saveManagerPrefab);
                saveManagerObj.name = "SaveManager";
                _saveManager = saveManagerObj.GetComponent<SaveManager>();
            }
            else if (_saveManager == null)
            {
                // Create a new GameObject for the save manager
                GameObject saveManagerObj = new GameObject("SaveManager");
                _saveManager = saveManagerObj.AddComponent<SaveManager>();
            }

            // Wait a frame for the save system to initialize
            yield return null;

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
                GameObject audioManagerObj = Instantiate(_audioManagerPrefab);
                audioManagerObj.name = "AudioManager";
                _audioManager = audioManagerObj.GetComponent<AudioManager>();
            }
            else if (_audioManager == null)
            {
                // Create a new GameObject for the audio manager
                GameObject audioManagerObj = new GameObject("AudioManager");
                _audioManager = audioManagerObj.AddComponent<AudioManager>();
            }

            // Wait a frame for the audio system to initialize
            yield return null;

            Debug.Log("[GameBootstrap] Audio system initialized.");
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
                GameObject networkManagerObj = Instantiate(_networkManagerPrefab);
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
                GameObject networkManagerObj = new GameObject("NetworkManager");
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
                GameObject gameStateManagerObj = Instantiate(_gameStateManagerPrefab);
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
                GameObject uiManagerObj = Instantiate(_uiManagerPrefab);
                uiManagerObj.name = "UIManager";
                _uiManager = uiManagerObj.GetComponent<UIManager>();
            }
            else if (_uiManager == null)
            {
                // Create a new GameObject for the UI manager
                GameObject uiManagerObj = new GameObject("UIManager");
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
                GameObject inputManagerObj = Instantiate(_inputManagerPrefab);
                inputManagerObj.name = "InputManager";
                _inputManager = inputManagerObj.GetComponent<InputManager>();
            }
            else if (_inputManager == null)
            {
                // Create a new GameObject for the input manager
                GameObject inputManagerObj = new GameObject("InputManager");
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
                GameObject gameModeManagerObj = Instantiate(_gameModeManagerPrefab);
                gameModeManagerObj.name = "GameModeManager";
                _gameModeManager = gameModeManagerObj.GetComponent<GameModeManager>();
            }
            else if (_gameModeManager == null)
            {
                // Create a new GameObject for the game mode manager
                GameObject gameModeManagerObj = new GameObject("GameModeManager");
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
                GameObject characterManagerObj = Instantiate(_characterManagerPrefab);
                characterManagerObj.name = "CharacterManager";
                _characterManager = characterManagerObj.GetComponent<CharacterManager>();
            }
            else if (_characterManager == null)
            {
                // Create a new GameObject for the character manager
                GameObject characterManagerObj = new GameObject("CharacterManager");
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
                GameObject scoreManagerObj = Instantiate(_scoreManagerPrefab);
                scoreManagerObj.name = "ScoreManager";
                _scoreManager = scoreManagerObj.GetComponent<ScoreManager>();
            }
            else if (_scoreManager == null)
            {
                // Create a new GameObject for the score manager
                GameObject scoreManagerObj = new GameObject("ScoreManager");
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
                GameObject orderManagerObj = Instantiate(_orderManagerPrefab);
                orderManagerObj.name = "OrderManager";
                _orderManager = orderManagerObj.GetComponent<OrderManager>();
            }
            else if (_orderManager == null)
            {
                // Create a new GameObject for the order manager
                GameObject orderManagerObj = new GameObject("OrderManager");
                _orderManager = orderManagerObj.AddComponent<OrderManager>();
            }

            // Wait a frame for the order system to initialize
            yield return null;

            Debug.Log("[GameBootstrap] Order system initialized.");
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
    }
}