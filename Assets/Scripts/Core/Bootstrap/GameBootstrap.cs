using System;
using Core.Animation;
using Core.Authentication;
using Core.Maintenance;
using Core.SaveSystem;
using Core.State.States;
using Cysharp.Threading.Tasks;
using UI;
using UI.Screens;
using UI;
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
            Debug.Log("[GameBootstrap] Starting game initialization...");
            Services = new ServiceContainer();
            InitializeFoundation();
            InitializeAsync().Forget();
        }
        private void InitializeFoundation()
        {
            Debug.Log("[GameBootstrap] Initializing foundation services");
            Services.RegisterEventBus(new Events.EventBus());

            var storageFactory = new StorageProviderFactory();
            Services.RegisterSaveService(new SaveService(storageFactory, new EncryptionService()));

            Services.RegisterAnimationService(CreateAnimationService());
            Services.RegisterUIService(CreateUIService());
            Services.UIService.InitializeScreens();

            Debug.Log("[GameBootstrap] Foundation services initialized");
        }
        private async UniTaskVoid InitializeAsync()
        {
            try
            {
                await UniTask.Yield();
                if (_showSplashScreen)
                {
                    await ShowSplashScreenAsync();
                }
                Debug.Log("[GameBootstrap] Initializing core services");
                InitializeCoreServices();

                bool isAuthenticated = await Services.AuthenticationService.InitializeAsync();
                if (isAuthenticated)
                {
                    await InitializePostLoginAsync();
                    ProceedToMainMenu();
                }
                else
                {
                    Services.EventBus.Subscribe<Events.LoginSuccessEvent>(OnLoginSuccess);
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
            var maintenanceService = new MaintenanceService(Services.EventBus);
            Services.RegisterMaintenanceService(maintenanceService);

            var authService = new AuthenticationService(Services.SaveService, Services.EventBus, Services.UIService);
            Services.RegisterAuthenticationService(authService);
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
            var uiService = Services?.UIService;
            if (uiService == null)
            {
                return;
            }
            uiService.ShowScreen(UIScreenType.Splash, false, true);
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
            var uiService = Services?.UIService;
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
            Debug.Log($"[GameBootstrap] Login successful for user: {evt.UserId}");

            // Check maintenance status after successful login
            if (Services.MaintenanceService != null)
            {
                bool maintenanceCheckSuccess = await Services.MaintenanceService.CheckMaintenanceStatusAsync();
                if (!maintenanceCheckSuccess)
                {
                    Debug.LogWarning("[GameBootstrap] Maintenance check failed, but proceeding with login");
                }
            }

            await InitializePostLoginAsync();
            ProceedToMainMenu();
        }
        private async void HandleLogout()
        {
            Debug.Log("[GameBootstrap] User logged out - performing full reboot");

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
            Debug.Log("[GameBootstrap] Reboot complete - ready for new login");
        }
        private void ProceedToMainMenu()
        {
            Services.StateManager.Initialize(new MainMenuState());
        }
        private void ShowErrorScreen(string errorMessage)
        {
            Debug.LogError($"[GameBootstrap] {errorMessage}");
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