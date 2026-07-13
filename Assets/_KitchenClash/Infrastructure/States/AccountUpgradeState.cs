using System;
using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Application.State;
using KitchenClash.Domain;
using Playcenter.GameFlow;

namespace KitchenClash.Infrastructure.States
{
    /// <summary>
    /// Shown exactly once after the tutorial completes (first install only).
    ///
    /// Presents the AccountUpgradeScreen where the player can:
    ///   A) Link a social account (Google / Facebook / Apple)
    ///   B) Continue as Guest
    ///
    /// On either choice:
    ///   • PlayerProgressData.HasSeenAccountUpgradePrompt is set to true (persisted)
    ///   • Transitions to MainMenuState
    ///
    /// The screen is NEVER shown as a popup again — it lives in Settings from this point.
    ///
    /// Entry condition (checked in SessionLoadingState.Route):
    ///   tutorial.IsComplete == true  AND  !progress.HasSeenAccountUpgradePrompt
    /// </summary>
    public class AccountUpgradeState : BaseState
    {
        private const string AccountUpgradeScreenTypeName = "KitchenClash.Presentation.Screens.AccountUpgradeScreen, KitchenClash.Presentation";

        private readonly IUIService        _uiService;
        private readonly IEventBus         _eventBus;
        private readonly IGameStateManager _stateManager;
        private readonly IAppFlow          _appFlow;

        private Type _accountUpgradeScreenType;

        public AccountUpgradeState(
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

            _eventBus?.Subscribe<AccountUpgradeResultEvent>(OnUpgradeResult);

            // Overlay the upgrade screen on top of whatever was last visible
            _accountUpgradeScreenType ??= Type.GetType(AccountUpgradeScreenTypeName);
            if (_accountUpgradeScreenType != null)
                _uiService.Show(_accountUpgradeScreenType);
            else
                LogError("AccountUpgradeScreen type not found");

            LogMessage("AccountUpgradeScreen shown");
        }

        public override void Exit()
        {
            _eventBus?.Unsubscribe<AccountUpgradeResultEvent>(OnUpgradeResult);
            if (_accountUpgradeScreenType != null) _uiService.Hide(_accountUpgradeScreenType);
            base.Exit();
        }

        // ── Event handler ─────────────────────────────────────────────────

        private void OnUpgradeResult(AccountUpgradeResultEvent evt)
        {
            if (!IsStateActive) return;

            if (evt.Linked)
                LogMessage($"Account linked via {evt.Provider} → ReturnHome");
            else
                LogMessage("Continuing as guest → ReturnHome");

            // Either path goes to the main lobby
            if (_appFlow != null)
            {
                _appFlow.ReturnHome();
            }
            else
            {
                // Fallback for tests without IAppFlow
                _stateManager.ChangeState<MainMenuState>();
            }
        }
    }
}
