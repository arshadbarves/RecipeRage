using KitchenClash.Infrastructure.Flow.Handlers;
using Playcenter.GameFlow;

namespace KitchenClash.Infrastructure.Flow
{
    /// <summary>
    /// Production boot port: drives BootSequence (NTP → RC → force update → maintenance → auth → session).
    /// </summary>
    public sealed class BootFlowPort : IBootPort
    {
        private readonly BootSequence _bootSequence;

        public BootFlowPort(BootSequence bootSequence)
        {
            _bootSequence = bootSequence;
        }

        public void EnterBoot(FlowContext context)
        {
            _ = context;
            _bootSequence?.Start();
        }

        public void ExitBoot()
        {
            _bootSequence?.Cancel();
        }
    }
}
