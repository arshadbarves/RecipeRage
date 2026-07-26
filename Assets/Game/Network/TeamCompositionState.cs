using Playcenter;
using Playcenter.Services;

namespace RecipeRage.Net
{
    /// <summary>
    /// Shows both teams' chefs for 5 seconds (spec: team compositions, then countdown).
    /// Slice 5 builds the visual; this state owns timing + transition.
    /// </summary>
    public sealed class TeamCompositionState : IGameState
    {
        private const float DurationSec = 5f;
        private float _remaining;

        public void Enter()
        {
            _remaining = DurationSec;
            ServiceLocator.Get<ILoggingService>().Log("[Flow] Team compositions (5s)");
        }

        public void Exit() { }

        public void Update(float deltaTime)
        {
            _remaining -= deltaTime;
            if (_remaining <= 0f)
            {
                ServiceLocator.Get<IGameStateMachine>().ChangeState(new CountdownState());
            }
        }
    }
}
