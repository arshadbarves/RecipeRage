using System;
using Core.Animation;
using Core.Authentication;
using Core.Logging;
using Core.Maintenance;
using Core.RemoteConfig;
using Core.SaveSystem;
using Core.State.States;
using Cysharp.Threading.Tasks;
using UI;
using UI.Screens;
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
        [SerializeField]
        private UIDocumentProvider _uiDocumentProvider;
        [Header("Splash & Loading")]
        [SerializeField]
        private bool _showSplashScreen = true;
        [SerializeField]
        private float _splashDuration = 2f;
        [SerializeField]
        private bool _showLoadingScreen = true;
        private bool _isInitialized;

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
            Debug.Log("Starting game initialization...");
            Services = new ServiceContainer();
            InitializeFoundation();
            InitializeAsync().Forget();
        }

        private void InitializeFoundation()
        {
            Debug.Log("Initializing foundation services");

            Services.RegisterLoggingService(new LoggingService(maxLogEntries: 10000));

            GameLogger.Log("Logging service initialized");

            Services.RegisterEventBus(new Events.EventBus());

            var storageFactory = new StorageProviderFactory();
            Services.RegisterSaveService(new SaveService(storageFactory, new EncryptionService()));

            // Initialize NTP Time Service early
            var ntpTimeService = new NTPTimeService();
            Services.RegisterNTPTimeService(ntpTimeService);
            
            // Initialize Remote Config Service early (before UI)
            var remoteConfigService = new RemoteConfigService();
            Services.RegisterRemoteConfigService(remoteConfigService);

            Services.RegisterAnimationService(CreateAnimationService());
            Services.RegisterUIService(CreateUIService());
            
            // Initialize UI screens AFTER RemoteConfig so they can subscribe to events
            Services.UIService.InitializeScreens();

            GameLogger.Log("Foundation services initialized");
        }

        private async UniTaskVoid InitializeAsync()
        {
            try
            {
                await UniTask.Yield();
                
                // Initialize RemoteConfig early (before splash screen)
                await InitializeRemoteConfigAsync();
                
                if (_showSplashScreen)
                {
                    await ShowSplashScreenAsync();
                }
                
                GameLogger.Log("Initializing core services");
                InitializeCoreServices();

                bool isAuthenticated = await Services.AuthenticationService.InitializeAsync();
                if (isAuthenticated)
                {
                    await InitializePostLoginAsync();
                }
                else
                {
                    Services.EventBus.Subscribe<Events.LoginSuccessEvent>(OnLoginSuccess);
                }
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
                ShowErrorScreen($"Initialization failed: {ex.Message}");
            }
        }

        private void InitializeCoreServices()
        {
            // NTP and RemoteConfig already initialized in InitializeFoundation
            
            var maintenanceService = new MaintenanceService(Services.EventBus);
            Services.RegisterMaintenanceService(maintenanceService);

            var authService = new AuthenticationService(Services.SaveService, Services.EventBus, Services.UIService);
            Services.RegisterAuthenticationService(authService);
            authService.OnLogoutComplete += HandleLogoutAsync;
            GameLogger.Log("Core services initialized");
        }
        
        private async UniTask InitializeRemoteConfigAsync()
        {
            try
            {
                GameLogger.Log("Initializing Remote Config...");
                
                // Sync NTP time first
                bool ntpSynced = await Services.NTPTimeService.SyncTime();
                if (!ntpSynced)
                {
                    GameLogger.LogWarning("NTP sync failed, using local time");
                }
                
                // Initialize Remote Config
                bool configInitialized = await Services.RemoteConfigService.Initialize();
                if (configInitialized)
                {
                    GameLogger.Log("Remote Config initialized successfully");
                }
                else
                {
                    GameLogger.LogWarning("Remote Config initialization failed, using fallback values");
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
                GameLogger.LogWarning("Remote Config initialization error, continuing with defaults");
            }
        }

        private async UniTask InitializePostLoginAsync()
        {
            GameLogger.Log("Post-login initialization");
            LoadingScreen loadingScreen = null;
            if (_showLoadingScreen)
            {
                loadingScreen = ShowLoadingScreen();
            }

            // Refresh remote config (already initialized, just refresh for latest data)
            loadingScreen?.UpdateProgress(0.3f, "Loading configuration...");
            await Services.RemoteConfigService.RefreshConfig();
            
            // Check for force update
            loadingScreen?.UpdateProgress(0.5f, "Checking for updates...");
            await CheckForceUpdateAsync();
            
            // Check maintenance mode
            loadingScreen?.UpdateProgress(0.7f, "Checking server status...");
            await CheckMaintenanceAsync();
            
            loadingScreen?.UpdateProgress(0.9f, "Loading game data...");
            await UniTask.Delay(200);
            
            loadingScreen?.UpdateProgress(1f, "Ready!");
            await UniTask.Delay(200);
            
            if (loadingScreen != null)
            {
                await loadingScreen.CompleteAsync();
            }
            GameLogger.Log("Post-login initialization complete");
        }
        
        private async UniTask CheckForceUpdateAsync()
        {
            try
            {
                var forceUpdateChecker = new ForceUpdateChecker(
                    Services.RemoteConfigService,
                    Services.UIService);
                bool updateRequired = await forceUpdateChecker.CheckForUpdate();
                
                if (updateRequired)
                {
                    GameLogger.Log("Force update required - user must update to continue");
                    // ForceUpdateChecker already shows the popup and blocks if needed
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
                GameLogger.LogWarning("Force update check failed, continuing");
            }
        }
        
        private async UniTask CheckMaintenanceAsync()
        {
            try
            {
                if (Services.MaintenanceService != null)
                {
                    bool maintenanceCheckSuccess = await Services.MaintenanceService.CheckMaintenanceStatusAsync();
                    if (!maintenanceCheckSuccess)
                    {
                        GameLogger.LogWarning("Maintenance check failed, but proceeding");
                    }
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
                GameLogger.LogWarning("Maintenance check error, continuing");
            }
        }

        private async UniTask ShowSplashScreenAsync()
        {
            IUIService uiService = Services?.UIService;
            if (uiService == null)
            {
                return;
            }
            uiService.ShowScreen(UIScreenType.Splash, false, true);
            SplashScreen splashScreen = uiService.GetScreen<SplashScreen>(UIScreenType.Splash);
            if (splashScreen != null)
            {
                await splashScreen.ShowForDurationAsync(_splashDuration);
            }
            else
            {
                GameLogger.LogError("SplashScreen not found in UIService");
            }
        }

        private LoadingScreen ShowLoadingScreen()
        {
            IUIService uiService = Services?.UIService;
            if (uiService == null)
            {
                return null;
            }
            uiService.ShowScreen(UIScreenType.Loading, false, true);
            return uiService.GetScreen<LoadingScreen>(UIScreenType.Loading);
        }

        private async void OnLoginSuccess(Events.LoginSuccessEvent evt)
        {
            Services.EventBus.Unsubscribe<Events.LoginSuccessEvent>(OnLoginSuccess);
            GameLogger.Log($"Login successful for user: {evt.UserId}");

            await InitializePostLoginAsync();
        }

        private async void HandleLogoutAsync()
        {
            GameLogger.Log("User logged out - performing full reboot");

            // Dispose all services
            Services?.Dispose();

            // Mark as uninitialized
            _isInitialized = false;

            // Wait a frame for cleanup
            await UniTask.Yield();

            // Reinitialize everything from scratch
            Services = new ServiceContainer();
            InitializeFoundation();
            InitializeCoreServices();

            // Show login screen
            Services.UIService.ShowScreen(UIScreenType.Login);
            Services.EventBus.Subscribe<Events.LoginSuccessEvent>(OnLoginSuccess);

            _isInitialized = true;
            GameLogger.Log("Reboot complete - ready for new login");
        }

        private void ShowErrorScreen(string errorMessage)
        {
            GameLogger.LogError(errorMessage);
            Services.UIService?.ShowScreen(UIScreenType.Login);
            Services.EventBus.Subscribe<Events.LoginSuccessEvent>(OnLoginSuccess);
        }

        private IAnimationService CreateAnimationService()
        {
            var uiAnimator = new DOTweenUIAnimator();
            var transformAnimator = new DOTweenTransformAnimator();
            return new AnimationService(uiAnimator, transformAnimator);
        }

        private IUIService CreateUIService()
        {
            var uiService = new UIService(Services.AnimationService);
            _uiDocumentProvider.Initialize(uiService);
            return uiService;
        }

        private void Update()
        {
            Services?.Update(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            Services?.FixedUpdate(Time.fixedDeltaTime);
        }

        private void OnDestroy()
        {
            Services?.Dispose();
        }

        public static ServiceContainer Services { get; private set; }
        public bool IsInitialized => _isInitialized;
        public bool IsHealthy => _isInitialized && Services != null;
    }
}