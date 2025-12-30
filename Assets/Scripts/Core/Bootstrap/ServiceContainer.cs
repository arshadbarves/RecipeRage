using System;
using Core.Animation;
using Core.Events;
using Core.Logging;
using Core.Maintenance;
using Core.RemoteConfig;
using Core.SaveSystem;
using Core.State;
using UI;
using Core.Networking;
using RecipeRage.Modules.Auth.Core;

namespace Core.Bootstrap
{
    /// <summary>
    /// Central service container with lazy loading support
    /// Follows Pyramid Architecture:
    /// - Foundation services (eager): EventBus, Animation, UI
    /// - Core services (eager): Logging, Save, Auth, Maintenance
    /// - Application services (lazy): Currency, Audio, Input
    /// - Game systems (lazy): GameMode, Character, Network, State
    /// </summary>
    public class ServiceContainer : IDisposable
    {
        // ============================================
        // FOUNDATION SERVICES (Eager - Always Loaded)
        // ============================================

        public IEventBus EventBus { get; private set; }
        public IAnimationService AnimationService { get; private set; }
        public IUIService UIService { get; private set; }

        private readonly UIDocumentProvider _uiDocumentProvider; // Platform dependency

        // ============================================
        // CORE SERVICES (Eager - Pre-Auth)
        // ============================================

        public ILoggingService LoggingService { get; private set; }
        public ISaveService SaveService { get; private set; }
        public IAuthService AuthService { get; private set; }
        public IMaintenanceService MaintenanceService { get; private set; }
        public IRemoteConfigService RemoteConfigService { get; private set; }
        public INTPTimeService NTPTimeService { get; private set; }
        public IConnectivityService ConnectivityService { get; private set; }
        public IGameStateManager StateManager { get; private set; }

        // ============================================
        // GAME SESSION (Scoped - User Specific)
        // ============================================

        /// <summary>
        /// Current game session (Post-Auth). Null if not logged in.
        /// </summary>
        public GameSession Session { get; private set; }

        // ============================================
        // SESSION MANAGEMENT
        // ============================================

        public void CreateSession()
        {
            // LEGACY: No longer used. Session handled by VContainer and SessionManager.
            GameLogger.LogWarning("LEGACY CreateSession called. Ignoring.");
        }

        public void DestroySession()
        {
            if (Session != null)
            {
                Session.Dispose();
                Session = null;
            }
        }

        public void Update(float deltaTime)
        {
            // Update foundation services
            UIService?.Update(deltaTime);

            // Update session services if active
            Session?.Update(deltaTime);
        }

        public void FixedUpdate(float fixedDeltaTime)
        {
            Session?.FixedUpdate(fixedDeltaTime);
        }

        public void Dispose()
        {
            GameLogger.Log("Disposing all services");

            // Dispose lazy services
            DestroySession();

            // Dispose eager services
            (LoggingService as IDisposable)?.Dispose();
            (UIService as IDisposable)?.Dispose();

            // Clear event bus
            EventBus?.ClearAllSubscriptions();

            GameLogger.Log("All services disposed");
        }

        // ============================================
        // CONSTRUCTOR & INIT
        // ============================================

        public ServiceContainer(UIDocumentProvider uiDocumentProvider)
        {
            _uiDocumentProvider = uiDocumentProvider;
            
            // Phase 1: Construct all services (store references only)
            ConstructFoundation();
            ConstructCoreServices();
            
            // Phase 2: Initialize all services (safe to access each other)
            InitializeAllServices();
        }

        /// <summary>
        /// Phase 1a: Construct foundation services.
        /// Only set up internal state, don't access other services.
        /// </summary>
        private void ConstructFoundation()
        {
            GameLogger.Log("Constructing foundation services...");

            LoggingService = new LoggingService(maxLogEntries: 10000);
            EventBus = new Events.EventBus();

            var storageFactory = new StorageProviderFactory();
            SaveService = new SaveService(storageFactory, new EncryptionService());

            NTPTimeService = new NTPTimeService();
            NTPTime.SetInstance(NTPTimeService);

            RemoteConfigService = new RemoteConfigService();

            AnimationService = CreateAnimationService();
            UIService = CreateUIService();

            ConnectivityService = new ConnectivityService(EventBus);

            GameLogger.Log("Foundation services constructed.");
        }

        /// <summary>
        /// Phase 1b: Construct core services.
        /// Only set up internal state, don't access other services.
        /// </summary>
        private void ConstructCoreServices()
        {
            GameLogger.Log("Constructing core services...");

            MaintenanceService = new MaintenanceService(EventBus, RemoteConfigService);
            AuthService = new EOSAuthService(EventBus, SaveService, CreateSession);
            StateManager = new GameStateManager();

            GameLogger.Log("Core services constructed.");
        }

        /// <summary>
        /// Phase 2: Initialize all services.
        /// Services can now safely access other services.
        /// </summary>
        private void InitializeAllServices()
        {
            GameLogger.Log("Initializing all services...");

            // Initialize foundation services that need UI setup
            UIService.InitializeScreens();

            // Initialize all services (all interfaces now extend IInitializable)
            LoggingService.Initialize();
            EventBus.Initialize();
            SaveService.Initialize();
            NTPTimeService.Initialize();
            RemoteConfigService.Initialize();
            AnimationService.Initialize();
            ConnectivityService.Initialize();
            MaintenanceService.Initialize();

            GameLogger.Log("All services initialized.");
        }

        private IAnimationService CreateAnimationService()
        {
            var uiAnimator = new DOTweenUIAnimator();
            var transformAnimator = new DOTweenTransformAnimator();
            return new AnimationService(uiAnimator, transformAnimator);
        }

        private IUIService CreateUIService()
        {
            var uiService = new UIService(AnimationService);
            _uiDocumentProvider.Initialize(uiService);
            return uiService;
        }
    }
}

