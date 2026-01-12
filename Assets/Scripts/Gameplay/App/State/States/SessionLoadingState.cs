using Cysharp.Threading.Tasks;
using Gameplay.UI.Features.Loading;
using Gameplay.Economy;
using Gameplay.Persistence;
using Core.Logging;
using Core.Persistence;
using Core.UI.Interfaces;
using Core.Session;
using VContainer;

namespace Gameplay.App.State.States
{
    /// <summary>
    /// State responsible for loading session-specific data (profile, currency, etc.)
    /// before showing the Main Menu.
    /// Progress: 50% (from Bootstrap) -> 90% (before MainMenu)
    /// Loading screen is hidden by MainMenuState after scene loads.
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

            // Get existing loading screen (already shown by BootstrapState)
            var loadingScreen = _uiService.GetScreen<LoadingScreen>();

            try
            {
                // 1. Ensure Session Scope is ready (50% -> 55%)
                loadingScreen?.UpdateProgress(0.55f, "Preparing Session...");
                if (!_sessionManager.IsSessionActive)
                {
                    GameLogger.Log("[SessionLoadingState] Creating new session...");
                    _sessionManager.CreateSession();
                }

                // 2. Sync Cloud/Disk Data (55% -> 65%)
                loadingScreen?.UpdateProgress(0.6f, "Syncing Data...");
                await _saveService.SyncAllCloudDataAsync();

                // 3. Initialize Economy (65% -> 75%)
                loadingScreen?.UpdateProgress(0.7f, "Loading Wallet...");
                var economyService = _sessionManager.SessionContainer.Resolve<EconomyService>();
                economyService.Initialize();

                // 4. Initialize Player Data (75% -> 85%)
                loadingScreen?.UpdateProgress(0.8f, "Loading Progress...");
                var playerDataService = _sessionManager.SessionContainer.Resolve<PlayerDataService>();
                playerDataService.Initialize();

                // 5. Ready - MainMenu will hide loading screen (85% -> 90%)
                loadingScreen?.UpdateProgress(0.9f, "Ready!");
                await UniTask.Delay(300);

                GameLogger.Log("[SessionLoadingState] Loading complete. Transitioning to MainMenu.");
                _stateManager.ChangeState<MainMenuState>();
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ex);
                _stateManager.ChangeState<LoginState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
            // DON'T hide loading screen here - MainMenuState will hide it after scene loads
        }
    }
}