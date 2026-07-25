using System.Collections.Generic;

namespace KitchenClash.Application.Services.Evaluators
{
    public static class EvaluatorHelpers
    {
        public static string PickFirst(List<string> ids)
        {
            return ids != null && ids.Count > 0 ? ids[0] : null;
        }
    }

    /// <summary>Priority 8 (terminal): always returns a wander task. Registered last.</summary>
    public sealed class WanderEvaluator : Playcenter.MobileCore.ITaskEvaluator<BotPlanningSnapshot, BotTaskPlan>
    {
        private readonly BotDifficultyConfig _config;

        public WanderEvaluator(BotDifficultyConfig config)
        {
            _config = config;
        }

        public BotTaskPlan Evaluate(BotPlanningSnapshot snapshot)
        {
            return new BotTaskPlan
            {
                Type = BotTaskType.Wander,
                DelayBeforeAction = _config.ReactionDelay
            };
        }
    }
}
