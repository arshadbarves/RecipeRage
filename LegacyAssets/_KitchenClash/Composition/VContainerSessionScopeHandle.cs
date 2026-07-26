using Playcenter.MobileCore;
using VContainer;
using VContainer.Unity;

namespace KitchenClash.Composition
{
    public sealed class VContainerSessionScopeHandle : ISessionScopeHandle
    {
        private readonly LifetimeScope _scope;

        public VContainerSessionScopeHandle(LifetimeScope scope)
        {
            _scope = scope;
        }

        public T Get<T>() where T : class
        {
            return _scope.Container.Resolve<T>();
        }

        public bool TryGet<T>(out T service) where T : class
        {
            return _scope.Container.TryResolve(out service);
        }

        public void Dispose()
        {
            if (_scope != null)
            {
                _scope.Dispose();
            }
        }
    }
}
