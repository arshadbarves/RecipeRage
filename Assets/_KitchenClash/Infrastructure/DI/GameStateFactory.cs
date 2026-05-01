using System;
using KitchenClash.Application.State;
using VContainer;

namespace KitchenClash.Infrastructure.DI
{
    public class GameStateFactory : IStateFactory
    {
        private readonly IObjectResolver _container;

        public GameStateFactory(IObjectResolver container)
        {
            _container = container;
        }

        public T Create<T>() where T : IState
        {
            return _container.Resolve<T>();
        }

        public IState Create(Type stateType)
        {
            return (IState)_container.Resolve(stateType);
        }
    }
}
