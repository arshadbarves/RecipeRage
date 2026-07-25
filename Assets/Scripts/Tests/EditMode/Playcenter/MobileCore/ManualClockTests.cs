using NUnit.Framework;
using Playcenter.MobileCore;

namespace RecipeRage.Tests.Playcenter.MobileCore
{
    public sealed class ManualClockTests
    {
        [Test]
        public void Tick_AccumulatesElapsed_AndFiresEvent()
        {
            var clock = new ManualClock();
            float observed = 0f;
            clock.Ticked += dt => observed = dt;

            clock.Tick(0.5f);
            clock.Tick(0.25f);

            Assert.AreEqual(0.75f, clock.Elapsed, 0.0001f);
            Assert.AreEqual(0.25f, clock.DeltaTime, 0.0001f);
            Assert.AreEqual(0.25f, observed, 0.0001f);
        }
    }
}
