using Cysharp.Threading.Tasks;
using UI;
using Modules.Logging;
using Modules.Persistence;
using Modules.Core.Banking.Interfaces;
using UI.Screens;
using Modules.Shared.Interfaces;
using Modules.UI;
using VContainer;

namespace App.State.States
{
    /// <summary>
    /// State responsible for loading session-specific data (profile, currency, etc.)
    /// before showing the Main Menu to prevent UI pop-in.
    /// </summary>
    public class SessionLoadingState : BaseState
    {
        private readonly IUIService _uiService;
        private readonly ISaveService _saveService;
        private readonly SessionManager _sessionManager;
        private readonly IGameStateManager _stateManager;

        public SessionLoadingState(
            IUIService uiService,
            ISaveService saveService,
            SessionManager sessionManager,
            IGameStateManager stateManager)
        {
            _uiService = uiService;
            _saveService = saveService;
            _sessionManager = sessionManager;
            _stateManager = stateManager;
        }

        public override async void Enter()
        {
            base.Enter();
            GameLogger.Log("[SessionLoadingState] Entered - Loading session data...");

            // Show Loading Screen
            _uiService.ShowScreen(UIScreenType.Loading);
            var loadingScreen = _uiService.GetScreen<LoadingScreen>();
            if (loadingScreen != null)
            {
                loadingScreen.UpdateProgress(0f, "Loading Profile...");
            }

            try
            {
                // 1. Ensure Session Scope is ready
                if (!_sessionManager.IsSessionActive)
                {
                    GameLogger.LogError("[SessionLoadingState] No active session found!");
                    _stateManager.ChangeState<LoginState>();
                    return;
                }

                // 2. Sync Cloud/Disk Data
                if (loadingScreen != null) loadingScreen.UpdateProgress(0.3f, "Syncing Data...");
                await _saveService.SyncAllCloudDataAsync();

                // 3. Load Currency (Refresh from SaveService)
                if (loadingScreen != null) loadingScreen.UpdateProgress(0.6f, "Updating Wallet...");
                
                // Resolve BankService from Session Scope
                var bankService = _sessionManager.SessionContainer.Resolve<IBankService>();
                await bankService.InitializeAsync();

                // 4. Simulate a small delay for visual smoothness if everything was too fast
                // (Optional, but often requested to prevent flickering loading screens)
                if (loadingScreen != null) loadingScreen.UpdateProgress(0.9f, "Finalizing...");
                await UniTask.Delay(500);

                GameLogger.Log("[SessionLoadingState] Loading complete. Transitioning to MainMenu.");
                _stateManager.ChangeState<MainMenuState>();
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ex);
                // On critical failure, maybe go back to login or show error?
                // For now, try to proceed or go back to login.
                _stateManager.ChangeState<LoginState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
            // Hide loading screen is handled by the next state (MainMenu usually hides others), 
            // but explicit hiding is safer if MainMenu is an Overlay/Screen combo.
            // MainMenuState calls ShowScreen(MainMenu), which (if it's a Screen category) usually hides others.
            // But LoadingScreen is likely a Screen or Overlay.
            _uiService.HideScreen(UIScreenType.Loading);
        }
    }
}