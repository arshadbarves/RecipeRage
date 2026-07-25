namespace Playcenter.MobileCore
{
    /// <summary>
    /// Multi-tap detector: taps within windowSeconds accumulate; idleResetSeconds
    /// of silence clears the count. Driven by IGameClock ticks (deterministic in tests).
    /// </summary>
    public sealed class TapGestureDetector
    {
        private readonly float _windowSeconds;
        private readonly float _idleResetSeconds;
        private float _sinceLastTap;

        public int TapCount { get; private set; }

        public TapGestureDetector(float windowSeconds, float idleResetSeconds, IGameClock clock)
        {
            _windowSeconds = windowSeconds;
            _idleResetSeconds = idleResetSeconds;
            clock.Ticked += OnTicked;
        }

        public void OnTap()
        {
            TapCount++;
            _sinceLastTap = 0f;
        }

        public void Reset()
        {
            TapCount = 0;
            _sinceLastTap = 0f;
        }

        private void OnTicked(float deltaTime)
        {
            if (TapCount == 0)
            {
                return;
            }

            _sinceLastTap += deltaTime;
            if (_sinceLastTap >= _idleResetSeconds)
            {
                Reset();
            }
        }
    }
}
