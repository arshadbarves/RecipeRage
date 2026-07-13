using System;
using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Application.State;
using KitchenClash.Domain;
using Playcenter.GameFlow;

namespace KitchenClash.Infrastructure.States
{
    /// <summary>
    /// Shown when the boot sequence cannot reach EOS / the network.
    /// Displays the NoInternetPopup and blocks until the player taps Retry,
    /// at which point it restarts the full SessionLoadingState boot sequence.
    ///
    /// This is the only state that shows a blocking modal — all other errors
    /// are silent fallbacks.
    /// </summary>
    public class NoConnectionState : BaseState
    {
        private const string NoInternetPopupTypeName = "KitchenClash.Presentation.Overlays.NoInternetPopup, KitchenClash.Presentation";

        private readonly IUIService        _uiService;
        private readonly IEventBus         _eventBus;
        private readonly IGameStateManager _stateManager;
        private readonly IAppFlow          _appFlow;

        private Type _noInternetPopupType;

        public NoConnectionState(
            IUIService        uiService,
            IEventBus         eventBus,
            IGameStateManager stateManager,
            IAppFlow          appFlow = null)
        {
            _uiService    = uiService;
            _eventBus     = eventBus;
            _stateManager = stateManager;
            _appFlow      = appFlow;
        }

        public override void Enter()
        {
            base.Enter();

            _eventBus?.Subscribe<RetryConnectionEvent>(OnRetry);

            _noInternetPopupType ??= Type.GetType(NoInternetPopupTypeName);
            if (_noInternetPopupType != null)
                _uiService.Show(_noInternetPopupType);
            else
                LogError("NoInternetPopup type not found");

            LogMessage("Waiting for player to retry connection");
        }

        public override void Exit()
        {
            _eventBus?.Unsubscribe<RetryConnectionEvent>(OnRetry);
            if (_noInternetPopupType != null) _uiService.Hide(_noInternetPopupType);
            base.Exit();
        }

        private void OnRetry(RetryConnectionEvent _)
        {
            if (!IsStateActive) return;
            LogMessage("Retry tapped → restarting SessionLoadingState");
            
            // If entered as side phase, notify completion
            _appFlow?.CompleteSidePhase();
            
            // Continue worker chain to reload session
            _stateManager.ChangeState<SessionLoadingState>();
        }
    }
}
