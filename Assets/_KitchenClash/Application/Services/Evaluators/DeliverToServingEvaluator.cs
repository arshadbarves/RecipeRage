using Playcenter.MobileCore;

namespace KitchenClash.Application.Services.Evaluators
{
    /// <summary>Priority 2: holding cooked item (or plate) → deliver to serving.</summary>
    public sealed class DeliverToServingEvaluator : ITaskEvaluator<BotPlanningSnapshot, BotTaskPlan>
    {
        private readonly BotDifficultyConfig _config;

        public DeliverToServingEvaluator(BotDifficultyConfig config)
        {
            _config = config;
        }

        public BotTaskPlan Evaluate(BotPlanningSnapshot snapshot)
        {
            if (!snapshot.IsHoldingItem || !snapshot.HeldItemIsCooked)
            {
                return null;
            }

            string targetServing = EvaluatorHelpers.PickFirst(snapshot.ServingStationIds);
            if (targetServing == null)
            {
                return null;
            }

            return new BotTaskPlan
            {
                Type = BotTaskType.DeliverToServing,
                TargetStationId = targetServing,
                DelayBeforeAction = _config.ReactionDelay
            };
        }
    }
}
