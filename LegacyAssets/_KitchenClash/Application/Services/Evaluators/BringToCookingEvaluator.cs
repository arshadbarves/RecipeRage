using Playcenter.MobileCore;

namespace KitchenClash.Application.Services.Evaluators
{
    /// <summary>Priority 3: holding cut/prepped item → bring to cooking.</summary>
    public sealed class BringToCookingEvaluator : ITaskEvaluator<BotPlanningSnapshot, BotTaskPlan>
    {
        private readonly BotDifficultyConfig _config;

        public BringToCookingEvaluator(BotDifficultyConfig config)
        {
            _config = config;
        }

        public BotTaskPlan Evaluate(BotPlanningSnapshot snapshot)
        {
            if (!snapshot.IsHoldingItem || !snapshot.HeldItemIsCut)
            {
                return null;
            }

            if (snapshot.HeldItemIsCooked || snapshot.HeldItemIsBurned)
            {
                return null;
            }

            string cookingId = EvaluatorHelpers.PickFirst(snapshot.CookingStationIds);
            if (cookingId == null)
            {
                return null;
            }

            return new BotTaskPlan
            {
                Type = BotTaskType.BringToCooking,
                TargetStationId = cookingId,
                TargetIngredient = snapshot.HeldIngredientType,
                DelayBeforeAction = _config.ReactionDelay
            };
        }
    }
}
