using Playcenter.MobileCore;
using VContainer;

namespace KitchenClash.Composition
{
    /// <summary>Adapts the module's container-neutral builder to VContainer's IContainerBuilder.</summary>
    public sealed class VContainerSessionContainerBuilder : ISessionContainerBuilder
    {
        private readonly IContainerBuilder _builder;

        public VContainerSessionContainerBuilder(IContainerBuilder builder)
        {
            _builder = builder;
        }

        public IContainerBuilder Inner => _builder;

        public void AddSingleton<TService>(TService instance) where TService : class
        {
            _builder.RegisterInstance(instance).As<TService>();
        }

        public void AddSingleton<TService, TImpl>()
            where TService : class where TImpl : class, TService, new()
        {
            _builder.Register<TImpl>(Lifetime.Singleton).As<TService>();
        }
    }
}
