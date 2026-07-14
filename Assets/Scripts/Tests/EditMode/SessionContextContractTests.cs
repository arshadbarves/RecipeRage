using System.Reflection;
using KitchenClash.Application;
using NUnit.Framework;

namespace RecipeRage.Tests.EditMode
{
    public class SessionContextContractTests
    {
        [Test]
        public void ISessionContext_LivesInApplication_AndExposesOnlyInterfaces()
        {
            var t = typeof(ISessionContext);
            Assert.AreEqual("KitchenClash.Application", t.Namespace);

            foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (p.Name == nameof(ISessionContext.IsSessionActive))
                {
                    continue;
                }

                Assert.True(
                    p.PropertyType.IsInterface,
                    $"{p.Name} must be an interface, was {p.PropertyType.Name}");
            }
        }
    }
}
