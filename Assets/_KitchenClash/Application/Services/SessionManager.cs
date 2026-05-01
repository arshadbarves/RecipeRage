using System;
using KitchenClash.Infrastructure.Logging;
using KitchenClash.Domain.Interfaces;
using KitchenClash.Application.State;
using KitchenClash.Application.State.States;
using VContainer;
using VContainer.Unity;

namespace KitchenClash.Application.Services
{
    public class SessionManager : IInitializable, IDisposable
    {
        private readonly IObjectResolver _container;
        private readonly IEventBus _eventBus;
        private readonly IGameStateManager _stateManager;

        private LifetimeScope _sessionScope;

        public IObjectResolver SessionContainer => _sessionScope?.Container;
        public bool IsSessionActive => _sessionScope != null;

        [Inject]
        public SessionManager(IObjectResolver container, IEventBus eventBus, IGameStateManager stateManager)
        {
            _container = container;
            _eventBus = eventBus;
            _stateManager = stateManager;
        }

        public void Initialize()
        {
        }

        public void CreateSession()
        {
            if (_sessionScope != null) DestroySession();
            var parentScope = _container.Resolve<LifetimeScope>();
            _sessionScope = parentScope.CreateChild<LifetimeScope>();
            GameLogger.LogInfo("SessionLifetimeScope created.");
        }

        public void DestroySession()
        {
            if (_sessionScope != null)
            {
                _sessionScope.Dispose();
                _sessionScope = null;
                GameLogger.LogInfo("SessionLifetimeScope destroyed.");
            }
        }

        public void Dispose()
        {
            DestroySession();
        }
    }
}
