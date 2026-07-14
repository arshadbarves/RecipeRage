using System;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using Playcenter.GameFlow;

namespace KitchenClash.Infrastructure.Flow.Handlers
{
    /// <summary>
    /// Account upgrade side phase: show upgrade screen; any result → ReturnHome.
    /// </summary>
    public sealed class AccountUpgradePhase
    {
        private const string AccountUpgradeScreenTypeName =
            "KitchenClash.Presentation.Screens.AccountUpgradeScreen, KitchenClash.Presentation";

        private readonly IUIService _uiService;
        private readonly IEventBus _eventBus;
        private readonly IAppFlow _appFlow;

        private Type _accountUpgradeScreenType;
        private bool _active;

        public AccountUpgradePhase(IUIService uiService, IEventBus eventBus, IAppFlow appFlow)
        {
            _uiService = uiService;
            _eventBus = eventBus;
            _appFlow = appFlow;
        }

        public void Enter()
        {
            Exit();
            _active = true;

            _eventBus?.Subscribe<AccountUpgradeResultEvent>(OnUpgradeResult);

            _accountUpgradeScreenType ??= Type.GetType(AccountUpgradeScreenTypeName);
            if (_accountUpgradeScreenType != null)
            {
                _uiService?.Show(_accountUpgradeScreenType);
            }
            else
            {
                GameLogger.LogError("[AccountUpgradePhase] AccountUpgradeScreen type not found");
            }

            GameLogger.Log("[AccountUpgradePhase] AccountUpgradeScreen shown");
        }

        public void Exit()
        {
            if (!_active)
            {
                return;
            }

            _active = false;
            _eventBus?.Unsubscribe<AccountUpgradeResultEvent>(OnUpgradeResult);
            if (_accountUpgradeScreenType != null)
            {
                _uiService?.Hide(_accountUpgradeScreenType);
            }
        }

        private void OnUpgradeResult(AccountUpgradeResultEvent evt)
        {
            if (!_active)
            {
                return;
            }

            if (evt.Linked)
            {
                GameLogger.Log($"[AccountUpgradePhase] Account linked via {evt.Provider} → ReturnHome");
            }
            else
            {
                GameLogger.Log("[AccountUpgradePhase] Continuing as guest → ReturnHome");
            }

            _appFlow?.ReturnHome();
        }
    }
}
