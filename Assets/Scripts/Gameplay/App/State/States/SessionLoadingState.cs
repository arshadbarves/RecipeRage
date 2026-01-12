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

            _uiService.Show<LoadingScreen>();
            var loadingScreen = _uiService.GetScreen<LoadingScreen>();
            loadingScreen?.UpdateProgress(0f, "Loading Profile...");

            try
            {
                // 1. Ensure Session Scope is ready
                if (!_sessionManager.IsSessionActive)
                {
                    GameLogger.Log("[SessionLoadingState] No active session found. Creating new session...");
                    _sessionManager.CreateSession();
                }

                // 2. Sync Cloud/Disk Data
                loadingScreen?.UpdateProgress(0.2f, "Syncing Data...");
                await _saveService.SyncAllCloudDataAsync();

                // 3. Initialize Economy (Currency, Inventory)
                loadingScreen?.UpdateProgress(0.4f, "Updating Wallet...");
                var economyService = _sessionManager.SessionContainer.Resolve<EconomyService>();
                economyService.Initialize();

                // 4. Initialize Player Data (Progress, Stats)
                loadingScreen?.UpdateProgress(0.6f, "Loading Progress...");
                var playerDataService = _sessionManager.SessionContainer.Resolve<PlayerDataService>();
                playerDataService.Initialize();

                // 5. Finalize
                loadingScreen?.UpdateProgress(0.9f, "Finalizing...");
                await UniTask.Delay(500);

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
            _uiService.Hide<LoadingScreen>();
        }
    }
}