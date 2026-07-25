namespace Playcenter.MobileCore
{
    /// <summary>
    /// Reconnect tuning (mc_reconnect_*). maxAttempts 0 = retry forever (menu mode,
    /// wiki connectivity table: menu retries every 3s indefinitely; match = 3 × 5s then forfeit).
    /// </summary>
    public readonly struct ReconnectConfig
    {
        public int MaxAttempts { get; }
        public float AttemptIntervalSeconds { get; }
        public float BackoffBaseSeconds { get; }

        public ReconnectConfig(int maxAttempts, float attemptIntervalSeconds, float backoffBaseSeconds)
        {
            MaxAttempts = maxAttempts;
            AttemptIntervalSeconds = attemptIntervalSeconds;
            BackoffBaseSeconds = backoffBaseSeconds;
        }
    }
}
