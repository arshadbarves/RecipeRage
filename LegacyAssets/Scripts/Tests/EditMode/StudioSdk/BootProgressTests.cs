using NUnit.Framework;
using Playcenter.SDK;

namespace RecipeRage.Tests.EditMode.StudioSdk
{
    public sealed class BootProgressTests
    {
        [Test]
        public void Report_TwoEqualModules_HalfwayAfterFirstComplete()
        {
            var p = new BootProgress(new[] { ("a", 1f), ("b", 1f) });
            p.Report("a", 1f);
            Assert.AreEqual(0.5f, p.Overall01, 0.001f);
        }

        [Test]
        public void Report_PartialLocal_IncludesFraction()
        {
            var p = new BootProgress(new[] { ("a", 1f), ("b", 1f) });
            p.Report("a", 0.5f);
            Assert.AreEqual(0.25f, p.Overall01, 0.001f);
        }
    }
}
