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
        private readonly ISessionScopeInstaller _sessionScopeInstaller;

        private LifetimeScope _sessionScope;

        public IObjectResolver SessionContainer => _sessionScope?.Container;
        public bool IsSessionActive => _sessionScope != null;

        [Inject]
        public SessionManager(
            IObjectResolver container,
            IEventBus eventBus,
            IUIService uiService,
            ISessionScopeInstaller sessionScopeInstaller = null)
        {
            _container = container;
            _eventBus = eventBus;
            _uiService = uiService;
            _sessionScopeInstaller = sessionScopeInstaller;
        }

        public void Initialize()
        {
        }

        public void CreateSession()
        {
            if (_sessionScopeInstaller == null)
            {
                throw new InvalidOperationException(
                    "ISessionScopeInstaller is required. Register MenuSessionScopeInstaller at root.");
            }

            if (_sessionScope != null)
            {
                DestroySession();
            }

            // Install menu/session registrations (IEconomyService, IWallet, …). A bare child
            // LifetimeScope has no Configure() and cannot resolve session-scoped services.
            LifetimeScope parentScope = _container.Resolve<LifetimeScope>();
            _sessionScope = parentScope.CreateChild(builder => _sessionScopeInstaller.Install(builder));

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
