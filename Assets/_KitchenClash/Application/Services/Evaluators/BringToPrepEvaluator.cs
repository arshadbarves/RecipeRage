using Playcenter.MobileCore;

namespace KitchenClash.Application.Services.Evaluators
{
    /// <summary>Priority 4: holding raw ingredient → bring to prep.</summary>
    public sealed class BringToPrepEvaluator : ITaskEvaluator<BotPlanningSnapshot, BotTaskPlan>
    {
        private readonly BotDifficultyConfig _config;

        public BringToPrepEvaluator(BotDifficultyConfig config)
        {
            _config = config;
        }

        public BotTaskPlan Evaluate(BotPlanningSnapshot snapshot)
        {
            if (!snapshot.IsHoldingItem || !snapshot.HeldItemIsRaw)
            {
                return null;
            }

            if (snapshot.HeldItemIsCut || snapshot.HeldItemIsCooked || snapshot.HeldItemIsBurned)
            {
                return null;
            }

            string prepId = EvaluatorHelpers.PickFirst(snapshot.PrepStationIds);
            if (prepId == null)
            {
                return null;
            }

            return new BotTaskPlan
            {
                Type = BotTaskType.BringToPrep,
                TargetStationId = prepId,
                TargetIngredient = snapshot.HeldIngredientType,
                DelayBeforeAction = _config.ReactionDelay
            };
        }
    }
}
