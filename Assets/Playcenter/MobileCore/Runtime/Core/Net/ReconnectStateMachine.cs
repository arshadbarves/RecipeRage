using System;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// Connected → Reconnecting → (recovered | Failed). Implements the wiki connectivity
    /// table; all timings from ReconnectConfig. Tick-driven via IGameClock.
    /// </summary>
    public sealed class ReconnectStateMachine
    {
        private readonly ReconnectConfig _config;
        private readonly BackoffPolicy _backoff;
        private float _sinceLastAttempt;

        public ReconnectState State { get; private set; } = ReconnectState.Connected;
        public int AttemptCount { get; private set; }
        public event Action ReconnectAttempted;
        public event Action ReconnectFailed;

        public ReconnectStateMachine(ReconnectConfig config, IGameClock clock, int seed)
        {
            _config = config;
            _backoff = new BackoffPolicy(config.BackoffBaseSeconds, seed);
            clock.Ticked += OnTicked;
        }

        public void OnDisconnected()
        {
            if (State == ReconnectState.Connected)
            {
                State = ReconnectState.Reconnecting;
                _sinceLastAttempt = _config.AttemptIntervalSeconds; // first attempt immediately
                AttemptCount = 0;
                _backoff.Reset();
            }
        }

        public void OnConnected()
        {
            State = ReconnectState.Connected;
            AttemptCount = 0;
            _sinceLastAttempt = 0f;
            _backoff.Reset();
        }

        private void OnTicked(float deltaTime)
        {
            if (State != ReconnectState.Reconnecting)
            {
                return;
            }

            _sinceLastAttempt += deltaTime;
            if (_sinceLastAttempt < _config.AttemptIntervalSeconds)
            {
                return;
            }

            _sinceLastAttempt = 0f;
            AttemptCount++;
            ReconnectAttempted?.Invoke();

            if (_config.MaxAttempts > 0 && AttemptCount >= _config.MaxAttempts)
            {
                State = ReconnectState.Failed;
                ReconnectFailed?.Invoke();
            }
        }
    }
}
