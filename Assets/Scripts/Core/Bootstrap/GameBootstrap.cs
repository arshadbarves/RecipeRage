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
using UI.UISystem.Screens;
using UnityEngine;

namespace Core.Bootstrap
{
    /// <summary>
    /// Single entry point for the entire game - bootstraps all services
    /// Flow: Bootstrap → ServiceContainer → Services → Game
    /// Progressive initialization with pyramid architecture
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

        // Static instance for factory methods
        public static GameBootstrap Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }

        private void InitializeGame()
        {
            Debug.Log("[GameBootstrap] Starting game initialization...");

            // Create service container
            _services = new ServiceContainer();

            InitializeFoundation();

            // Make services globally accessible
            Services = _services;

            // Start async initialization
            InitializeAsync().Forget();
        }

        private void InitializeFoundation()
        {
            Debug.Log("[GameBootstrap] Initializing foundation services");

            _services.RegisterEventBus(new Events.EventBus());

            _services.RegisterAnimationService(CreateAnimationService());

            _services.RegisterUIService(CreateUIService());

            Debug.Log("[GameBootstrap] Foundation services initialized");
        }

        private async UniTaskVoid InitializeAsync()
        {
            try
            {
                // Wait for next frame to ensure Unity is ready
                await UniTask.Yield();

                if (_showSplashScreen)
                {
                    await ShowSplashScreenAsync();
                }

                Debug.Log("[GameBootstrap] Initializing core services");
                InitializeCoreServices();

                // Delegate authentication flow to AuthenticationService
                bool isAuthenticated = await _services.AuthenticationService.InitializeAsync();

                if (isAuthenticated)
                {
                    // User is authenticated - proceed with post-login initialization
                    await InitializePostLoginAsync();
                    ProceedToMainMenu();
                }
                else
                {
                    // User needs to login - AuthenticationService handles showing login screen
                    // Subscribe to login success event to continue initialization
                    _services.EventBus.Subscribe<Events.LoginSuccessEvent>(OnLoginSuccess);
                }

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameBootstrap] Initialization failed: {ex.Message}");
                ShowErrorScreen($"Initialization failed: {ex.Message}");
            }
        }

        private void InitializeCoreServices()
        {
            // Save service
            var storageFactory = new StorageProviderFactory();
            _services.RegisterSaveService(new SaveService(
                storageFactory,
                new EncryptionService()
            ));

            // Authentication service with proper dependency injection
            var authService = new AuthenticationService(
                _services.SaveService,
                _services.EventBus,
                _services.UIService
            );
            _services.RegisterAuthenticationService(authService);

            // Subscribe to logout event
            authService.OnLogoutComplete += HandleLogout;

            Debug.Log("[GameBootstrap] Core services initialized");
        }

        private async UniTask InitializePostLoginAsync()
        {
            Debug.Log("[GameBootstrap] Post-login initialization");

            LoadingScreen loadingScreen = null;
            if (_showLoadingScreen)
            {
                loadingScreen = ShowLoadingScreen();
            }

            // Services are now lazy-loaded on first access
            // Just show progress for user experience
            loadingScreen?.UpdateProgress(0.5f, "Loading game data...");
            await UniTask.Delay(200);

            loadingScreen?.UpdateProgress(1f, "Ready!");
            await UniTask.Delay(200);

            if (loadingScreen != null)
            {
                await loadingScreen.CompleteAsync();
            }

            Debug.Log("[GameBootstrap] Post-login initialization complete");
        }

        private async UniTask ShowSplashScreenAsync()
        {
            var uiService = _services?.UIService;
            if (uiService == null) return;

            uiService.ShowScreen(UIScreenType.Splash, false, true);

            // Get splash screen instance
            var splashScreen = uiService.GetScreen<SplashScreen>(UIScreenType.Splash);
            if (splashScreen != null)
            {
                await splashScreen.ShowForDurationAsync(_splashDuration);
            }
            else
            {
                Debug.LogError("[GameBootstrap] SplashScreen not found in UIService");
            }
        }

        private LoadingScreen ShowLoadingScreen()
        {
            var uiService = _services?.UIService;
            if (uiService == null) return null;

            uiService.ShowScreen(UIScreenType.Loading, false, true);
            return uiService.GetScreen<LoadingScreen>(UIScreenType.Loading);
        }

        private async void OnLoginSuccess(Events.LoginSuccessEvent evt)
        {
            // Unsubscribe
            _services.EventBus.Unsubscribe<Events.LoginSuccessEvent>(OnLoginSuccess);

            Debug.Log($"[GameBootstrap] Login successful for user: {evt.UserId}");

            // Initialize post-login services (AuthenticationService already notified SaveService)
            await InitializePostLoginAsync();

            ProceedToMainMenu();
        }

        private void HandleLogout()
        {
            Debug.Log("[GameBootstrap] User logged out - resetting services");

            // Reset user-specific services
            _services.ResetUserServices();

            // Hide all game screens
            _services.UIService.HideAllGameScreens(animate: false);

            // AuthenticationService handles showing login screen
            // Subscribe to login success for next login
            _services.EventBus.Subscribe<Events.LoginSuccessEvent>(OnLoginSuccess);

            Debug.Log("[GameBootstrap] Logout complete - ready for new login");
        }

        private void ProceedToMainMenu()
        {
            _services.StateManager.Initialize(new MainMenuState());
        }

        private void ShowErrorScreen(string errorMessage)
        {
            // Show error UI or fallback to login screen
            Debug.LogError($"[GameBootstrap] {errorMessage}");
            
            // Let AuthenticationService handle showing login screen
            _services.UIService?.ShowScreen(UIScreenType.Login);
            _services.EventBus.Subscribe<Events.LoginSuccessEvent>(OnLoginSuccess);
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
