using VContainer;

namespace Core.State
{
    public class StateFactory : IStateFactory
    {
        private readonly IObjectResolver _container;

        public StateFactory(IObjectResolver container)
        {
            _container = container;
        }

        public T CreateState<T>() where T : IState
        {
            // Use VContainer to instantiate the state, resolving its constructor dependencies
            return _container.Resolve<T>();
        }
    }
}
