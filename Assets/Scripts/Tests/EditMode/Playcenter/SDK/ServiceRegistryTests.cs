using NUnit.Framework;
using Playcenter.SDK;

namespace RecipeRage.Tests.EditMode.Playcenter.SDK
{
    public sealed class ServiceRegistryTests
    {
        public interface IFoo { }
        public sealed class Foo : IFoo { }
        public sealed class Foo2 : IFoo { }

        [Test]
        public void Get_AfterAddSingletonInstance_ReturnsSameInstance()
        {
            var reg = new ServiceRegistry();
            var foo = new Foo();
            reg.AddSingleton<IFoo>(foo);
            IPlaycenterServices services = reg.Build();

            Assert.AreSame(foo, services.Get<IFoo>());
        }

        [Test]
        public void Get_AfterAddSingletonType_CreatesSingleInstance()
        {
            var reg = new ServiceRegistry();
            reg.AddSingleton<IFoo, Foo>();
            IPlaycenterServices services = reg.Build();

            Assert.AreSame(services.Get<IFoo>(), services.Get<IFoo>());
            Assert.IsInstanceOf<Foo>(services.Get<IFoo>());
        }

        [Test]
        public void Get_AfterFactory_ReceivesServices()
        {
            var reg = new ServiceRegistry();
            reg.AddSingleton<IFoo, Foo>();
            reg.AddSingleton<string>(sp => "id:" + sp.Get<IFoo>().GetType().Name);
            IPlaycenterServices services = reg.Build();

            Assert.AreEqual("id:Foo", services.Get<string>());
        }

        [Test]
        public void Get_WhenMissing_ThrowsInvalidOperationException()
        {
            IPlaycenterServices services = new ServiceRegistry().Build();
            Assert.Throws<System.InvalidOperationException>(() => services.Get<IFoo>());
        }

        [Test]
        public void TryGet_WhenMissing_ReturnsFalse()
        {
            IPlaycenterServices services = new ServiceRegistry().Build();
            bool ok = services.TryGet<IFoo>(out IFoo _);
            Assert.IsFalse(ok);
        }

        [Test]
        public void AddSingleton_AfterBuild_ThrowsInvalidOperationException()
        {
            var reg = new ServiceRegistry();
            reg.Build();
            Assert.Throws<System.InvalidOperationException>(() => reg.AddSingleton<IFoo, Foo>());
        }

        [Test]
        public void AddSingleton_DuplicateService_ThrowsInvalidOperationException()
        {
            var reg = new ServiceRegistry();
            reg.AddSingleton<IFoo, Foo>();
            Assert.Throws<System.InvalidOperationException>(() => reg.AddSingleton<IFoo, Foo2>());
        }
    }
}
