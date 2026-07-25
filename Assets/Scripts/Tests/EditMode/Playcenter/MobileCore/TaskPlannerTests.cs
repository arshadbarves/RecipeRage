using NUnit.Framework;
using Playcenter.MobileCore;

namespace RecipeRage.Tests.Playcenter.MobileCore
{
    public sealed class TaskPlannerTests
    {
        private sealed class Snapshot { public bool HasFire; public bool HasDelivery; }

        private sealed class FireEvaluator : ITaskEvaluator<Snapshot, string>
        {
            public string Evaluate(Snapshot s) => s.HasFire ? "extinguish" : null;
        }

        private sealed class DeliverEvaluator : ITaskEvaluator<Snapshot, string>
        {
            public string Evaluate(Snapshot s) => s.HasDelivery ? "deliver" : null;
        }

        [Test]
        public void Plan_FirstNonNullWins()
        {
            var planner = new TaskPlanner<Snapshot, string>();
            planner.Register(new DeliverEvaluator());
            planner.Register(new FireEvaluator());

            // registered deliver-first → deliver wins even when fire also exists
            Assert.AreEqual("deliver", planner.Plan(new Snapshot { HasFire = true, HasDelivery = true }));
        }

        [Test]
        public void Plan_NullPassesThrough_ToNextEvaluator()
        {
            var planner = new TaskPlanner<Snapshot, string>();
            planner.Register(new FireEvaluator());
            planner.Register(new DeliverEvaluator());

            Assert.AreEqual("deliver", planner.Plan(new Snapshot { HasFire = false, HasDelivery = true }));
        }

        [Test]
        public void Plan_EmptyChain_ReturnsDefault()
        {
            var planner = new TaskPlanner<Snapshot, string>();

            Assert.IsNull(planner.Plan(new Snapshot()));
        }
    }
}
