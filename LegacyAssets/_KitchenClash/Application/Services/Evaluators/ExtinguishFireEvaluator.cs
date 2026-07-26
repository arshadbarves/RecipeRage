using System;
using Playcenter.MobileCore;

namespace KitchenClash.Application.Services.Evaluators
{
    /// <summary>Priority 1: extinguish fires.</summary>
    public sealed class ExtinguishFireEvaluator : ITaskEvaluator<BotPlanningSnapshot, BotTaskPlan>
    {
        private readonly BotDifficultyConfig _config;
        private readonly Random _random;

        public ExtinguishFireEvaluator(BotDifficultyConfig config, Random random)
        {
            _config = config;
            _random = random;
        }

        public BotTaskPlan Evaluate(BotPlanningSnapshot snapshot)
        {
            if (snapshot.StationsOnFire == null || snapshot.StationsOnFire.Count == 0)
            {
                return null;
            }

            if (!_config.CanExtinguishFires)
            {
                return null;
            }

            if (_config.FireExtinguishChance < 1.0f && _random.NextDouble() > _config.FireExtinguishChance)
            {
                return null;
            }

            if (snapshot.IsHoldingItem)
            {
                return null;
            }

            return new BotTaskPlan
            {
                Type = BotTaskType.ExtinguishFire,
                TargetStationId = snapshot.StationsOnFire[0],
                DelayBeforeAction = _config.ReactionDelay
            };
        }
    }
}
