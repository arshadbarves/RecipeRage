using System;
using KitchenClash.Application.Services;
using KitchenClash.Infrastructure.DI;
using KitchenClash.Application.State;
using Cysharp.Threading.Tasks;
using KitchenClash.Application;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Persistence;
using Playcenter.GameFlow;

namespace KitchenClash.Infrastructure.States
{
    public class SessionLoadingState : BaseState
    {
        private readonly IUIService _uiService;
        private readonly SessionManager _sessionManager;
        private readonly ISessionContext _sessionContext;
        private readonly IGameStateManager _stateManager;
        private readonly IAppFlow _appFlow;

        public SessionLoadingState(
            IUIService uiService,
            SessionManager sessionManager,
            ISessionContext sessionContext,
            IGameStateManager stateManager,
            IAppFlow appFlow = null)
        {
            _uiService = uiService;
            _sessionManager = sessionManager;
            _sessionContext = sessionContext;
            _stateManager = stateManager;
            _appFlow = appFlow;
        }

        public override void Enter()
        {
            base.Enter();
            EnterAsync().Forget();
        }

        private async UniTask EnterAsync()
        {
            GameLogger.Log("[SessionLoadingState] Entered - Loading session data...");

            try
            {
                if (!_sessionManager.IsSessionActive)
                {
                    _sessionManager.CreateSession();
                }
                if (!IsStateActive)
                {
                    return;
                }

                EconomyService economyService = _sessionContext.EconomyService;
                economyService?.Initialize();
                if (!IsStateActive)
                {
                    return;
                }

                PlayerDataService playerDataService = _sessionContext.PlayerDataService;
                playerDataService?.Initialize();
                if (!IsStateActive)
                {
                    return;
                }

                await UniTask.Delay(300, cancellationToken: StateCancellationToken);
                if (!IsStateActive)
                {
                    return;
                }

                GameLogger.Log("[SessionLoadingState] Loading complete. Transitioning to Home.");
                if (_appFlow != null)
                {
                    // Two paths: (1) Login side-phase → CompleteSidePhase returns to Home
                    //            (2) Authenticated cold boot (still Boot) → NotifyBootComplete → Home
                    // CompleteSidePhase is no-op if not in side phase, so call it first.
                    _appFlow.CompleteSidePhase();
                    // If still Boot (side phase path already returned to Home), complete boot.
                    _appFlow.NotifyBootComplete();
                }
                else
                {
                    _stateManager.ChangeState<MainMenuState>();
                }
            }
            catch (OperationCanceledException)
            {
                GameLogger.Log("[SessionLoadingState] Enter cancelled");
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
                _appFlow?.EnterSidePhase(FlowPhaseId.Login);
                _stateManager.ChangeState<LoginState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
