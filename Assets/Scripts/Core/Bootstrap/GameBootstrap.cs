using System.Collections;
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
using PlayEveryWare.EpicOnlineServices;
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
            StartCoroutine(InitializeAsync());
        }

        private IEnumerator InitializeAsync()
        {
            // Wait for EOS to initialize
            yield return WaitForEOS();

            // Initialize authentication
            InitializeAuthentication();

            // Try auto-login
            yield return _services.AuthenticationService.AttemptAutoLogin();

            // Initialize networking (requires auth)
            InitializeNetworking();

            // Initialize state machine
            InitializeStateMachine();

            _isInitialized = true;

            // Make services globally accessible
            Services = _services;

            Debug.Log("[GameBootstrap] ✅ Game initialization complete!");
        }

        private IEnumerator WaitForEOS()
        {
            Debug.Log("[GameBootstrap] Waiting for EOS to initialize...");

            while (EOSManager.Instance == null)
            {
                yield return new WaitForSeconds(0.1f);
            }

            Debug.Log("[GameBootstrap] EOS initialized");
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

        private void InitializeStateMachine()
        {
            // Start with main menu state
            _services.StateManager.Initialize(new MainMenuState());
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
            // Create UIManager as a pure service (no MonoBehaviour)
            // UIDocument will be provided by UIDocumentProvider component in the scene
            var uiManager = new UIManager();
            
            GameLogger.Log("[GameBootstrap] UIManager service created. Waiting for UIDocumentProvider to initialize...");
            
            return uiManager;
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
