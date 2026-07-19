using KitchenClash.Infrastructure.Input;
using NUnit.Framework;
using Playcenter.Services;

namespace KitchenClash.Tests.EditMode.Playcenter.Input
{
    public sealed class GameplayInputMapperTests
    {
        private const float Epsilon = 0.001f;

        [Test]
        public void FromKeyboard_WOnly_ReturnsUp()
        {
            InputAxis2 axis = GameplayInputMapper.FromKeyboard(
                w: true, a: false, s: false, d: false,
                up: false, left: false, down: false, right: false);

            Assert.AreEqual(0f, axis.X, Epsilon);
            Assert.AreEqual(1f, axis.Y, Epsilon);
        }

        [Test]
        public void FromKeyboard_WD_ReturnsNormalizedDiagonal()
        {
            InputAxis2 axis = GameplayInputMapper.FromKeyboard(
                w: true, a: false, s: false, d: true,
                up: false, left: false, down: false, right: false);

            float expected = 1f / (float)System.Math.Sqrt(2d);
            Assert.AreEqual(expected, axis.X, Epsilon);
            Assert.AreEqual(expected, axis.Y, Epsilon);
            Assert.AreEqual(1f, axis.Magnitude, Epsilon);
        }

        [Test]
        public void FromKeyboard_NoKeys_ReturnsZero()
        {
            InputAxis2 axis = GameplayInputMapper.FromKeyboard(
                false, false, false, false, false, false, false, false);

            Assert.AreEqual(0f, axis.X, Epsilon);
            Assert.AreEqual(0f, axis.Y, Epsilon);
        }

        [Test]
        public void FromVirtualStick_InsideDeadzone_ReturnsZero()
        {
            InputAxis2 axis = GameplayInputMapper.FromVirtualStick(0.05f, 0f, deadZone: 0.1f);

            Assert.AreEqual(0f, axis.X, Epsilon);
            Assert.AreEqual(0f, axis.Y, Epsilon);
        }

        [Test]
        public void FromVirtualStick_FullRight_ReturnsUnitX()
        {
            InputAxis2 axis = GameplayInputMapper.FromVirtualStick(1f, 0f, deadZone: 0.1f);

            Assert.AreEqual(1f, axis.X, Epsilon);
            Assert.AreEqual(0f, axis.Y, Epsilon);
        }

        [Test]
        public void FromVirtualStick_OverLength_ClampsToOne()
        {
            InputAxis2 axis = GameplayInputMapper.FromVirtualStick(2f, 2f, deadZone: 0.1f);

            Assert.AreEqual(1f, axis.Magnitude, Epsilon);
        }

        [Test]
        public void GameplayInputService_Publish_UpdatesReadSnapshot()
        {
            var service = new GameplayInputService();
            service.Publish(new InputAxis2(0.5f, -0.5f), interactPressed: true, abilityPressed: false);

            Assert.AreEqual(0.5f, service.Move.X, Epsilon);
            Assert.AreEqual(-0.5f, service.Move.Y, Epsilon);
            Assert.IsTrue(service.InteractPressed);
            Assert.IsFalse(service.AbilityPressed);

            IGameplayInput read = service;
            Assert.AreEqual(0.5f, read.Move.X, Epsilon);
        }
    }
}
