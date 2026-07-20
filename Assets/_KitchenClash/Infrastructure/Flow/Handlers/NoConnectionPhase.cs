using System;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Flow;
using Playcenter.GameFlow;
using Playcenter.SDK;
using Playcenter.Shell;

namespace KitchenClash.Infrastructure.Flow.Handlers
{
    /// <summary>
    /// No-connection side phase: show SDK NoConnection shell; Retry → re-runs full SDK boot via
    /// <see cref="IPlaycenterBootRetry"/>.
    /// </summary>
    public sealed class NoConnectionPhase
    {
        private readonly IEventBus _eventBus;
        private readonly IPlaycenterBootRetry _bootRetry;
        private readonly IShellUi _shellUi;

        private bool _active;

        public NoConnectionPhase(
            IEventBus eventBus,
            IAppFlow appFlow,
            IPlaycenterBootRetry bootRetry,
            IShellUi shellUi)
        {
            _eventBus = eventBus;
            _bootRetry = bootRetry;
            _shellUi = shellUi;
        }

        public void Enter()
        {
            Exit();
            _active = true;

            _eventBus?.Subscribe<RetryConnectionEvent>(OnRetry);
            _shellUi?.Show(ShellScreenId.NoConnection);

            GameLogger.Log("[NoConnectionPhase] Waiting for player to retry connection");
        }

        public void Exit()
        {
            if (!_active)
            {
                return;
            }

            _active = false;
            _eventBus?.Unsubscribe<RetryConnectionEvent>(OnRetry);
            _shellUi?.HideAll();
        }

        private void OnRetry(RetryConnectionEvent _)
        {
            if (!_active)
            {
                return;
            }

            GameLogger.Log("[NoConnectionPhase] Retry tapped → re-running SDK boot");

            // Exit first to avoid re-entrancy if boot immediately re-enters NoConnection.
            Exit();
            _bootRetry?.Retry();
        }
    }
}

