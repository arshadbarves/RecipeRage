using System;
using Core.Authentication;
using Core.Bootstrap;
using Core.Events;
using Core.Logging;
using Core.Maintenance;
using Core.RemoteConfig;
using Cysharp.Threading.Tasks;
using UI;
using UI.Screens;
using UnityEngine;

namespace Core.State.States
{
    /// <summary>
    /// Initial state of the game. Handles the startup sequence:
    /// Splash -> Auth -> Config -> Maintenance -> Main Menu
    /// </summary>
    public class BootstrapState : BaseState
    {
        private const float SplashDuration = 2.0f;
        private const float MinLoadingTime = 2.0f;
        private readonly IUIService _uiService;
        private readonly INTPTimeService _ntpTimeService;
        private readonly IRemoteConfigService _remoteConfigService;
        private readonly IAuthenticationService _authService;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IGameStateManager _stateManager;
        private readonly IEventBus _eventBus;
        private readonly ServiceContainer _serviceContainer;

        public BootstrapState(
            IUIService uiService,
            INTPTimeService ntpTimeService,
            IRemoteConfigService remoteConfigService,
            IAuthenticationService authService,
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
            GameLogger.Log("Entering game initialization sequence");

            try
            {
                // 1. Show Splash Screen
                await ShowSplashScreenAsync();

                await InitializeFoundationAsync();

                bool isAuthenticated = await InitializeAuthenticationAsync();

                if (isAuthenticated)
                {
                    // Create the GameSession immediately after successful authentication
                    _serviceContainer.CreateSession();

                    await PerformPostLoginChecksAsync();

                    GameLogger.Log("Initialization complete. Transitioning to MainMenu.");
                    _stateManager.ChangeState(new MainMenuState());
                }
                else
                {
                    GameLogger.Log("Not authenticated. Transitioning to Login.");
                    _stateManager.ChangeState(new LoginState(_uiService, _eventBus, _stateManager, _serviceContainer));
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
                _stateManager.ChangeState(new LoginState(_uiService, _eventBus, _stateManager, _serviceContainer));
            }
        }

        private async UniTask ShowSplashScreenAsync()
        {
            _uiService.ShowScreen(UIScreenType.Splash);

            await UniTask.Delay(TimeSpan.FromSeconds(SplashDuration));

            _uiService.HideScreen(UIScreenType.Splash);
            await UniTask.Delay(500);
        }

        private async UniTask InitializeFoundationAsync()
        {
            GameLogger.Log("Initializing Foundation...");

            try
            {
                var cts = new System.Threading.CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(10.0f));

                var (isCanceled, ntpSynced) = await _ntpTimeService.SyncTime()
                    .AttachExternalCancellation(cts.Token)
                    .SuppressCancellationThrow();

                if (isCanceled)
                {
                    GameLogger.LogWarning("NTP sync timed out, using local time.");
                }
                else if (!ntpSynced)
                {
                    GameLogger.LogWarning("NTP sync failed, using local time.");
                }

                cts.Dispose();
            }
            catch (Exception ex)
            {
                GameLogger.LogWarning($"NTP sync exception (non-critical): {ex.Message}");
            }

            await _remoteConfigService.Initialize();
        }

        private async UniTask<bool> InitializeAuthenticationAsync()
        {
            GameLogger.Log("Initializing Authentication...");
            return await _authService.InitializeAsync();
        }

        private async UniTask PerformPostLoginChecksAsync()
        {
            GameLogger.Log("Performing Post-Login Checks...");

            var startTime = Time.realtimeSinceStartup;

            _uiService.ShowScreen(UIScreenType.Loading);
            var loadingScreen = _uiService.GetScreen<LoadingScreen>(UIScreenType.Loading);

            loadingScreen?.UpdateProgress(0.3f, "Loading configuration...");
            await _remoteConfigService.RefreshConfig();

            loadingScreen?.UpdateProgress(0.5f, "Checking for updates...");
            var forceUpdateChecker = new ForceUpdateChecker(
                _remoteConfigService,
                _uiService);
            await forceUpdateChecker.CheckForUpdate();

            loadingScreen?.UpdateProgress(0.7f, "Checking server status...");
            if (_maintenanceService != null)
            {
                await _maintenanceService.CheckMaintenanceStatusAsync();
            }

            loadingScreen?.UpdateProgress(1.0f, "Ready!");
            await UniTask.Delay(500);

            var elapsedTime = Time.realtimeSinceStartup - startTime;
            var remainingTime = MinLoadingTime - elapsedTime;
            if (remainingTime > 0)
            {
                GameLogger.Log($"Waiting {remainingTime:F2}s to meet minimum loading time");
                await UniTask.Delay(TimeSpan.FromSeconds(remainingTime));
            }

            _uiService.HideScreen(UIScreenType.Loading);
        }
    }
}