using System;
using Core.Bootstrap;
using Core.Logging;
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
        private const float SPLASH_DURATION = 2.0f;
        private const float MIN_LOADING_TIME = 1.0f;

        public override async void Enter()
        {
            base.Enter();
            GameLogger.Log("[BootstrapState] Entering game initialization sequence");

            try
            {
                // 1. Show Splash Screen
                await ShowSplashScreen();

                // 2. Initialize Remote Config & Time
                await InitializeFoundation();

                // 3. Initialize Authentication
                bool isAuthenticated = await InitializeAuthentication();

                // 4. Post-Auth Checks (Maintenance, Updates, etc.)
                if (isAuthenticated)
                {
                    await PerformPostLoginChecks();

                    // 5. Transition to Main Menu
                    GameLogger.Log("[BootstrapState] Initialization complete. Transitioning to MainMenu.");
                    GameBootstrap.Services.StateManager.ChangeState(new MainMenuState());
                }
                else
                {
                    // If not authenticated, go to Login screen
                    GameLogger.Log("[BootstrapState] Not authenticated. Transitioning to Login.");
                    // Note: LoginState isn't created yet in the plan, but we'll assume the UI Service handles the screen
                    // For now, we'll just show the login screen via a LoginState if it existed,
                    // or let the Auth service handle the UI trigger.
                    // Based on existing code, we should probably transition to a LoginState.
                    // Since LoginState might not exist in the file list I saw, I'll check if I need to create it or if I can just show the screen.
                    // The original code did: Services.UIService.ShowScreen(UIScreenType.Login);
                    // But we want to use States. Let's assume we'll create/use a LoginState or similar.
                    // For now, I'll use a placeholder or direct UI call if State doesn't exist, but the goal is States.
                    // Let's stick to the plan: "Transition to MainMenuState (or LoginState if auth fails)".
                    // I will assume LoginState exists or I will create a simple one.
                    // Actually, looking at the file list, I didn't see LoginState. I'll use direct UI for now to match previous behavior
                    // but wrapped in a state if possible.
                    // Wait, the original code had `Services.UIService.ShowScreen(UIScreenType.Login)`.
                    // I'll create a simple LoginState in the next step if needed, or just use the UI service here for now
                    // and leave the state transition for when the user actually logs in.
                    // Actually, better to have a LoginState. I'll add it to the plan if it's missing, or just implement it.
                    // For this file, I'll assume we transition to a LoginState.

                    // For now, to be safe and not break compilation, I'll just show the screen and let the Auth service events drive the next step.
                    // But wait, the StateManager needs a state.
                    // I'll create a basic LoginState in this file or a separate one.
                    // Let's just show the screen and stay in BootstrapState? No, that's bad.
                    // I will create a LoginState.cs as well.

                    // For this specific file, I'll refer to LoginState.
                     GameBootstrap.Services.StateManager.ChangeState(new LoginState());
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
                // In case of critical failure, try to go to Login
                 GameBootstrap.Services.StateManager.ChangeState(new LoginState());
            }
        }

        private async UniTask ShowSplashScreen()
        {
            var uiService = GameBootstrap.Services.UIService;
            uiService.ShowScreen(UIScreenType.Splash);

            // Wait for duration
            await UniTask.Delay(TimeSpan.FromSeconds(SPLASH_DURATION));

            // The SplashScreen.cs logic handles hiding itself via FadeOut usually,
            // or we can explicitly hide it.
            // The original code called ShowForDurationAsync. We can replicate that or just wait here.
            // Let's just wait here and then hide.
            uiService.HideScreen(UIScreenType.Splash);
            await UniTask.Delay(500); // Wait for fade
        }

        private async UniTask InitializeFoundation()
        {
            GameLogger.Log("[BootstrapState] Initializing Foundation...");

            // Sync NTP (non-critical - will fallback to local time if fails)

            try
            {
                // Create a timeout task for 2 seconds
                var timeoutTask = UniTask.Delay(TimeSpan.FromSeconds(2.0f));
                var syncTask = GameBootstrap.Services.NTPTimeService.SyncTime();

                // Wait for either sync to finish or timeout
                // We convert syncTask to non-generic UniTask to get an index from WhenAny
                int completedIndex = await UniTask.WhenAny(syncTask.AsUniTask(), timeoutTask);

                if (completedIndex == 0) // Sync finished first
                {
                    bool ntpSynced = await syncTask;
                    if (!ntpSynced)
                    {
                        GameLogger.LogWarning("[BootstrapState] NTP sync failed, using local time.");
                    }
                }
                else // Timeout
                {
                    GameLogger.LogWarning("[BootstrapState] NTP sync timed out (2s limit). Proceeding with local time.");
                    // We let the sync task continue in background
                    syncTask.Forget();
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogWarning($"[BootstrapState] NTP sync exception (non-critical): {ex.Message}");
            }

            // Init Remote Config
            await GameBootstrap.Services.RemoteConfigService.Initialize();
        }

        private async UniTask<bool> InitializeAuthentication()
        {
            GameLogger.Log("[BootstrapState] Initializing Authentication...");
            return await GameBootstrap.Services.AuthenticationService.InitializeAsync();
        }

        private async UniTask PerformPostLoginChecks()
        {
            GameLogger.Log("[BootstrapState] Performing Post-Login Checks...");

            // Show Loading Screen
            var uiService = GameBootstrap.Services.UIService;
            uiService.ShowScreen(UIScreenType.Loading);
            var loadingScreen = uiService.GetScreen<LoadingScreen>(UIScreenType.Loading);

            // 1. Refresh Config
            loadingScreen?.UpdateProgress(0.3f, "Loading configuration...");
            await GameBootstrap.Services.RemoteConfigService.RefreshConfig();

            // 2. Force Update Check
            loadingScreen?.UpdateProgress(0.5f, "Checking for updates...");
            // (Logic extracted from GameBootstrap)
            var forceUpdateChecker = new ForceUpdateChecker(
                GameBootstrap.Services.RemoteConfigService,
                GameBootstrap.Services.UIService);
            await forceUpdateChecker.CheckForUpdate();

            // 3. Maintenance Check
            loadingScreen?.UpdateProgress(0.7f, "Checking server status...");
            if (GameBootstrap.Services.MaintenanceService != null)
            {
                await GameBootstrap.Services.MaintenanceService.CheckMaintenanceStatusAsync();
            }

            // 4. Finalize
            loadingScreen?.UpdateProgress(1.0f, "Ready!");
            await UniTask.Delay(500);

            uiService.HideScreen(UIScreenType.Loading);
        }
    }
}
