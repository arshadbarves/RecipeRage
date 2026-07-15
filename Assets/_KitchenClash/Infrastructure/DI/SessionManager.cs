using System;
using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using VContainer;
using VContainer.Unity;
using Playcenter.Shell;
using Playcenter.UI;

namespace KitchenClash.Infrastructure.DI
{
    public class SessionManager : ISessionLifecycle, IInitializable, IDisposable
    {
        private readonly IObjectResolver _container;
        private readonly IEventBus _eventBus;
        private readonly IUIService _uiService;

        private LifetimeScope _sessionScope;

        public IObjectResolver SessionContainer => _sessionScope?.Container;
        public bool IsSessionActive => _sessionScope != null;

        [Inject]
        public SessionManager(IObjectResolver container, IEventBus eventBus, IUIService uiService)
        {
            _container = container;
            _eventBus = eventBus;
            _uiService = uiService;
        }

        public void Initialize()
        {
        }

        public void CreateSession()
        {
            if (_sessionScope != null)
            {
                DestroySession();
            }

            LifetimeScope parentScope = _container.Resolve<LifetimeScope>();
            _sessionScope = parentScope.CreateChild<LifetimeScope>();
            _uiService.SetCurrentScope(_sessionScope.Container);
            GameLogger.LogInfo("SessionLifetimeScope created.");
        }

        public void DestroySession()
        {
            if (_sessionScope != null)
            {
                _sessionScope.Dispose();
                _sessionScope = null;
                _uiService.SetCurrentScope(null);
                GameLogger.LogInfo("SessionLifetimeScope destroyed.");
            }
        }

        public void Dispose()
        {
            DestroySession();
        }
    }
}
