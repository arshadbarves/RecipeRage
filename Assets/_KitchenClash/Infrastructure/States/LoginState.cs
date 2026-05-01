using KitchenClash.Application.State;
using KitchenClash.Application.Services;
using KitchenClash.Domain;

namespace KitchenClash.Infrastructure.States
{
    public class LoginState : BaseState
    {
        private readonly IUIService _uiService;
        private readonly IEventBus _eventBus;
        private readonly IGameStateManager _stateManager;

        public LoginState(IUIService uiService, IEventBus eventBus, IGameStateManager stateManager)
        {
            _uiService = uiService;
            _eventBus = eventBus;
            _stateManager = stateManager;
        }

        public override void Enter()
        {
            base.Enter();
            GameLogger.Log("[LoginState] Entered - Subscribing to events");
        }

        public override void Exit()
        {
            base.Exit();
            GameLogger.Log("[LoginState] Exiting - Unsubscribing");
        }
    }
}
