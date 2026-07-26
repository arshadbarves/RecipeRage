using Playcenter;
using Playcenter.Services;

namespace RecipeRage.Net
{
    /// <summary>
    /// 3-2-1 countdown, then the match begins. Timing driven by the server via
    /// NetworkMatch.RemainingSeconds in networked games; local fallback here for
    /// dev/offline so the flow is always testable.
    /// </summary>
    public sealed class CountdownState : IGameState
    {
        private const float DurationSec = 3f;
        private float _remaining;
        private int _lastWhole = 4;

        public void Enter()
        {
            _remaining = DurationSec;
        }

        public void Exit() { }

        public void Update(float deltaTime)
        {
            _remaining -= deltaTime;
            var whole = (int)_remaining + 1;
            if (whole < _lastWhole && whole >= 1)
            {
                _lastWhole = whole;
                ServiceLocator.Get<IAudioService>().Play(SfxId.Countdown);
            }

            if (_remaining <= 0f)
            {
                ServiceLocator.Get<IGameStateMachine>().ChangeState(new MatchRuntimeState());
            }
        }
    }
}
