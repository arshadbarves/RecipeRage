using System;
using Core.Events;
using Core.Logging;
using VContainer;
using VContainer.Unity;

namespace Core.Bootstrap
{
    public class SessionManager : IInitializable, IDisposable
    {
        private readonly IObjectResolver _container;
        private readonly IEventBus _eventBus;
        private SessionLifetimeScope _sessionScope;

        public IObjectResolver SessionContainer => _sessionScope?.Container;
        public bool IsSessionActive => _sessionScope != null;

        [Inject]
        public SessionManager(IObjectResolver container, IEventBus eventBus)
        {
            _container = container;
            _eventBus = eventBus;
        }

        public void Initialize()
        {
            _eventBus.Subscribe<LogoutEvent>(HandleLogout);
        }

        private void HandleLogout(LogoutEvent evt)
        {
            DestroySession();
        }

        public void CreateSession()
        {
            if (_sessionScope != null)
            {
                DestroySession();
            }

            var parentScope = _container.Resolve<LifetimeScope>();
            _sessionScope = parentScope.CreateChild<SessionLifetimeScope>();

            GameLogger.Log("[SessionManager] SessionLifetimeScope created.");
        }

        public void DestroySession()
        {
            if (_sessionScope != null)
            {
                _sessionScope.Dispose();
                _sessionScope = null;
                GameLogger.Log("[SessionManager] SessionLifetimeScope destroyed.");
            }
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<LogoutEvent>(HandleLogout);
            DestroySession();
        }
    }
}
