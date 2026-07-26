using NUnit.Framework;
using Playcenter.SDK;

namespace RecipeRage.Tests.EditMode.StudioSdk
{
    public sealed class ForceUpdateEvaluatorTests
    {
        [Test]
        public void IsUpdateRequired_WhenCurrentLower_ReturnsTrue()
        {
            Assert.IsTrue(ForceUpdateEvaluator.IsUpdateRequired("1.0.0", "1.1.0"));
        }

        [Test]
        public void IsUpdateRequired_WhenCurrentEqual_ReturnsFalse()
        {
            Assert.IsFalse(ForceUpdateEvaluator.IsUpdateRequired("1.1.0", "1.1.0"));
        }

        [Test]
        public void IsUpdateRequired_WhenCurrentHigher_ReturnsFalse()
        {
            Assert.IsFalse(ForceUpdateEvaluator.IsUpdateRequired("2.0.0", "1.1.0"));
        }

        [Test]
        public void IsUpdateRequired_WhenMinimumEmpty_ReturnsFalse()
        {
            Assert.IsFalse(ForceUpdateEvaluator.IsUpdateRequired("1.0.0", null));
            Assert.IsFalse(ForceUpdateEvaluator.IsUpdateRequired("1.0.0", string.Empty));
        }

        [Test]
        public void CompareVersions_MajorMinorPatch_OrdersCorrectly()
        {
            Assert.Less(ForceUpdateEvaluator.CompareVersions("1.0.0", "1.0.1"), 0);
            Assert.Less(ForceUpdateEvaluator.CompareVersions("1.0.9", "1.1.0"), 0);
            Assert.Greater(ForceUpdateEvaluator.CompareVersions("2.0.0", "1.9.9"), 0);
            Assert.AreEqual(0, ForceUpdateEvaluator.CompareVersions("1.2.3", "1.2.3"));
        }
    }
}
