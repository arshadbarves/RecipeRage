using System;
using Core.Animation;
using Core.Authentication;
using Core.Events;
using Core.Logging;
using Core.Maintenance;
using Core.RemoteConfig;
using Core.SaveSystem;
using Core.State;
using UI;

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
        public IAuthenticationService AuthenticationService { get; private set; }
        public IMaintenanceService MaintenanceService { get; private set; }
        public IRemoteConfigService RemoteConfigService { get; private set; }
        public INTPTimeService NTPTimeService { get; private set; }
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
            if (Session != null)
            {
                GameLogger.LogWarning("Session already exists! Destroying previous session.");
                DestroySession();
            }

            Session = new GameSession(SaveService, EventBus, LoggingService);
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
            InitializeFoundation();
            InitializeCoreServices();
        }

        private void InitializeFoundation()
        {
            GameLogger.Log("Initializing foundation services");

            LoggingService = new LoggingService(maxLogEntries: 10000);
            GameLogger.Log("Logging service initialized");

            EventBus = new Events.EventBus();

            var storageFactory = new StorageProviderFactory();
            SaveService = new SaveService(storageFactory, new EncryptionService());

            NTPTimeService = new NTPTimeService();

            RemoteConfigService = new RemoteConfigService();

            AnimationService = CreateAnimationService();
            UIService = CreateUIService();

            UIService.InitializeScreens();

            GameLogger.Log("Foundation services initialized");
        }

        private void InitializeCoreServices()
        {
            MaintenanceService = new MaintenanceService(EventBus);

            AuthenticationService = new AuthenticationService(SaveService, EventBus);

            // StateManager is global application state
            StateManager = new GameStateManager();
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
