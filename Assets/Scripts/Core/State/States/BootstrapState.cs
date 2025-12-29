using System;
using Core.Authentication;
using Core.Bootstrap;
using Core.Events;
using Core.Logging;
using Core.Maintenance;
using Core.RemoteConfig;
using Core.Update;
using Cysharp.Threading.Tasks;
using UI;
using UI.Screens;
using UnityEngine;
using RecipeRage.Modules.Auth.Core;

namespace Core.State.States
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
        private readonly IMaintenanceService _maintenanceService;
        private readonly IGameStateManager _stateManager;
        private readonly IEventBus _eventBus;
        private readonly ServiceContainer _serviceContainer;

        public BootstrapState(
            IUIService uiService,
            INTPTimeService ntpTimeService,
            IRemoteConfigService remoteConfigService,
            IAuthService authService,
            IMaintenanceService maintenanceService,
            IGameStateManager stateManager,
            IEventBus eventBus,
            ServiceContainer serviceContainer)
        {
            _uiService = uiService;
            _ntpTimeService = ntpTimeService;
            _remoteConfigService = remoteConfigService;
            _authService = authService;
            _maintenanceService = maintenanceService;
            _stateManager = stateManager;
            _eventBus = eventBus;
            _serviceContainer = serviceContainer;
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
                _uiService.ShowScreen(UIScreenType.Loading);
                await InitializeGameSequence();
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
                _uiService.HideScreen(UIScreenType.Loading);
                _stateManager.ChangeState(new LoginState(_uiService, _eventBus, _stateManager, _serviceContainer));
            }
        }

        private async UniTask InitializeGameSequence()
        {
            var loadingScreen = _uiService.GetScreen<LoadingScreen>(UIScreenType.Loading);

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

            // --- STEP 2: Authentication (30% - 60%) ---
            loadingScreen?.UpdateProgress(0.3f, "Authenticating...");
            
            // New logic: Check if logged in, otherwise attempt auto-login with DeviceID
            bool isAuthenticated = _authService.IsLoggedIn();
            if (!isAuthenticated)
            {
                isAuthenticated = await _authService.LoginAsync(AuthType.DeviceID);
            }

            if (!isAuthenticated)
            {
                _uiService.HideScreen(UIScreenType.Loading);
                _stateManager.ChangeState(new LoginState(_uiService, _eventBus, _stateManager, _serviceContainer));
                return;
            }

            // --- STEP 3: Post-Login (60% - 90%) ---
            loadingScreen?.UpdateProgress(0.6f, "Checking Updates...");
            await _remoteConfigService.RefreshConfig();

            var forceUpdateChecker = new ForceUpdateChecker(_remoteConfigService, _uiService);
            await forceUpdateChecker.CheckForUpdateAsync();

            loadingScreen?.UpdateProgress(0.8f, "Checking Maintenance...");
            if (_maintenanceService != null)
            {
                await _maintenanceService.CheckMaintenanceStatusAsync();
            }

            // --- STEP 4: Ready (100%) ---
            loadingScreen?.UpdateProgress(1.0f, "READY!");
            await UniTask.Delay(TimeSpan.FromSeconds(1.0f)); // 1s Delay as requested

            _uiService.HideScreen(UIScreenType.Loading);
            GameLogger.Log("Initialization complete. Transitioning to MainMenu.");
            _stateManager.ChangeState(new MainMenuState());
        }

        private async UniTask ShowSplashScreenAsync()
        {
            _uiService.ShowScreen(UIScreenType.Splash);
            await UniTask.Delay(TimeSpan.FromSeconds(SplashDuration));
            _uiService.HideScreen(UIScreenType.Splash);
            await UniTask.Delay(500);
        }
    }
}