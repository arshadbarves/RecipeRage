using Playcenter.MobileCore;

namespace KitchenClash.Application.Services.Evaluators
{
    /// <summary>Priority 5: holding burned item → recover (drop it).</summary>
    public sealed class RecoverBurnedEvaluator : ITaskEvaluator<BotPlanningSnapshot, BotTaskPlan>
    {
        private readonly BotDifficultyConfig _config;

        public RecoverBurnedEvaluator(BotDifficultyConfig config)
        {
            _config = config;
        }

        public BotTaskPlan Evaluate(BotPlanningSnapshot snapshot)
        {
            if (!snapshot.IsHoldingItem || !snapshot.HeldItemIsBurned)
            {
                return null;
            }

            return new BotTaskPlan
            {
                Type = BotTaskType.Recover,
                DelayBeforeAction = _config.ReactionDelay
            };
        }
    }
}
