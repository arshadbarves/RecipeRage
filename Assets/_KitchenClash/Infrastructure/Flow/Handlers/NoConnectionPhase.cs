using System;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using Playcenter.GameFlow;
using Playcenter.Shell;
using Playcenter.UI;

namespace KitchenClash.Infrastructure.Flow.Handlers
{
    /// <summary>
    /// No-connection side phase: show NoInternetPopup; Retry → SessionLoader → CompleteSidePhase.
    /// </summary>
    public sealed class NoConnectionPhase
    {
        private const string NoInternetPopupTypeName =
            "KitchenClash.Presentation.Overlays.NoInternetPopup, KitchenClash.Presentation";

        private readonly IUIService _uiService;
        private readonly IEventBus _eventBus;
        private readonly IAppFlow _appFlow;
        private readonly SessionLoader _sessionLoader;

        private Type _noInternetPopupType;
        private bool _active;

        public NoConnectionPhase(
            IUIService uiService,
            IEventBus eventBus,
            IAppFlow appFlow,
            SessionLoader sessionLoader)
        {
            _uiService = uiService;
            _eventBus = eventBus;
            _appFlow = appFlow;
            _sessionLoader = sessionLoader;
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

        private async void OnRetry(RetryConnectionEvent _)
        {
            if (!_active)
            {
                return;
            }

            GameLogger.Log("[NoConnectionPhase] Retry tapped → reloading session");

            try
            {
                await _sessionLoader.LoadAsync();
                if (!_active)
                {
                    return;
                }

                _appFlow?.CompleteSidePhase();
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
            }
        }
    }
}
