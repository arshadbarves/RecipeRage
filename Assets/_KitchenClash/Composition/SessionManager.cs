using System;
using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Composition;
using KitchenClash.Domain;
using VContainer;
using VContainer.Unity;
using Playcenter.MobileCore;
using Playcenter.Shell;
using Playcenter.UI;

namespace KitchenClash.Infrastructure.DI
{
    /// <summary>
    /// Game-facing session lifecycle: delegates state transitions to the module's
    /// fail-closed SessionLifecycleController; keeps the UI-scope side effect and
    /// the ISessionLifecycle API surface unchanged for callers.
    /// </summary>
    public class SessionManager : ISessionLifecycle, IInitializable, IDisposable
    {
        private readonly IObjectResolver _container;
        private readonly IEventBus _eventBus;
        private readonly IUIService _uiService;
        private readonly SessionLifecycleController _controller;

        [Inject]
        public SessionManager(
            IObjectResolver container,
            IEventBus eventBus,
            IUIService uiService,
            KitchenClash.Application.ISessionScopeInstaller sessionScopeInstaller = null)
        {
            _container = container;
            _eventBus = eventBus;
            _uiService = uiService;

            LifetimeScope rootScope = container.Resolve<LifetimeScope>();
            _controller = new SessionLifecycleController(
                new VContainerSessionScopeFactory(rootScope),
                sessionScopeInstaller != null ? new MenuSessionScopeInstallerAdapter(sessionScopeInstaller) : null);
        }

        public IObjectResolver SessionContainer =>
            _controller.State == SessionState.Active && _controller.Scope is VContainerSessionScopeHandle handle
                ? handle.Get<IObjectResolver>()
                : null;

        public bool IsSessionActive => _controller.State == SessionState.Active;

        public void Initialize()
        {
        }

        public void CreateSession()
        {
            // Controller throws when the installer is missing (installer law) — the
            // same guard as before, now owned by the module FSM.
            _controller.CreateAsync().GetAwaiter().GetResult();

            if (_controller.Scope is VContainerSessionScopeHandle handle)
            {
                _uiService.SetCurrentScope(handle.Get<IObjectResolver>());
                GameLogger.LogInfo("SessionLifetimeScope created.");
            }
        }

        public void DestroySession()
        {
            if (_controller.State != SessionState.Active)
            {
                return;
            }

            _controller.TeardownAsync().GetAwaiter().GetResult();
            _uiService.SetCurrentScope(null);
            GameLogger.LogInfo("SessionLifetimeScope destroyed.");
        }

        public void Dispose()
        {
            DestroySession();
        }
    }
}
