using Gameplay.Networking.Bot;
using NUnit.Framework;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    public class BotClaimRegistryTests
    {
        [Test]
        public void TryClaimOrder_PreventsDifferentBotFromStealingClaim()
        {
            BotClaimRegistry registry = new();

            Assert.IsTrue(registry.TryClaimOrder(100, "bot-a"));
            Assert.IsFalse(registry.TryClaimOrder(100, "bot-b"));
            Assert.IsTrue(registry.IsOrderClaimedByAnotherBot(100, "bot-b"));
        }

        [Test]
        public void ReleaseOrderForBot_ClearsOrderAndCounterAssignments()
        {
            BotClaimRegistry registry = new();
            registry.TryClaimOrder(200, "bot-a");
            registry.AssignCounter(200, "counter-1");

            registry.ReleaseOrderForBot("bot-a");

            Assert.IsNull(registry.GetClaimedOrderId("bot-a"));
            Assert.IsNull(registry.GetClaimedCounterId(200));
            Assert.IsFalse(registry.IsOrderClaimedByAnotherBot(200, "bot-b"));
        }
    }
}
