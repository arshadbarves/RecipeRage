using Playcenter.MobileCore;

namespace KitchenClash.Application.Services.Evaluators
{
    /// <summary>Priority 6: claim highest-priority unclaimed order.</summary>
    public sealed class ClaimOrderEvaluator : ITaskEvaluator<BotPlanningSnapshot, BotTaskPlan>
    {
        private readonly BotDifficultyConfig _config;

        public ClaimOrderEvaluator(BotDifficultyConfig config)
        {
            _config = config;
        }

        public BotTaskPlan Evaluate(BotPlanningSnapshot snapshot)
        {
            if (snapshot.ClaimedOrderId.HasValue)
            {
                return null;
            }

            if (snapshot.Orders == null || snapshot.Orders.Count == 0)
            {
                return null;
            }

            BotOrderDescriptor best = null;
            foreach (BotOrderDescriptor order in snapshot.Orders)
            {
                if (order.IsExpired || order.IsCompleted)
                {
                    continue;
                }

                if (best == null || order.Priority > best.Priority)
                {
                    best = order;
                }
            }

            if (best == null)
            {
                return null;
            }

            return new BotTaskPlan
            {
                Type = BotTaskType.ClaimOrder,
                OrderId = best.OrderId,
                DelayBeforeAction = _config.ReactionDelay
            };
        }
    }
}
