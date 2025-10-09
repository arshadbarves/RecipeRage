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
        [Header("Configuration")]
        [SerializeField] private Configuration.BootstrapConfiguration bootstrapConfig;

        [Header("Manager Prefabs")]
        [SerializeField] private GameObject authenticationManagerPrefab;
        [SerializeField] private GameObject networkManagerPrefab;
        [SerializeField] private GameObject gameStateManagerPrefab;
        [SerializeField] private GameObject uiManagerPrefab;
        [SerializeField] private GameObject inputManagerPrefab;
        [SerializeField] private GameObject gameModeManagerPrefab;
        [SerializeField] private GameObject characterManagerPrefab;
        [SerializeField] private GameObject scoreManagerPrefab;
        [SerializeField] private GameObject orderManagerPrefab;
        [SerializeField] private GameObject saveManagerPrefab;
        [SerializeField] private GameObject audioManagerPrefab;

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
            // Defer UI initialization to avoid threading issues with UI Toolkit text rendering
        }

        private void Start()
        {
            if (_initializationCoroutine == null)
            {
                _initializationCoroutine = StartCoroutine(InitializeGameSystemsAsync());
            }
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

        private T GetManager<T>() where T : class
        {
            return _managers.TryGetValue(typeof(T), out object manager) ? manager as T : null;
        }

        private void UpdateProgress(string message, float progress)
        {
            OnInitializationProgress?.Invoke(message, progress);
            GetManager<UIManager>()?.GetScreen<LoadingScreen>()?.UpdateProgress(message, progress);
        }

        private IEnumerator InitializeGameSystemsAsync()
        {
            // Wait one frame to ensure Unity is fully initialized before creating UI
            yield return null;

            // Initialize UI Manager first, now that we're on the main thread after first frame
            InitializeUIManager();
            yield return null;

            yield return StartCoroutine(HandleSplashScreenAsync());
            yield return StartCoroutine(InitializeAllSystemsAsync());

            if (!ValidateSystemHealth())
            {
                const string error = "Critical systems failed validation";
                Debug.LogError($"[GameBootstrap] {error}");
                OnInitializationError?.Invoke(error);
                yield break;
            }

            SetInitialGameState();
            OnInitializationComplete?.Invoke();
            _isInitialized = true;
        }

        private IEnumerator HandleSplashScreenAsync()
        {
            UIManager uiManager = GetManager<UIManager>();
            if (bootstrapConfig == null || !bootstrapConfig.showSplashScreens || uiManager == null)
            {
                yield break;
            }

            SplashScreen splashScreen = uiManager.GetScreen<SplashScreen>();
            if (splashScreen != null)
            {
                splashScreen
                    .SetSplashDuration(bootstrapConfig.companySplashDuration)
                    .SetFadeDurations(bootstrapConfig.splashFadeInDuration, bootstrapConfig.splashFadeOutDuration);

                bool splashCompleted = false;
                splashScreen.OnSplashComplete += () => splashCompleted = true;

                uiManager.ShowScreen(UIScreenType.Splash, true, false);

                while (!splashCompleted)
                {
                    yield return null;
                }
            }
        }

        private readonly struct SystemInitInfo
        {
            public readonly string Name;
            public readonly float Progress;
            public readonly GameObject Prefab;
            public readonly Action<GameObject> OnInitialized;
            public readonly Func<bool> IsReady;
            public readonly bool AllowTimeout;

            public SystemInitInfo(string name, float progress, GameObject prefab,
                Action<GameObject> onInitialized, Func<bool> isReady = null, bool allowTimeout = true)
            {
                Name = name;
                Progress = progress;
                Prefab = prefab;
                OnInitialized = onInitialized;
                IsReady = isReady ?? (() => true);
                AllowTimeout = allowTimeout;
            }
        }

        private void InitializeUIManager()
        {
            // Prevent double initialization
            if (_managers.ContainsKey(typeof(UIManager)))
            {
                return;
            }

            InitializeSystemSync(new SystemInitInfo("UIManager", 0f, uiManagerPrefab,
                _ => { _managers[typeof(UIManager)] = UIManager.Instance; }));
        }

        private void InitializeSystemSync(SystemInitInfo systemInfo)
        {
            if (systemInfo.Prefab == null)
            {
                Debug.LogError($"[GameBootstrap] {systemInfo.Name} prefab not assigned");
                return;
            }

            GameObject systemObj = Instantiate(systemInfo.Prefab);
            systemObj.name = systemInfo.Name.Replace(" ", "");
            systemInfo.OnInitialized?.Invoke(systemObj);
        }

        private IEnumerator InitializeAllSystemsAsync()
        {
            UIManager uiManager = GetManager<UIManager>();
            LoadingScreen loadingScreen = uiManager?.GetScreen<LoadingScreen>();

            if (loadingScreen != null && bootstrapConfig != null)
            {
                loadingScreen
                    .SetMinimumDuration(bootstrapConfig.minLoadingScreenDuration)
                    .SetHideDelay(bootstrapConfig.loadingFadeTransitionDuration);
            }

            uiManager?.ShowScreen(UIScreenType.Loading, true, false);
            loadingScreen?.StartLoading();

            SystemInitInfo[] systems = new[]
            {
                new SystemInitInfo("Save System", 0.05f, saveManagerPrefab, _ =>
                {
                    _managers[typeof(SaveManager)] = SaveManager.Instance;
                    SaveManager.Instance?.ImportFromPlayerPrefs();
                }),
                new SystemInitInfo("Audio System", 0.1f, audioManagerPrefab, _ =>
                {
                    _managers[typeof(AudioManager)] = AudioManager.Instance;
                }),
                new SystemInitInfo("Authentication System", 0.2f, authenticationManagerPrefab, obj =>
                {
                    _managers[typeof(Authentication.AuthenticationManager)] = obj.GetComponent<Authentication.AuthenticationManager>();
                }),
                new SystemInitInfo("Networking System", 0.3f, networkManagerPrefab, obj =>
                {
                    _managers[typeof(NetworkBootstrap)] = obj.GetComponent<NetworkBootstrap>();
                }, () => GetManager<NetworkBootstrap>()?.IsInitialized ?? true, allowTimeout: false),
                new SystemInitInfo("Game State System", 0.4f, gameStateManagerPrefab, _ =>
                {
                    _managers[typeof(GameStateManager)] = GameStateManager.Instance;
                }),
                new SystemInitInfo("UI System Validation", 0.5f, null, _ =>
                {
                    if (GetManager<UIManager>() == null)
                    {
                        Debug.LogError("[GameBootstrap] UIManager not properly initialized");
                    }
                }),
                new SystemInitInfo("Input System", 0.6f, inputManagerPrefab, obj =>
                {
                    _managers[typeof(InputManager)] = obj.GetComponent<InputManager>();
                }),
                new SystemInitInfo("Game Mode System", 0.7f, gameModeManagerPrefab, obj =>
                {
                    _managers[typeof(GameModeManager)] = obj.GetComponent<GameModeManager>();
                }),
                new SystemInitInfo("Character System", 0.8f, characterManagerPrefab, obj =>
                {
                    _managers[typeof(CharacterManager)] = obj.GetComponent<CharacterManager>();
                }),
                new SystemInitInfo("Scoring System", 0.9f, scoreManagerPrefab, obj =>
                {
                    _managers[typeof(ScoreManager)] = obj.GetComponent<ScoreManager>();
                }),
                new SystemInitInfo("Order System", 1.0f, orderManagerPrefab, obj =>
                {
                    _managers[typeof(OrderManager)] = obj.GetComponent<OrderManager>();
                })
            };

            float timeoutTime = Time.time + (bootstrapConfig?.initializationTimeout ?? 30f);

            foreach (SystemInitInfo system in systems)
            {
                if (Time.time > timeoutTime)
                {
                    const string error = "System initialization timed out";
                    Debug.LogError($"[GameBootstrap] {error}");
                    OnInitializationError?.Invoke(error);
                    yield break;
                }

                UpdateProgress($"Initializing {system.Name}...", system.Progress);
                yield return StartCoroutine(InitializeSystemAsync(system));
                OnSystemInitialized?.Invoke(system.Name);
                yield return null;
            }

            UpdateProgress("Complete!", 1.0f);
        }

        private IEnumerator InitializeSystemAsync(SystemInitInfo systemInfo)
        {
            if (systemInfo.Prefab == null)
            {
                systemInfo.OnInitialized?.Invoke(null);
                yield break;
            }

            GameObject systemObj = Instantiate(systemInfo.Prefab);
            systemObj.name = systemInfo.Name.Replace(" ", "");
            systemInfo.OnInitialized?.Invoke(systemObj);
            yield return null;

            if (systemInfo.AllowTimeout)
            {
                float timeout = Time.time + (bootstrapConfig?.systemInitTimeout ?? 5f);
                while (!systemInfo.IsReady() && Time.time < timeout)
                {
                    yield return null;
                }

                if (!systemInfo.IsReady())
                {
                    Debug.LogWarning($"[GameBootstrap] {systemInfo.Name} not ready after timeout");
                }
            }
            else
            {
                while (!systemInfo.IsReady())
                {
                    yield return null;
                }
            }
        }


        private void SetInitialGameState()
        {
            GameStateManager gameStateManager = GetManager<GameStateManager>();
            if (gameStateManager == null)
            {
                Debug.LogError("[GameBootstrap] GameStateManager not found");
                return;
            }

            GameStateType initialStateType = bootstrapConfig?.initialState ?? GameStateType.MainMenu;
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
            Type[] criticalManagers = new[]
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