using System;

namespace Playcenter.MobileCore
{
    /// <summary>Exponential backoff with ±25% jitter. Seeded → deterministic in tests and replays.</summary>
    public sealed class BackoffPolicy
    {
        private readonly float _baseSeconds;
        private readonly Random _random;
        private int _attempt;

        public BackoffPolicy(float baseSeconds, int seed)
        {
            _baseSeconds = baseSeconds;
            _random = new Random(seed);
        }

        public float NextDelay()
        {
            float expo = _baseSeconds * (float)Math.Pow(2.0, _attempt);
            _attempt++;
            float jitter = 1f + ((float)_random.NextDouble() - 0.5f) * 0.5f;
            return expo * jitter;
        }

        public void Reset()
        {
            _attempt = 0;
        }
    }
}
