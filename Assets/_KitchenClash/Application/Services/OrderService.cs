using System;
using System.Collections.Generic;
using System.Linq;
using KitchenClash.Application.Services;
using KitchenClash.Domain;

namespace KitchenClash.Application
{
    public sealed class OrderService : IOrderService
    {
        private readonly IConfigService _cfg;
        private readonly RecipeCatalog _catalog;
        private readonly IEventBus _eventBus;
        private readonly List<OrderModel> _activeOrders = new();
        private readonly Random _random = new();

        public OrderService(IConfigService cfg, RecipeCatalog catalog, IEventBus eventBus)
        {
            _cfg = cfg;
            _catalog = catalog;
            _eventBus = eventBus;
        }

        public IReadOnlyList<OrderModel> ActiveOrders => _activeOrders;

        public event Action<OrderModel> OnOrderGenerated;
        public event Action<OrderModel> OnOrderCompleted;
        public event Action<OrderModel> OnOrderExpired;

        public OrderModel GenerateOrder(float matchTimeRemaining)
        {
            IReadOnlyList<RecipeDefinition> allRecipes = _catalog.GetAll();
            if (allRecipes.Count == 0)
            {
                return null;
            }

            // Pick a tier weighted by match time remaining:
            // Early match → more tier 1-2, late match → more tier 2-3
            int maxTier = matchTimeRemaining > 120f ? 2 : 3;
            int minTier = matchTimeRemaining > 180f ? 1 : (matchTimeRemaining > 60f ? 1 : 2);

            var candidates = allRecipes
                .Where(r => r.Tier >= minTier && r.Tier <= maxTier)
                .ToList();

            if (candidates.Count == 0)
            {
                candidates = allRecipes.ToList();
            }

            RecipeDefinition recipe = candidates[_random.Next(candidates.Count)];

            var order = new OrderModel(
                Guid.NewGuid(),
                recipe.RecipeId,
                recipe.Tier,
                recipe.RequiredIngredients,
                recipe.BaseTimeLimitSec,
                matchTimeRemaining
            );
            order.PointValue = recipe.BasePoints;

            _activeOrders.Add(order);
            OnOrderGenerated?.Invoke(order);
            return order;
        }

        public CompleteResult CompleteOrder(Guid orderId, float timeLeft, int combo)
        {
            OrderModel order = _activeOrders.FirstOrDefault(o => o.Id == orderId);
            if (order == null || order.IsCompleted || order.IsExpired)
            {
                return new CompleteResult { Success = false, OrderId = orderId };
            }

            order.IsCompleted = true;
            _activeOrders.Remove(order);

            float speedRatio = Math.Clamp(timeLeft / order.TimeLimit, 0f, 1f);

            var result = new CompleteResult
            {
                Success = true,
                OrderId = orderId,
                TimeBonus = speedRatio,
                ComboMultiplier = combo,
                Score = order.PointValue
            };

            OnOrderCompleted?.Invoke(order);
            _eventBus?.Publish(new SFXEvent(SFXType.OrderComplete));
            return result;
        }

        public void ExpireOrder(Guid orderId)
        {
            OrderModel order = _activeOrders.FirstOrDefault(o => o.Id == orderId);
            if (order == null || order.IsCompleted)
            {
                return;
            }

            order.IsExpired = true;
            _activeOrders.Remove(order);
            OnOrderExpired?.Invoke(order);
            _eventBus?.Publish(new SFXEvent(SFXType.OrderExpired));
        }

        public bool TryGetBestActiveOrder<T>(IReadOnlyList<T> orders, Func<T, bool> predicate, out T bestOrder)
        {
            bestOrder = default;
            foreach (T order in orders)
            {
                if (predicate(order))
                {
                    bestOrder = order;
                    return true;
                }
            }
            return false;
        }

        public float GetRemainingTime(RecipeOrderState order, float currentTime)
        {
            float elapsed = currentTime - order.CreationTime;
            return Math.Max(0f, order.TimeLimit - elapsed);
        }
    }
}
