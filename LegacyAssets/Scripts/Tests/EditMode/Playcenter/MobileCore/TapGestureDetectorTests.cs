using NUnit.Framework;
using Playcenter.MobileCore;

namespace RecipeRage.Tests.Playcenter.MobileCore
{
    public sealed class TapGestureDetectorTests
    {
        [Test]
        public void Taps_WithinWindow_AccumulateCount()
        {
            var clock = new ManualClock();
            var detector = new TapGestureDetector(windowSeconds: 0.3f, idleResetSeconds: 0.5f, clock);

            detector.OnTap();
            clock.Tick(0.1f);
            detector.OnTap();
            clock.Tick(0.1f);
            detector.OnTap();

            Assert.AreEqual(3, detector.TapCount);
        }

        [Test]
        public void Idle_BeyondReset_ClearsCount()
        {
            var clock = new ManualClock();
            var detector = new TapGestureDetector(0.3f, 0.5f, clock);

            detector.OnTap();
            detector.OnTap();
            clock.Tick(0.6f);

            Assert.AreEqual(0, detector.TapCount);
        }
    }
}
