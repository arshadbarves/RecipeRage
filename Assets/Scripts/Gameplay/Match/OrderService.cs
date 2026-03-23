using System;
using System.Collections.Generic;
using Gameplay.Cooking;
using UnityEngine;

namespace Gameplay.Match
{
    /// <summary>
    /// Chooses the oldest valid live order so serving matches the active ticket queue.
    /// </summary>
    public sealed class OrderService : IOrderService
    {
        public bool TryGetBestActiveOrder(
            IReadOnlyList<RecipeOrderState> activeOrders,
            Predicate<RecipeOrderState> canServeOrder,
            out RecipeOrderState order)
        {
            order = default;

            if (activeOrders == null || canServeOrder == null)
            {
                return false;
            }

            bool found = false;

            foreach (RecipeOrderState candidate in activeOrders)
            {
                if (candidate.IsCompleted || candidate.IsExpired)
                {
                    continue;
                }

                if (!canServeOrder(candidate))
                {
                    continue;
                }

                if (!found || candidate.CreationTime < order.CreationTime)
                {
                    order = candidate;
                    found = true;
                }
            }

            return found;
        }

        public float GetRemainingTime(RecipeOrderState order, float currentTime)
        {
            float elapsed = Mathf.Max(0f, currentTime - order.CreationTime);
            return Mathf.Max(0f, order.TimeLimit - elapsed);
        }
    }
}
