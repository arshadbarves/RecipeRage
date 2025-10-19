using System;
using Core.Animation;
using Core.Audio;
using Core.Authentication;
using Core.Characters;
using Core.GameModes;
using Core.Input;
using Core.Logging;
using Core.Networking;
using Core.SaveSystem;
using Core.State;
using Core.State.States;
using Cysharp.Threading.Tasks;
using UI.UISystem;
using UnityEngine;

namespace Core.Bootstrap
{
    /// <summary>
    /// Single entry point for the entire game - bootstraps all services
    /// Flow: Bootstrap → ServiceContainer → Services → Game
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private UIDocumentProvider _uiDocumentProvider;

        [Header("Splash & Loading")]
        [SerializeField] private bool _showSplashScreen = true;
        [SerializeField] private float _splashDuration = 2f;
        [SerializeField] private bool _showLoadingScreen = true;

        private ServiceContainer _services;
        private bool _isInitialized;

        private void Awake()
        {
            if (_isInitialized) return;

            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }

        private void InitializeGame()
        {
            Debug.Log("[GameBootstrap] Starting game initialization...");

            // Create service container (single source of truth)
            _services = new ServiceContainer();

            // Initialize services in dependency order
            InitializeCore();
            InitializeGameSystems();

            // Start async initialization
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            try
            {
                // Wait for next frame to ensure Unity is ready
                await UniTask.Yield();
                
                // Make services globally accessible immediately (before async operations)
                Services = _services;

                // Show splash screen if enabled
                if (_showSplashScreen)
                {
                    await ShowSplashScreenAsync();
                }

                // Show loading screen if enabled
                UI.UISystem.Screens.LoadingScreen loadingScreen = null;
                if (_showLoadingScreen)
                {
                    loadingScreen = ShowLoadingScreen();
                }

                // Initialize authentication (EOS is already attached to this GameObject)
                loadingScreen?.UpdateProgress(0.3f, "Initializing authentication...");
                InitializeAuthentication();
                await UniTask.Delay(200); // Small delay for visual feedback

                // Initialize networking (requires auth)
                loadingScreen?.UpdateProgress(0.6f, "Connecting to services...");
                InitializeNetworking();
                await UniTask.Delay(200);

                _isInitialized = true;
                loadingScreen?.UpdateProgress(0.9f, "Finalizing...");
                await UniTask.Delay(200);

                Debug.Log("[GameBootstrap] ✅ Core initialization complete!");

                // Try auto-login
                loadingScreen?.UpdateProgress(1f, "Checking credentials...");
                bool loginSuccess = await _services.AuthenticationService.AttemptAutoLoginAsync();

                // Hide loading screen
                if (loadingScreen != null)
                {
                    await loadingScreen.CompleteAsync();
                }

                if (loginSuccess)
                {
                    Debug.Log("[GameBootstrap] Auto-login successful - proceeding to main menu");
                    ProceedToMainMenu();
                }
                else
                {
                    Debug.Log("[GameBootstrap] Auto-login failed - showing login screen");
                    ShowLoginScreen();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameBootstrap] Initialization failed: {ex.Message}");
                ShowErrorScreen($"Initialization failed: {ex.Message}");
            }
        }

