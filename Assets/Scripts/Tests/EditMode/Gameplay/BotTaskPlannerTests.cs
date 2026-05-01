using System.Collections.Generic;
using KitchenClash.Application.Services;
using NUnit.Framework;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    public class BotTaskPlannerTests
    {
        [Test]
        public void Plan_ReturnsWanderForEmptySnapshot()
        {
            BotTaskPlanner planner = new();
            BotPlanningSnapshot snapshot = new();

            BotTaskPlan plan = planner.Plan(snapshot);

            Assert.AreEqual(BotTaskType.Wander, plan.Type);
        }

        [Test]
        public void Plan_ReturnsWanderWhenNoOrdersAvailable()
        {
            BotTaskPlanner planner = new();
            BotPlanningSnapshot snapshot = new()
            {
                Orders = new List<BotOrderDescriptor>()
            };

            BotTaskPlan plan = planner.Plan(snapshot);

            Assert.AreEqual(BotTaskType.Wander, plan.Type);
        }

        [Test]
        public void Plan_ReturnsWanderWithExpiredOrders()
        {
            BotTaskPlanner planner = new();
            BotPlanningSnapshot snapshot = new()
            {
                Orders = new List<BotOrderDescriptor>
                {
                    new()
                    {
                        OrderId = 10,
                        RecipeId = 3001,
                        IsExpired = true
                    }
                }
            };

            BotTaskPlan plan = planner.Plan(snapshot);

            Assert.AreEqual(BotTaskType.Wander, plan.Type);
        }

        [Test]
        public void Plan_ReturnsNonNullPlan()
        {
            BotTaskPlanner planner = new();
            BotPlanningSnapshot snapshot = new()
            {
                ClaimedOrderId = 25,
                Orders = new List<BotOrderDescriptor>
                {
                    new()
                    {
                        OrderId = 25,
                        RecipeId = 3002,
                        RemainingTime = 15f
                    }
                }
            };

            BotTaskPlan plan = planner.Plan(snapshot);

            Assert.IsNotNull(plan);
        }

        [Test]
        public void IdlePlan_HasCorrectType()
        {
            BotTaskPlan idle = BotTaskPlan.Idle();

            Assert.AreEqual(BotTaskType.Idle, idle.Type);
        }
    }
}
