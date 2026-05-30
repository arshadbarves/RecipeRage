using System;
using System.Collections.Generic;
using KitchenClash.Domain;

namespace KitchenClash.Application.Services
{
    public sealed class BotTaskPlanner
    {
        private BotDifficultyConfig _config;
        private readonly Random _random;

        public BotTaskPlanner() : this(BotDifficulty.Medium) { }

        public BotTaskPlanner(BotDifficulty difficulty)
        {
            _config = BotDifficultyConfig.FromDifficulty(difficulty);
            _random = new Random();
        }

        public void SetDifficulty(BotDifficulty difficulty)
        {
            _config = BotDifficultyConfig.FromDifficulty(difficulty);
        }

        public BotDifficultyConfig Config => _config;

        public BotTaskPlan Plan(BotPlanningSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return BotTaskPlan.Idle();
            }

            // Priority 1: Extinguish fires
            BotTaskPlan firePlan = TryPlanExtinguishFire(snapshot);
            if (firePlan != null)
            {
                return firePlan;
            }

            // Priority 2: Holding cooked item → deliver to serving
            BotTaskPlan deliverPlan = TryPlanDeliverToServing(snapshot);
            if (deliverPlan != null)
            {
                return deliverPlan;
            }

            // Priority 3: Holding cut/prepped item → bring to cooking
            BotTaskPlan cookingPlan = TryPlanBringToCooking(snapshot);
            if (cookingPlan != null)
            {
                return cookingPlan;
            }

            // Priority 4: Holding raw ingredient → bring to prep
            BotTaskPlan prepPlan = TryPlanBringToPrep(snapshot);
            if (prepPlan != null)
            {
                return prepPlan;
            }

            // Priority 5: Holding burned item → recover (drop it)
            if (snapshot.IsHoldingItem && snapshot.HeldItemIsBurned)
            {
                return new BotTaskPlan
                {
                    Type = BotTaskType.Recover,
                    DelayBeforeAction = _config.ReactionDelay
                };
            }

            // Priority 6: Claim an order if we don't have one
            BotTaskPlan claimPlan = TryPlanClaimOrder(snapshot);
            if (claimPlan != null)
            {
                return claimPlan;
            }

            // Priority 7: Fetch ingredient for active order
            BotTaskPlan fetchPlan = TryPlanFetchIngredient(snapshot);
            if (fetchPlan != null)
            {
                return fetchPlan;
            }

            // Priority 8: Idle with wander
            return new BotTaskPlan
            {
                Type = BotTaskType.Wander,
                DelayBeforeAction = _config.ReactionDelay
            };
        }

        private BotTaskPlan TryPlanExtinguishFire(BotPlanningSnapshot snapshot)
        {
            if (snapshot.StationsOnFire == null || snapshot.StationsOnFire.Count == 0)
            {
                return null;
            }

            if (!_config.CanExtinguishFires)
            {
                return null;
            }

            // Probabilistic: check fire extinguish chance
            if (_config.FireExtinguishChance < 1.0f && _random.NextDouble() > _config.FireExtinguishChance)
            {
                return null;
            }

            // Don't extinguish while holding something
            if (snapshot.IsHoldingItem)
            {
                return null;
            }

            string fireStation = snapshot.StationsOnFire[0];
            return new BotTaskPlan
            {
                Type = BotTaskType.ExtinguishFire,
                TargetStationId = fireStation,
                DelayBeforeAction = _config.ReactionDelay
            };
        }

        private BotTaskPlan TryPlanDeliverToServing(BotPlanningSnapshot snapshot)
        {
            if (!snapshot.IsHoldingItem || !snapshot.HeldItemIsCooked)
            {
                return null;
            }

            // Also handle plates
            if (snapshot.HeldItemIsPlate)
            {
                string servingId = PickFirst(snapshot.ServingStationIds);
                if (servingId != null)
                {
                    return new BotTaskPlan
                    {
                        Type = BotTaskType.DeliverToServing,
                        TargetStationId = servingId,
                        DelayBeforeAction = _config.ReactionDelay
                    };
                }
            }

            // Cooked ingredient → serve or assemble
            string targetServing = PickFirst(snapshot.ServingStationIds);
            if (targetServing != null)
            {
                return new BotTaskPlan
                {
                    Type = BotTaskType.DeliverToServing,
                    TargetStationId = targetServing,
                    DelayBeforeAction = _config.ReactionDelay
                };
            }

            return null;
        }

        private BotTaskPlan TryPlanBringToCooking(BotPlanningSnapshot snapshot)
        {
            if (!snapshot.IsHoldingItem || !snapshot.HeldItemIsCut)
            {
                return null;
            }

            if (snapshot.HeldItemIsCooked || snapshot.HeldItemIsBurned)
            {
                return null;
            }

            string cookingId = PickFirst(snapshot.CookingStationIds);
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

        private BotTaskPlan TryPlanBringToPrep(BotPlanningSnapshot snapshot)
        {
            if (!snapshot.IsHoldingItem || !snapshot.HeldItemIsRaw)
            {
                return null;
            }

            if (snapshot.HeldItemIsCut || snapshot.HeldItemIsCooked || snapshot.HeldItemIsBurned)
            {
                return null;
            }

            string prepId = PickFirst(snapshot.PrepStationIds);
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

        private BotTaskPlan TryPlanClaimOrder(BotPlanningSnapshot snapshot)
        {
            if (snapshot.ClaimedOrderId.HasValue)
            {
                return null;
            }

            if (snapshot.Orders == null || snapshot.Orders.Count == 0)
            {
                return null;
            }

            // Pick highest priority unclaimed order
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

        private BotTaskPlan TryPlanFetchIngredient(BotPlanningSnapshot snapshot)
        {
            if (snapshot.IsHoldingItem)
            {
                return null;
            }

            if (!snapshot.ClaimedOrderId.HasValue)
            {
                return null;
            }

            string ingredientStationId = PickFirst(snapshot.IngredientStationIds);
            if (ingredientStationId == null)
            {
                return null;
            }

            // Determine ingredient to fetch - apply mistake chance
            IngredientType targetIngredient = DetermineIngredientToFetch(snapshot);

            return new BotTaskPlan
            {
                Type = BotTaskType.FetchIngredient,
                TargetStationId = ingredientStationId,
                TargetIngredient = targetIngredient,
                OrderId = snapshot.ClaimedOrderId,
                DelayBeforeAction = _config.ReactionDelay
            };
        }

        private IngredientType DetermineIngredientToFetch(BotPlanningSnapshot snapshot)
        {
            // If mistake chance triggers, pick a random available ingredient
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

            // Otherwise pick the first available ingredient (correct one would be recipe-driven)
            if (snapshot.AvailableIngredients != null && snapshot.AvailableIngredients.Length > 0)
            {
                if (Enum.TryParse<IngredientType>(snapshot.AvailableIngredients[0], true, out IngredientType correctType))
                {
                    return correctType;
                }
            }

            return IngredientType.None;
        }

        private static string PickFirst(List<string> ids)
        {
            return ids != null && ids.Count > 0 ? ids[0] : null;
        }
    }
}
