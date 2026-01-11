using System;
using Cysharp.Threading.Tasks;
using Gameplay.UI.Features.Loading;
using Gameplay.UI.Features.System;
using Core.Auth.Core;
using Core.Logging;
using Core.Persistence;
using Core.RemoteConfig;
using Core.RemoteConfig.Interfaces;
using Core.Shared.Events;
using Core.UI.Interfaces;

namespace Gameplay.App.State.States
{
    /// <summary>
    /// Initial state of the game. Handles the startup sequence using professional Task-Based Architecture.
    /// Steps: Splash -> Loading (Tasks) -> Main Menu / Login
    /// </summary>
    public class BootstrapState : BaseState
    {
        private const float SplashDuration = 3.5f;
        private readonly IUIService _uiService;
        private readonly INTPTimeService _ntpTimeService;
        private readonly IRemoteConfigService _remoteConfigService;
        private readonly IAuthService _authService;
        private readonly ISaveService _saveService;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IGameStateManager _stateManager;
        private readonly IEventBus _eventBus;

        public BootstrapState(
            IUIService uiService,
            INTPTimeService ntpTimeService,
            IRemoteConfigService remoteConfigService,
            IAuthService authService,
            ISaveService saveService,
            IMaintenanceService maintenanceService,
            IGameStateManager stateManager,
            IEventBus eventBus)
        {
            _uiService = uiService;
            _ntpTimeService = ntpTimeService;
            _remoteConfigService = remoteConfigService;
            _authService = authService;
            _saveService = saveService;
            _maintenanceService = maintenanceService;
            _stateManager = stateManager;
            _eventBus = eventBus;
        }

        public override async void Enter()
        {
            base.Enter();
            GameLogger.Log("Entering game initialization setup");

            try
            {
                // 1. Show Splash
                await ShowSplashScreenAsync();

                // 2. Start Loading Sequence
                _uiService.Show<LoadingScreen>();
                await InitializeGameSequence();
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
                _uiService.Hide<LoadingScreen>();
                _stateManager.ChangeState<LoginState>();
            }
        }

        private async UniTask InitializeGameSequence()
        {
            var loadingScreen = _uiService.GetScreen<LoadingScreen>();

            // --- STEP 1: Foundation (0% - 30%) ---

            loadingScreen?.UpdateProgress(0.1f, "Initializing Foundation...");

            // NTP Sync
            try
            {
                var cts = new System.Threading.CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(5.0f));
                await _ntpTimeService.SyncTime().AttachExternalCancellation(cts.Token).SuppressCancellationThrow();
            }
            catch { /* Ignore NTP errors */ }

            loadingScreen?.UpdateProgress(0.2f, "Loading Configuration...");
            await _remoteConfigService.Initialize();

            // --- STEP 1.5: System Checks (Pre-Login) ---
            // These checks must pass before we allow any login (auto or manual).

            // 1. Refresh Remote Config
            loadingScreen?.UpdateProgress(0.25f, "Syncing Configuration...");
            await _remoteConfigService.RefreshConfig();

            // 2. Force Update Check
            loadingScreen?.UpdateProgress(0.3f, "Checking for Updates...");
            var forceUpdateChecker = new ForceUpdateChecker(_remoteConfigService, _eventBus);
            bool isUpdateRequired = await forceUpdateChecker.CheckForUpdateAsync();

            if (isUpdateRequired)
            {
                GameLogger.LogInfo("[Bootstrap] Force update required. Halting boot sequence.");
                _uiService.Hide<LoadingScreen>();

                // TODO: Show Force Upgrade Popup, Move the Logic to show from the Core itself liek using Interface or something.
                // The ForceUpdateChecker publishes an event that should show the Update Popup.
                // We stop here to prevent login.
                return;
            }

            // 3. Maintenance Check
            loadingScreen?.UpdateProgress(0.35f, "Checking Maintenance...");
            if (_maintenanceService != null)
            {
                // TODO: We need to show maintenance screen and return here if it under maintenance
                // Assuming CheckMaintenanceStatusAsync shows a Blocking UI if under maintenance.
                // We rely on the service to handle the UI.
                // If maintenance is strict, we might need a return value, but usually it pops a screen.
                await _maintenanceService.CheckMaintenanceStatusAsync();
            }

            // --- STEP 2: Authentication (40% - 70%) ---
            loadingScreen?.UpdateProgress(0.4f, "Authenticating...");

            bool isAuthenticated = _authService.IsLoggedIn();

            if (!isAuthenticated)
            {
                string lastLogin = _saveService.GetSettings().LastLoginMethod;
                if (!string.IsNullOrEmpty(lastLogin) && lastLogin == nameof(AuthType.DeviceID))
                {
                    GameLogger.Log("[Bootstrap] Attempting auto-login with DeviceID");
                    isAuthenticated = await _authService.LoginAsync(AuthType.DeviceID);
                }
                else
                {
                    GameLogger.Log("[Bootstrap] No last login found, skipping auto-login");
                }
            }

            if (!isAuthenticated)
            {
                _uiService.Hide<LoadingScreen>();
                _stateManager.ChangeState<LoginState>();
                return;
            }

            // --- STEP 3: Ready (100%) ---
            loadingScreen?.UpdateProgress(1.0f, "READY!");
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            // _uiService.Hide<LoadingScreen>();
            GameLogger.Log("Initialization complete. Transitioning to SessionLoadingState.");
            _stateManager.ChangeState<SessionLoadingState>();
        }

        private async UniTask ShowSplashScreenAsync()
        {
            _uiService.Show<SplashScreen>();
            await UniTask.Delay(TimeSpan.FromSeconds(SplashDuration));
            _uiService.Hide<SplashScreen>();
            await UniTask.Delay(500);
        }
    }
}