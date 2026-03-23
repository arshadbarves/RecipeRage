using System;
using VContainer;

namespace Gameplay.App.State
{
    /// <summary>
    /// VContainer-backed factory for application states.
    /// </summary>
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
