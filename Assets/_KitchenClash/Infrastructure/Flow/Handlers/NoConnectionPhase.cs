using System;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Flow;
using Playcenter.GameFlow;
using Playcenter.Shell;
using Playcenter.UI;

namespace KitchenClash.Infrastructure.Flow.Handlers
{
    /// <summary>
    /// No-connection side phase: show NoInternetPopup; Retry → re-runs full SDK boot via
    /// <see cref="IPlaycenterBootRetry"/>.
    /// </summary>
    public sealed class NoConnectionPhase
    {
        private const string NoInternetPopupTypeName =
            "KitchenClash.Presentation.Overlays.NoInternetPopup, KitchenClash.Presentation";

        private readonly IUIService _uiService;
        private readonly IEventBus _eventBus;
        private readonly IPlaycenterBootRetry _bootRetry;

        private Type _noInternetPopupType;
        private bool _active;

        public NoConnectionPhase(
            IUIService uiService,
            IEventBus eventBus,
            IAppFlow appFlow,
            IPlaycenterBootRetry bootRetry)
        {
            _uiService = uiService;
            _eventBus = eventBus;
            _bootRetry = bootRetry;
        }

        public void Enter()
        {
            Exit();
            _active = true;

            _eventBus?.Subscribe<RetryConnectionEvent>(OnRetry);

            _noInternetPopupType ??= Type.GetType(NoInternetPopupTypeName);
            if (_noInternetPopupType != null)
            {
                _uiService?.Show(_noInternetPopupType);
            }
            else
            {
                GameLogger.LogError("[NoConnectionPhase] NoInternetPopup type not found");
            }

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
            if (_noInternetPopupType != null)
            {
                _uiService?.Hide(_noInternetPopupType);
            }
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

