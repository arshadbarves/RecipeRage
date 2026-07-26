using System;
using KitchenClash.Domain;
using Playcenter.MobileCore;

namespace KitchenClash.Application.Services.Evaluators
{
    /// <summary>Priority 7: fetch ingredient for the claimed order (mistake chance applies).</summary>
    public sealed class FetchIngredientEvaluator : ITaskEvaluator<BotPlanningSnapshot, BotTaskPlan>
    {
        private readonly BotDifficultyConfig _config;
        private readonly Random _random;

        public FetchIngredientEvaluator(BotDifficultyConfig config, Random random)
        {
            _config = config;
            _random = random;
        }

        public BotTaskPlan Evaluate(BotPlanningSnapshot snapshot)
        {
            if (snapshot.IsHoldingItem)
            {
                return null;
            }

            if (!snapshot.ClaimedOrderId.HasValue)
            {
                return null;
            }

            string ingredientStationId = EvaluatorHelpers.PickFirst(snapshot.IngredientStationIds);
            if (ingredientStationId == null)
            {
                return null;
            }

            return new BotTaskPlan
            {
                Type = BotTaskType.FetchIngredient,
                TargetStationId = ingredientStationId,
                TargetIngredient = DetermineIngredientToFetch(snapshot),
                OrderId = snapshot.ClaimedOrderId,
                DelayBeforeAction = _config.ReactionDelay
            };
        }

        private IngredientType DetermineIngredientToFetch(BotPlanningSnapshot snapshot)
        {
            if (_config.MistakeChance > 0f && _random.NextDouble() < _config.MistakeChance)
            {
                if (snapshot.AvailableIngredients != null && snapshot.AvailableIngredients.Length > 0)
                {
                    string randomName = snapshot.AvailableIngredients[_random.Next(snapshot.AvailableIngredients.Length)];
                    if (Enum.TryParse<IngredientType>(randomName, true, out IngredientType mistakeType))
                    {
                        return mistakeType;
                    }
                }
            }

            if (snapshot.AvailableIngredients != null && snapshot.AvailableIngredients.Length > 0)
            {
                if (Enum.TryParse<IngredientType>(snapshot.AvailableIngredients[0], true, out IngredientType correctType))
                {
                    return correctType;
                }
            }

            return IngredientType.None;
        }
    }
}
