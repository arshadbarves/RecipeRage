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
                if (_appFlow != null)
                {
                    _appFlow.EnterSidePhase(FlowPhaseId.Login);
                }
                else
                {
                    _stateManager.ChangeState<LoginState>();
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
