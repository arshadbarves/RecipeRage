using System;
using KitchenClash.Application.Services;
using KitchenClash.Infrastructure.DI;
using KitchenClash.Application.State;
using Cysharp.Threading.Tasks;
using KitchenClash.Domain;

namespace KitchenClash.Infrastructure.States
{
    public class SessionLoadingState : BaseState
    {
        private readonly IUIService _uiService;
        private readonly SessionManager _sessionManager;
        private readonly ISessionContext _sessionContext;
        private readonly IGameStateManager _stateManager;

        public SessionLoadingState(
            IUIService uiService,
            SessionManager sessionManager,
            ISessionContext sessionContext,
            IGameStateManager stateManager)
        {
            _uiService = uiService;
            _sessionManager = sessionManager;
            _sessionContext = sessionContext;
            _stateManager = stateManager;
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
                if (!IsStateActive) return;

                var economyService = _sessionContext.EconomyService;
                economyService?.Initialize();
                if (!IsStateActive) return;

                var playerDataService = _sessionContext.PlayerDataService;
                playerDataService?.Initialize();
                if (!IsStateActive) return;

                await UniTask.Delay(300, cancellationToken: StateCancellationToken);
                if (!IsStateActive) return;

                GameLogger.Log("[SessionLoadingState] Loading complete. Transitioning to MainMenu.");
                _stateManager.ChangeState<MainMenuState>();
            }
            catch (OperationCanceledException)
            {
                GameLogger.Log("[SessionLoadingState] Enter cancelled");
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
                _stateManager.ChangeState<LoginState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
