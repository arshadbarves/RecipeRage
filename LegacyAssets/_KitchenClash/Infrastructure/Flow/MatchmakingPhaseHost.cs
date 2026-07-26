using KitchenClash.Infrastructure.Flow.Handlers;
using VContainer.Unity;

namespace KitchenClash.Infrastructure.Flow
{
    /// <summary>
    /// Root ITickable host for MatchmakingPhase (constructed inside IAppFlow factory with AppFlowProxy).
    /// </summary>
    public sealed class MatchmakingPhaseHost : ITickable
    {
        public MatchmakingPhase Phase { get; set; }

        public void Tick()
        {
            Phase?.Tick();
        }
    }
}
