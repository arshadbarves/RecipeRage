using System;
using Core.Bootstrap;
using Core.Events;
using Core.Logging;
using Core.SaveSystem;
using VContainer;
using VContainer.Unity;

namespace Core.Bootstrap
{
    public class SessionManager : IInitializable, IDisposable
    {
        private readonly IObjectResolver _container;
        private readonly IEventBus _eventBus;
        private SessionLifetimeScope _sessionScope;

        public GameSession CurrentSession => _sessionScope?.Container?.Resolve<GameSession>();
        
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
            _sessionScope = parentScope.CreateChild<SessionLifetimeScope>(builder =>
            {
                // Register GameSession itself into its own scope so we can resolve it
                builder.Register<GameSession>(Lifetime.Singleton);
            });

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
