using System;
using System.Threading.Tasks;
using KitchenClash.Application;
using KitchenClash.Domain;
using NUnit.Framework;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    public class ConfigServiceTests
    {
        [Test]
        public void Get_ReturnsFallbackValue()
        {
            FallbackConfigService service = new();

            Assert.AreEqual(180, service.Get("missing.int", 180));
            Assert.AreEqual(1.25f, service.Get("missing.float", 1.25f));
            Assert.IsFalse(service.Get("missing.bool", false));
            Assert.AreEqual("fallback", service.Get("missing.string", "fallback"));
        }

        [Test]
        public void FetchAsync_CompletesSuccessfully()
        {
            FallbackConfigService service = new();

            Task task = service.FetchAsync();

            Assert.IsTrue(task.IsCompleted);
        }
    }
}
