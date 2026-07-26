namespace Playcenter.MobileCore
{
    /// <summary>Time-sliced budget: up to maxMsPerTick of planning per clock tick (mc_bot_budget_ms, default 2).</summary>
    public sealed class BotBudget : IBotBudget
    {
        private readonly float _maxMsPerTick;
        private float _consumedMs;

        public BotBudget(float maxMsPerTick = 2f)
        {
            _maxMsPerTick = maxMsPerTick;
        }

        public bool TryConsume(float milliseconds)
        {
            if (_consumedMs + milliseconds > _maxMsPerTick)
            {
                return false;
            }

            _consumedMs += milliseconds;
            return true;
        }

        public void ResetTick()
        {
            _consumedMs = 0f;
        }
    }
}
