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
    /// Flow: Bootstrap → ServiceContainer → Services → BootstrapState
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField]
        private UIDocumentProvider _uiDocumentProvider;

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
            
            // Start the State Machine with BootstrapState
            GameLogger.Log("Starting State Machine...");
            Services.StateManager.Initialize(new BootstrapState());
            
            _isInitialized = true;
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
            
            // Register Core Services (Auth, Maintenance)
            InitializeCoreServices();

            GameLogger.Log("Foundation services initialized");
        }

        private void InitializeCoreServices()
        {
            var maintenanceService = new MaintenanceService(Services.EventBus);
            Services.RegisterMaintenanceService(maintenanceService);

            var authService = new AuthenticationService(Services.SaveService, Services.EventBus);
            Services.RegisterAuthenticationService(authService);
            authService.OnLogoutComplete += HandleLogoutAsync;
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

            // Restart State Machine
            Services.StateManager.Initialize(new LoginState());

            _isInitialized = true;
            GameLogger.Log("Reboot complete - ready for new login");
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