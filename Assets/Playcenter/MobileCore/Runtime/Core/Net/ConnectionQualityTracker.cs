using System;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// RTT exponential moving average to quality tier for telemetry and UI badges.
    /// degradedThresholdMs / poorThresholdMs from config (mc_reconnect_*).
    /// </summary>
    public sealed class ConnectionQualityTracker
    {
        private readonly float _degradedMs;
        private readonly float _poorMs;
        private readonly float _smoothing;
        private float _emaMs = -1f;

        public ConnectionQuality Quality { get; private set; } = ConnectionQuality.Good;
        public float RttEmaMs => _emaMs;
        public event Action<ConnectionQuality> QualityChanged;

        public ConnectionQualityTracker(float degradedMs = 150f, float poorMs = 400f, float smoothing = 0.2f)
        {
            _degradedMs = degradedMs;
            _poorMs = poorMs;
            _smoothing = smoothing;
        }

        public void Sample(float rttMs)
        {
            _emaMs = _emaMs < 0f ? rttMs : _emaMs + _smoothing * (rttMs - _emaMs);

            ConnectionQuality next =
                _emaMs >= _poorMs ? ConnectionQuality.Poor :
                _emaMs >= _degradedMs ? ConnectionQuality.Degraded :
                ConnectionQuality.Good;

            if (next != Quality)
            {
                Quality = next;
                QualityChanged?.Invoke(next);
            }
        }
    }
}
