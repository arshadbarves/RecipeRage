using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Audio;
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
using UI.UISystem;
using UI.UISystem.Screens;
using UnityEngine;

namespace Core
{
    public class GameBootstrap : MonoBehaviourSingleton<GameBootstrap>
    {
        [Header("Configuration")] [SerializeField]
        private Configuration.BootstrapConfiguration _bootstrapConfig;

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

        public static event Action<string, float> OnInitializationProgress;
        public static event Action<string> OnSystemInitialized;
        public static event Action OnInitializationComplete;
        public static event Action<string> OnInitializationError;

        private readonly Dictionary<Type, object> _managers = new Dictionary<Type, object>();
        private bool _isInitialized;
        private Coroutine _initializationCoroutine;

        protected override void Awake()
        {
            base.Awake();
            InitializeUIManager();
        }

        private void Start()
        {
            if (_initializationCoroutine == null)
                _initializationCoroutine = StartCoroutine(InitializeGameSystemsAsync());
        }

        protected override void OnDestroy()
        {
            if (_initializationCoroutine != null)
            {
                StopCoroutine(_initializationCoroutine);
                _initializationCoroutine = null;
            }

            base.OnDestroy();
        }

        private void UpdateProgress(string message, float progress)
        {
            OnInitializationProgress?.Invoke(message, progress);
            var uiManager = GetManager<UIManager>();
            LoadingScreen loadingScreen = uiManager?.GetScreen<LoadingScreen>();
            loadingScreen?.UpdateProgress(message, progress);
        }

        private T GetManager<T>() where T : class
        {
            return _managers.TryGetValue(typeof(T), out var manager) ? manager as T : null;
        }

        private IEnumerator InitializeGameSystemsAsync()
        {
            yield return StartCoroutine(HandleSplashScreenAsync());
            yield return StartCoroutine(InitializeAllSystemsAsync());

            if (ValidateSystemHealth())
            {
                SetInitialGameState();
                OnInitializationComplete?.Invoke();
                _isInitialized = true;
            }
            else
            {
                Debug.LogError("[GameBootstrap] Critical systems failed validation");
                OnInitializationError?.Invoke("Critical systems failed validation");
            }
        }

        private IEnumerator HandleSplashScreenAsync()
        {
            var uiManager = GetManager<UIManager>();

            if (_bootstrapConfig == null || !_bootstrapConfig.showSplashScreens || uiManager == null)
                yield break;

            // Show splash screen
            uiManager.ShowScreen(UIScreenType.Splash, true, false);

            // Wait for splash duration
            yield return new WaitForSeconds(_bootstrapConfig.companySplashDuration);

            // Hide splash screen (no extra delay needed - animation handles it)
            uiManager.HideScreen(UIScreenType.Splash, true);
        }

        private readonly struct SystemInitInfo
        {
            public readonly string Name;
            public readonly float Progress;
            public readonly GameObject Prefab;
            public readonly Action<GameObject> OnInitialized;
            public readonly Func<bool> IsReady;

            public SystemInitInfo(string name, float progress, GameObject prefab,
                Action<GameObject> onInitialized, Func<bool> isReady = null)
            {
                Name = name;
                Progress = progress;
                Prefab = prefab;
                OnInitialized = onInitialized;
                IsReady = isReady ?? (() => true);
            }
        }

        private void InitializeUIManager()
        {
            InitializeSystemSync(new SystemInitInfo("UIManager", 0f, _uiManagerPrefab,
                obj => { _managers[typeof(UIManager)] = UIManager.Instance; }));
        }

        private void InitializeSystemSync(SystemInitInfo systemInfo)
        {
            if (systemInfo.Prefab == null)
            {
                Debug.LogError($"[GameBootstrap] {systemInfo.Name} prefab not assigned");
                return;
            }

            var systemObj = Instantiate(systemInfo.Prefab);
            systemObj.name = systemInfo.Name.Replace(" ", "");
            systemInfo.OnInitialized?.Invoke(systemObj);

            if (!systemInfo.IsReady())
                Debug.LogWarning($"[GameBootstrap] {systemInfo.Name} not ready after initialization");
        }

