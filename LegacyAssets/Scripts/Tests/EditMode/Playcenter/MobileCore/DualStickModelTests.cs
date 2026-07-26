using NUnit.Framework;
using Playcenter.MobileCore;
using Playcenter.Services;

namespace RecipeRage.Tests.Playcenter.MobileCore
{
    public sealed class DualStickModelTests
    {
        private static DualStickModel CreateModel(ManualClock clock)
        {
            return new DualStickModel(new DualStickConfig(deadzone: 0.15f), clock);
        }

        [Test]
        public void Move_BelowDeadzone_ReturnsZero()
        {
            var clock = new ManualClock();
            var model = CreateModel(clock);
            float halfW = 400f, halfH = 400f;

            // small deflection on left half (move stick): ~5% of half-width
            model.OnPointer(new PointerEvent(1, halfW * 0.5f + 0.05f * halfW, halfH, PointerPhase.Moved, halfW, halfH));
            InputFrame frame = model.Tick();

            Assert.AreEqual(0f, frame.Move.X, 0.0001f);
            Assert.AreEqual(0f, frame.Move.Y, 0.0001f);
        }

        [Test]
        public void Tick_IncrementsSequenceNumber()
        {
            var clock = new ManualClock();
            var model = CreateModel(clock);

            InputFrame first = model.Tick();
            InputFrame second = model.Tick();

            Assert.AreEqual(first.SequenceNumber + 1u, second.SequenceNumber);
        }

        [Test]
        public void AimRelease_RaisesFlag_ForExactlyOneTick()
        {
            var clock = new ManualClock();
            var model = CreateModel(clock);
            float halfW = 400f, halfH = 400f;

            // press on right half (aim stick), then release
            model.OnPointer(new PointerEvent(2, halfW * 1.5f, halfH, PointerPhase.Began, halfW, halfH));
            model.Tick();
            model.OnPointer(new PointerEvent(2, halfW * 1.5f, halfH, PointerPhase.Ended, halfW, halfH));

            InputFrame releaseFrame = model.Tick();
            InputFrame nextFrame = model.Tick();

            Assert.IsTrue((releaseFrame.Buttons & InputButtons.AimReleased) != 0);
            Assert.IsTrue((nextFrame.Buttons & InputButtons.AimReleased) == 0);
        }
    }
}