        private async UniTask ShowSplashScreenAsync()
        {
            var uiService = _services?.UIService;
            if (uiService == null) return;

            uiService.ShowScreen(UIScreenType.Splash, false, true);

            // Get splash screen instance
            var splashScreen = uiService.GetScreen<UI.UISystem.Screens.SplashScreen>(UIScreenType.Splash);
            if (splashScreen != null)
            {
                await splashScreen.ShowForDurationAsync(_splashDuration);
            }
            else
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_splashDuration));
            }
        }

        private UI.UISystem.Screens.LoadingScreen ShowLoadingScreen()
        {
            var uiService = _services?.UIService;
            if (uiService == null) return null;

            uiService.ShowScreen(UIScreenType.Loading, false, true);
            return uiService.GetScreen<UI.UISystem.Screens.LoadingScreen>(UIScreenType.Loading);
        }

        private void ShowLoginScreen()
        {
            var uiService = _services?.UIService;
            if (uiService != null)
            {
                uiService.ShowScreen(UIScreenType.Login);

                // Subscribe to login success to proceed to main menu
                _services.AuthenticationService.OnLoginSuccess += OnLoginSuccessHandler;
            }
            else
            {
                Debug.LogError("[GameBootstrap] UIService not available to show login screen");
            }
        }

        private void OnLoginSuccessHandler()
        {
            // Unsubscribe to avoid duplicate calls
            _services.AuthenticationService.OnLoginSuccess -= OnLoginSuccessHandler;

            Debug.Log("[GameBootstrap] Login successful - proceeding to main menu");
            ProceedToMainMenu();
        }

        private void ProceedToMainMenu()
        {
            _services.StateManager.Initialize(new MainMenuState());
        }

        private void ShowErrorScreen(string errorMessage)
        {
            // Show error UI or fallback to login screen
            Debug.LogError($"[GameBootstrap] {errorMessage}");
            ShowLoginScreen();
        }

        private void InitializeCore()
        {
            // Logging service first (only in development builds)
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            _services.RegisterLoggingService(new LoggingService(maxLogEntries: 5000));
            Debug.Log("[GameBootstrap] Logging service initialized (Development Build)");
#endif

            // Core services (no dependencies)
            _services.RegisterSaveService(new SaveService(
                new FileSystemStorage(),
                new EncryptionService()
            ));

            _services.RegisterAudioService(CreateAudioService());
            _services.RegisterInputService(CreateInputService());
            _services.RegisterAnimationService(CreateAnimationService());
            _services.RegisterUIService(CreateUIService());
        }

        private void InitializeGameSystems()
        {
            // Game systems (depend on core services)
            _services.RegisterGameModeService(new GameModeService());
            _services.RegisterCharacterService(new CharacterService());
            _services.RegisterStateManager(new GameStateManager());
        }

        private void InitializeAuthentication()
        {
            // Authentication service (with SaveService dependency)
            var authService = new AuthenticationService(_services.SaveService);
            _services.RegisterAuthenticationService(authService);

            Debug.Log("[GameBootstrap] Authentication service initialized");
        }

        private void InitializeNetworking()
        {
            // Networking (depends on authentication)
            var networkingServices = new NetworkingServiceContainer();
            _services.RegisterNetworkingServices(networkingServices);
        }

        private IAudioService CreateAudioService()
        {
            var poolManager = new AudioPoolManager(transform);
            var volumeController = new AudioVolumeController(_services.SaveService);
            var musicPlayer = new MusicPlayer(volumeController);
            var sfxPlayer = new SFXPlayer(poolManager, volumeController);

            return new AudioService(musicPlayer, sfxPlayer, volumeController);
        }

        private IInputService CreateInputService()
        {
            var provider = InputProviderFactory.CreateForPlatform();
            return new InputService(provider);
        }

        private IAnimationService CreateAnimationService()
        {
            var uiAnimator = new DOTweenUIAnimator();
            var transformAnimator = new DOTweenTransformAnimator();
            return new AnimationService(uiAnimator, transformAnimator);
        }

        private IUIService CreateUIService()
        {
            // UIDocument will be provided by UIDocumentProvider component in the scene
            var uiService = new UIService(_services.AnimationService);
            _uiDocumentProvider.Initialize(uiService);
            return uiService;
        }

        private void Update()
        {
            _services?.Update(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            _services?.FixedUpdate(Time.fixedDeltaTime);
        }

        private void OnDestroy()
        {
            _services?.Dispose();
        }

        /// <summary>
        /// Global access point (only for MonoBehaviours that can't use DI)
        /// </summary>
        public static ServiceContainer Services { get; private set; }

        /// <summary>
        /// Check if bootstrap is fully initialized
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Check if system is healthy
        /// </summary>
        public bool IsHealthy => _isInitialized && _services != null;
    }
}
