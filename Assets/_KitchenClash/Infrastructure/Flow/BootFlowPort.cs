using KitchenClash.Application.State;
using KitchenClash.Infrastructure.States;
using Playcenter.GameFlow;

namespace KitchenClash.Infrastructure.Flow
{
    /// <summary>
    /// Production boot port: enters BootstrapState worker.
    /// BootstrapState performs initialization then reports outcomes back to IAppFlow.
    /// </summary>
    public sealed class BootFlowPort : IBootPort
    {
        private readonly IGameStateManager _stateManager;
        private readonly IStateFactory _stateFactory;

        public BootFlowPort(IGameStateManager stateManager, IStateFactory stateFactory = null)
        {
            _stateManager = stateManager;
            _stateFactory = stateFactory;
        }

        public void EnterBoot(FlowContext context)
        {
            if (_stateManager?.CurrentState is BootstrapState)
            {
                return;
            }

            // If state machine has no current state, initialize with BootstrapState.
            // Otherwise, transition normally.
            if (_stateManager?.CurrentState == null)
            {
                var bootstrapState = _stateFactory?.Create<BootstrapState>();
                if (bootstrapState != null)
                {
                    _stateManager.Initialize(bootstrapState);
                }
                else
                {
                    _stateManager?.ChangeState<BootstrapState>();
                }
            }
            else
            {
                _stateManager?.ChangeState<BootstrapState>();
            }
        }

        public void ExitBoot()
        {
        }
    }
}
