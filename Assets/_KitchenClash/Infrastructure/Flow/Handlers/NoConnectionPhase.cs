using System;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using Playcenter.GameFlow;
using Playcenter.Shell;
using Playcenter.UI;

namespace KitchenClash.Infrastructure.Flow.Handlers
{
    /// <summary>
    /// No-connection side phase: show NoInternetPopup; Retry → re-runs full BootSequence.
    /// </summary>
    public sealed class NoConnectionPhase
    {
        private const string NoInternetPopupTypeName =
            "KitchenClash.Presentation.Overlays.NoInternetPopup, KitchenClash.Presentation";

        private readonly IUIService _uiService;
        private readonly IEventBus _eventBus;
        private readonly BootSequence _bootSequence;

        private Type _noInternetPopupType;
        private bool _active;

        public NoConnectionPhase(
            IUIService uiService,
            IEventBus eventBus,
            IAppFlow appFlow,
            BootSequence bootSequence)
        {
            _uiService = uiService;
            _eventBus = eventBus;
            _bootSequence = bootSequence;
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

            GameLogger.Log("[NoConnectionPhase] Retry tapped → re-running boot sequence");

            // Exit first to avoid re-entrancy if boot immediately re-enters NoConnection.
            Exit();
            _bootSequence?.Start();
        }
    }
}
