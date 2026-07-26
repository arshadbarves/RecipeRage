namespace Playcenter.GameFlow
{
    /// <summary>
    /// Brawl rule: matchmaking always produces a match (bots after timeout).
    /// Timeout seconds come from game config; this type only documents the contract.
    /// </summary>
    public static class AlwaysResolveMatchPolicy
    {
        public const float DefaultTimeoutSeconds = 30f;

        public static bool ShouldFillWithBots(float searchSeconds, float timeoutSeconds)
        {
            return searchSeconds >= timeoutSeconds;
        }
    }
}
