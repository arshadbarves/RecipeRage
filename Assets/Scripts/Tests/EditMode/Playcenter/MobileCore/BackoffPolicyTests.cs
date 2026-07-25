using NUnit.Framework;
using Playcenter.MobileCore;

namespace RecipeRage.Tests.Playcenter.MobileCore
{
    public sealed class BackoffPolicyTests
    {
        [Test]
        public void Delay_GrowsExponentially_IsDeterministicPerSeed()
        {
            var a = new BackoffPolicy(baseSeconds: 1f, seed: 42);
            var b = new BackoffPolicy(baseSeconds: 1f, seed: 42);

            float a1 = a.NextDelay();
            float a2 = a.NextDelay();
            float b1 = b.NextDelay();
            float b2 = b.NextDelay();

            Assert.AreEqual(a1, b1, 0.0001f);
            Assert.AreEqual(a2, b2, 0.0001f);
            Assert.Greater(a2, 1.5f); // attempt 2 base = 2s, jitter ±25% keeps above 1.5
        }

        [Test]
        public void Reset_RestartsSequence()
        {
            var policy = new BackoffPolicy(1f, seed: 42);
            float first = policy.NextDelay();
            policy.NextDelay();

            policy.Reset();

            Assert.AreEqual(first, policy.NextDelay(), 0.0001f);
        }
    }
}
