using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
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

            OrderService service = new(new StubConfigService(), new RecipeCatalog(new StubConfigService()));

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

            OrderService service = new(new StubConfigService(), new RecipeCatalog(new StubConfigService()));

            Assert.AreEqual(15f, service.GetRemainingTime(order, 15f));
            Assert.AreEqual(0f, service.GetRemainingTime(order, 35f));
        }

        private sealed class StubConfigService : IConfigService
        {
            public T Get<T>(string key, T fallback) => fallback;
            public Task FetchAsync() => Task.CompletedTask;
        }
    }
}
