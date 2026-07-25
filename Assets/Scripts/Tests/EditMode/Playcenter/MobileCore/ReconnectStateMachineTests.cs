using NUnit.Framework;
using Playcenter.MobileCore;

namespace RecipeRage.Tests.Playcenter.MobileCore
{
    public sealed class ReconnectStateMachineTests
    {
        private static ReconnectConfig MatchConfig() => new ReconnectConfig(
            maxAttempts: 3,
            attemptIntervalSeconds: 5f,
            backoffBaseSeconds: 1f);

        [Test]
        public void MenuMode_RetriesIndefinitely()
        {
            var clock = new ManualClock();
            var sm = new ReconnectStateMachine(
                new ReconnectConfig(maxAttempts: 0, attemptIntervalSeconds: 3f, backoffBaseSeconds: 0f),
                clock,
                seed: 42);

            sm.OnDisconnected();
            for (int i = 0; i < 10; i++)
            {
                clock.Tick(3.1f);
            }

            Assert.AreEqual(ReconnectState.Reconnecting, sm.State);
            Assert.IsTrue(sm.AttemptCount >= 10);
        }

        [Test]
        public void MatchMode_FailsAfterMaxAttempts()
        {
            var clock = new ManualClock();
            var sm = new ReconnectStateMachine(MatchConfig(), clock, seed: 42);

            sm.OnDisconnected();
            for (int i = 0; i < 3; i++)
            {
                clock.Tick(5.1f);
            }
            clock.Tick(5.1f);

            Assert.AreEqual(ReconnectState.Failed, sm.State);
        }

        [Test]
        public void OnConnected_Recovers_ToConnected()
        {
            var clock = new ManualClock();
            var sm = new ReconnectStateMachine(MatchConfig(), clock, seed: 42);

            sm.OnDisconnected();
            clock.Tick(2f);
            sm.OnConnected();

            Assert.AreEqual(ReconnectState.Connected, sm.State);
            Assert.AreEqual(0, sm.AttemptCount);
        }
    }
}
