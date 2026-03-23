using System.Collections.Generic;
using Gameplay.Cooking;
using Gameplay.Match;
using NUnit.Framework;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    public class OrderServiceTests
    {
        [Test]
        public void TryGetBestActiveOrder_PicksOldestMatchingLiveOrder()
        {
            List<RecipeOrderState> orders = new()
            {
                new RecipeOrderState { OrderId = 1, RecipeId = 100, CreationTime = 12f, TimeLimit = 30f },
                new RecipeOrderState { OrderId = 2, RecipeId = 200, CreationTime = 4f, TimeLimit = 30f },
                new RecipeOrderState { OrderId = 3, RecipeId = 200, CreationTime = 8f, TimeLimit = 30f, IsExpired = true }
            };

            OrderService service = new();

            bool found = service.TryGetBestActiveOrder(orders, order => order.RecipeId == 200, out RecipeOrderState result);

            Assert.IsTrue(found);
            Assert.AreEqual(2, result.OrderId);
        }

        [Test]
        public void GetRemainingTime_ClampsAtZero()
        {
            RecipeOrderState order = new()
            {
                CreationTime = 10f,
                TimeLimit = 20f
            };

            OrderService service = new();

            Assert.AreEqual(15f, service.GetRemainingTime(order, 15f));
            Assert.AreEqual(0f, service.GetRemainingTime(order, 35f));
        }
    }
}
