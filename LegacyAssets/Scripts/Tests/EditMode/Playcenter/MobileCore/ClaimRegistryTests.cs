using NUnit.Framework;
using Playcenter.MobileCore;

namespace RecipeRage.Tests.Playcenter.MobileCore
{
    public sealed class ClaimRegistryTests
    {
        [Test]
        public void TryClaim_FirstClaimSucceeds_SecondBotFails()
        {
            var registry = new ClaimRegistry<int>();

            Assert.IsTrue(registry.TryClaim(7, "bot-a"));
            Assert.IsFalse(registry.TryClaim(7, "bot-b"));
            Assert.IsTrue(registry.IsClaimedByOther(7, "bot-b"));
            Assert.IsFalse(registry.IsClaimedByOther(7, "bot-a"));
        }

        [Test]
        public void Release_FreesClaim_ForOtherBots()
        {
            var registry = new ClaimRegistry<int>();
            registry.TryClaim(7, "bot-a");

            Assert.IsTrue(registry.Release(7, "bot-a"));
            Assert.IsTrue(registry.TryClaim(7, "bot-b"));
        }

        [Test]
        public void Release_ByNonOwner_Fails()
        {
            var registry = new ClaimRegistry<int>();
            registry.TryClaim(7, "bot-a");

            Assert.IsFalse(registry.Release(7, "bot-b"));
        }

        [Test]
        public void TryClaim_EmptyOwner_Fails()
        {
            var registry = new ClaimRegistry<int>();

            Assert.IsFalse(registry.TryClaim(7, ""));
            Assert.IsFalse(registry.TryClaim(7, null));
        }
    }
}
