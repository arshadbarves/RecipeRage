using System;
using System.Collections.Generic;
using System.Linq;
using KitchenClash.Domain;

namespace KitchenClash.Application
{
    public sealed class OrderService : IOrderService
    {
        private readonly IConfigService _cfg;
        private readonly List<OrderModel> _activeOrders = new();
        private readonly Random _random = new();

        public OrderService(IConfigService cfg) => _cfg = cfg;

        public IReadOnlyList<OrderModel> ActiveOrders => _activeOrders;

        public event Action<OrderModel> OnOrderGenerated;
        public event Action<OrderModel> OnOrderCompleted;
        public event Action<OrderModel> OnOrderExpired;

        public OrderModel GenerateOrder(float matchTimeRemaining)
        {
            int recipeId = _random.Next(1, 10);
            int tier = recipeId <= 3 ? 1 : recipeId <= 6 ? 2 : 3;
            float baseTime = _cfg.Get("order_base_time", 30f);
            float timeLimit = baseTime + (tier * 10f);

            var order = new OrderModel(Guid.NewGuid(), recipeId, tier, timeLimit, matchTimeRemaining);
            _activeOrders.Add(order);
            OnOrderGenerated?.Invoke(order);
            return order;
        }

        public CompleteResult CompleteOrder(Guid orderId, float timeLeft, int combo)
        {
            var order = _activeOrders.FirstOrDefault(o => o.Id == orderId);
            if (order == null || order.IsCompleted || order.IsExpired)
                return new CompleteResult { Success = false, OrderId = orderId };

            order.IsCompleted = true;
            _activeOrders.Remove(order);

            float speedRatio = Math.Clamp(timeLeft / order.TimeLimit, 0f, 1f);

            var result = new CompleteResult
            {
                Success = true,
                OrderId = orderId,
                TimeBonus = speedRatio,
                ComboMultiplier = combo
            };

            OnOrderCompleted?.Invoke(order);
            return result;
        }

        public void ExpireOrder(Guid orderId)
        {
            var order = _activeOrders.FirstOrDefault(o => o.Id == orderId);
            if (order == null || order.IsCompleted) return;

            order.IsExpired = true;
            _activeOrders.Remove(order);
            OnOrderExpired?.Invoke(order);
        }

        public bool TryGetBestActiveOrder<T>(IReadOnlyList<T> orders, Func<T, bool> predicate, out T bestOrder)
        {
            bestOrder = default;
            foreach (var order in orders)
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
