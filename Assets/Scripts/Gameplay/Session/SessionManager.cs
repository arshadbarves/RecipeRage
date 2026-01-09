using System;
using Gameplay.Bootstrap;
using Modules.Shared.Events;
using Modules.Logging;
using VContainer;
using VContainer.Unity;

namespace Modules.Session
{
    public class SessionManager : IInitializable, IDisposable
    {
        private readonly IObjectResolver _container;
        private readonly IEventBus _eventBus;
        private readonly ILoggingService _logger;
        private SessionLifetimeScope _sessionScope;

        public IObjectResolver SessionContainer => _sessionScope?.Container;
        public bool IsSessionActive => _sessionScope != null;

        [Inject]
        public SessionManager(IObjectResolver container, IEventBus eventBus, ILoggingService logger)
        {
            _container = container;
            _eventBus = eventBus;
            _logger = logger;
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

            _logger.LogInfo("SessionLifetimeScope created.", "SessionManager");
        }

        public void DestroySession()
        {
            if (_sessionScope != null)
            {
                _sessionScope.Dispose();
                _sessionScope = null;
                _logger.LogInfo("SessionLifetimeScope destroyed.", "SessionManager");
            }
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<LogoutEvent>(HandleLogout);
            DestroySession();
        }
    }
}
