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

        public BootFlowPort(IGameStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public void EnterBoot(FlowContext context)
        {
            if (_stateManager?.CurrentState is not BootstrapState)
            {
                _stateManager?.ChangeState<BootstrapState>();
            }
        }

        public void ExitBoot()
        {
        }
    }
}
