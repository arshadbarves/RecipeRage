using System;
using System.Collections.Generic;
using Gameplay.Cooking;

namespace Gameplay.Match
{
    /// <summary>
    /// Pure selection logic for resolving which active order a served dish should satisfy.
    /// </summary>
    public interface IOrderService
    {
        bool TryGetBestActiveOrder(
            IReadOnlyList<RecipeOrderState> activeOrders,
            Predicate<RecipeOrderState> canServeOrder,
            out RecipeOrderState order);

        float GetRemainingTime(RecipeOrderState order, float currentTime);
    }
}
