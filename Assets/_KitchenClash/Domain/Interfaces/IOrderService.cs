using System;
using System.Collections.Generic;

namespace KitchenClash.Domain
{
    public interface IOrderService
    {
        IReadOnlyList<OrderModel> ActiveOrders { get; }
        OrderModel GenerateOrder(float matchTimeRemaining);
        CompleteResult CompleteOrder(Guid orderId, float timeLeft, int combo);
        void ExpireOrder(Guid orderId);
        bool TryGetBestActiveOrder<T>(IReadOnlyList<T> orders, Func<T, bool> predicate, out T bestOrder);
        float GetRemainingTime(RecipeOrderState order, float currentTime);
    }
}
}