        private IEnumerator InitializeAllSystemsAsync()
        {
            var uiManager = GetManager<UIManager>();
            uiManager?.ShowScreen(UIScreenType.Loading, true, false);

            var systems = new SystemInitInfo[]
            {
                new("Save System", 0.1f, _saveManagerPrefab, obj =>
                {
                    var manager = SaveManager.Instance;
                    if (manager == null)
                        Debug.LogError("[GameBootstrap] SaveManager instance missing after prefab instantiation");
                    _managers[typeof(SaveManager)] = manager;
                    manager?.ImportFromPlayerPrefs();
                }),
                new("Audio System", 0.2f, _audioManagerPrefab,
                    obj =>
                    {
                        var manager = AudioManager.Instance;
                        if (manager == null)
                            Debug.LogError("[GameBootstrap] AudioManager instance missing after prefab instantiation");
                        _managers[typeof(AudioManager)] = manager;
                    }),
                new("Networking System", 0.3f, _networkManagerPrefab, obj =>
                {
                    var bootstrap = obj.GetComponent<NetworkBootstrap>();
                    if (bootstrap == null)
                        Debug.LogError(
                            "[GameBootstrap] NetworkBootstrap component missing on Networking System prefab");
                    _managers[typeof(NetworkBootstrap)] = bootstrap;
                }, () => GetManager<NetworkBootstrap>()?.IsInitialized ?? true),
                new("Game State System", 0.4f, _gameStateManagerPrefab,
                    obj =>
                    {
                        var manager = GameStateManager.Instance;
                        if (manager == null)
                            Debug.LogError(
                                "[GameBootstrap] GameStateManager instance missing after prefab instantiation");
                        _managers[typeof(GameStateManager)] = manager;
                    }),
                // UI System already initialized in Awake, just validate
                new("UI System Validation", 0.5f, null,
                    obj =>
                    {
                        var manager = GetManager<UIManager>();
                        if (manager == null)
                            Debug.LogError("[GameBootstrap] UIManager not properly initialized");
                    }),
                new("Input System", 0.6f, _inputManagerPrefab,
                    obj =>
                    {
                        var manager = obj.GetComponent<InputManager>();
                        if (manager == null)
                            Debug.LogError("[GameBootstrap] InputManager component missing on Input System prefab");
                        _managers[typeof(InputManager)] = manager;
                    }),
                new("Game Mode System", 0.7f, _gameModeManagerPrefab,
                    obj =>
                    {
                        var manager = obj.GetComponent<GameModeManager>();
                        if (manager == null)
                            Debug.LogError(
                                "[GameBootstrap] GameModeManager component missing on Game Mode System prefab");
                        _managers[typeof(GameModeManager)] = manager;
                    }),
                new("Character System", 0.8f, _characterManagerPrefab,
                    obj =>
                    {
                        var manager = obj.GetComponent<CharacterManager>();
                        if (manager == null)
                            Debug.LogError(
                                "[GameBootstrap] CharacterManager component missing on Character System prefab");
                        _managers[typeof(CharacterManager)] = manager;
                    }),
                new("Scoring System", 0.9f, _scoreManagerPrefab,
                    obj =>
                    {
                        var manager = obj.GetComponent<ScoreManager>();
                        if (manager == null)
                            Debug.LogError("[GameBootstrap] ScoreManager component missing on Scoring System prefab");
                        _managers[typeof(ScoreManager)] = manager;
                    }),
                new("Order System", 1.0f, _orderManagerPrefab,
                    obj =>
                    {
                        var manager = obj.GetComponent<OrderManager>();
                        if (manager == null)
                            Debug.LogError("[GameBootstrap] OrderManager component missing on Order System prefab");
                        _managers[typeof(OrderManager)] = manager;
                    })
            };

            float timeoutTime = Time.time + (_bootstrapConfig?.initializationTimeout ?? 30f);
            bool hasErrors = false;

            foreach (var system in systems)
            {
                if (Time.time > timeoutTime)
                {
                    Debug.LogError("[GameBootstrap] System initialization timed out");
                    OnInitializationError?.Invoke("System initialization timed out");
                    hasErrors = true;
                    break;
                }

                UpdateProgress($"Initializing {system.Name}...", system.Progress);

                yield return StartCoroutine(InitializeSystemAsync(system));
                OnSystemInitialized?.Invoke(system.Name);

                yield return null;
            }

            if (!hasErrors)
            {
                UpdateProgress("Complete!", 1.0f);
            }
        }

        private IEnumerator InitializeSystemAsync(SystemInitInfo systemInfo)
        {
            // Skip if no prefab (for validation-only systems)
            if (systemInfo.Prefab == null)
            {
                systemInfo.OnInitialized?.Invoke(null);
                yield break;
            }

            var systemObj = Instantiate(systemInfo.Prefab);
            systemObj.name = systemInfo.Name.Replace(" ", "");

            systemInfo.OnInitialized?.Invoke(systemObj);
            yield return null;

            float timeout = Time.time + 5f;
            while (!systemInfo.IsReady() && Time.time < timeout)
                yield return null;

            if (!systemInfo.IsReady())
                Debug.LogWarning($"[GameBootstrap] {systemInfo.Name} not ready after timeout");
        }


        private void SetInitialGameState()
        {
            var gameStateManager = GetManager<GameStateManager>();
            if (gameStateManager == null)
            {
                Debug.LogError("[GameBootstrap] GameStateManager not found");
                return;
            }

            var uiManager = GetManager<UIManager>();
            if (uiManager == null)
            {
                Debug.LogError("[GameBootstrap] UIManager not found");
                return;
            }

            GameStateType initialStateType =
                _bootstrapConfig?.initialState ?? GameStateType.MainMenu;

            IState initialState = initialStateType switch
            {
                GameStateType.MainMenu => new MainMenuState(),
                GameStateType.Lobby => new LobbyState(),
                GameStateType.Game => new GameplayState(),
                GameStateType.GameOver => new GameOverState(),
                _ => new MainMenuState()
            };

            gameStateManager.Initialize(initialState);
        }

        private bool ValidateSystemHealth()
        {
            var criticalManagers = new Type[]
            {
                typeof(GameStateManager),
                typeof(UIManager)
            };

            return criticalManagers.All(type => _managers.ContainsKey(type) && _managers[type] != null);
        }

        public bool IsInitialized => _isInitialized;
        public bool IsHealthy => _isInitialized && ValidateSystemHealth();

        public enum GameStateType
        {
            MainMenu,
            Lobby,
            Game,
            GameOver
        }
    }
}