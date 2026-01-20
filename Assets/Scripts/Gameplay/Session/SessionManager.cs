using System;
using Gameplay.Bootstrap;
using Gameplay.App.State;
using Gameplay.App.State.States;
using Core.Logging;
using Core.UI.Interfaces;
using Core.Shared.Events;
using VContainer;
using VContainer.Unity;

namespace Core.Session
{
    public class SessionManager : IInitializable, IDisposable
    {
        private readonly IObjectResolver _container;
        private readonly IEventBus _eventBus;
        private readonly IUIService _uiService;
        private readonly IGameStateManager _stateManager;

        private SessionLifetimeScope _sessionScope;

        public IObjectResolver SessionContainer => _sessionScope?.Container;
        public bool IsSessionActive => _sessionScope != null;

        [Inject]
        public SessionManager(
            IObjectResolver container,
            IEventBus eventBus,
            IUIService uiService,
            IGameStateManager stateManager)
        {
            _container = container;
            _eventBus = eventBus;
            _uiService = uiService;
            _stateManager = stateManager;
        }

        public void Initialize()
        {
            _eventBus.Subscribe<LogoutEvent>(HandleLogout);
        }

        private void HandleLogout(LogoutEvent evt)
        {
            GameLogger.LogInfo("Orchestrating full logout flow...");

            // 1. Clear UI
            _uiService?.HideAllScreens(false);

            // 2. Change State back to Login
            _stateManager?.ChangeState<LoginState>();

            // 3. Destroy Session Scope
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

            // Update GameStateManager to use the session container so it can resolve session-scoped states (like GameplayState)
            if (_stateManager is GameStateManager concreteStateManager)
            {
                concreteStateManager.SetContainerResolver(_sessionScope.Container);
            }

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
            _eventBus.Unsubscribe<LogoutEvent>(HandleLogout);
            DestroySession();
        }
    }
}
