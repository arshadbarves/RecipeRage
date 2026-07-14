using KitchenClash.Infrastructure.Flow.Handlers;
using Playcenter.GameFlow;

namespace KitchenClash.Infrastructure.Flow
{
    /// <summary>
    /// Maps FlowPhaseId side phases to concrete handlers. ForceUpdate is UI-event driven (no-op enter).
    /// </summary>
    public sealed class SidePhaseFlowPort : ISidePhasePort
    {
        private readonly LoginPhase _login;
        private readonly MaintenancePhase _maintenance;
        private readonly NoConnectionPhase _noConnection;
        private readonly TutorialPhase _tutorial;
        private readonly AccountUpgradePhase _accountUpgrade;

        private FlowPhaseId _active = FlowPhaseId.None;

        public SidePhaseFlowPort(
            LoginPhase login,
            MaintenancePhase maintenance,
            NoConnectionPhase noConnection,
            TutorialPhase tutorial,
            AccountUpgradePhase accountUpgrade)
        {
            _login = login;
            _maintenance = maintenance;
            _noConnection = noConnection;
            _tutorial = tutorial;
            _accountUpgrade = accountUpgrade;
        }

        public void EnterSidePhase(FlowPhaseId phase, FlowContext context)
        {
            _ = context;
            if (_active == phase)
            {
                return;
            }

            ExitSidePhase(_active);
            _active = phase;

            switch (phase)
            {
                case FlowPhaseId.Login:
                    _login?.Enter();
                    break;
                case FlowPhaseId.Maintenance:
                    _maintenance?.Enter();
                    break;
                case FlowPhaseId.NoConnection:
                    _noConnection?.Enter();
                    break;
                case FlowPhaseId.Tutorial:
                    _tutorial?.Enter();
                    break;
                case FlowPhaseId.AccountUpgrade:
                    _accountUpgrade?.Enter();
                    break;
                case FlowPhaseId.ForceUpdate:
                    // Force-update UI is driven by ForceUpdateChecker events; no dedicated handler.
                    break;
            }
        }

        public void ExitSidePhase(FlowPhaseId phase)
        {
            if (phase == FlowPhaseId.None)
            {
                return;
            }

            switch (phase)
            {
                case FlowPhaseId.Login:
                    _login?.Exit();
                    break;
                case FlowPhaseId.Maintenance:
                    _maintenance?.Exit();
                    break;
                case FlowPhaseId.NoConnection:
                    _noConnection?.Exit();
                    break;
                case FlowPhaseId.Tutorial:
                    _tutorial?.Exit();
                    break;
                case FlowPhaseId.AccountUpgrade:
                    _accountUpgrade?.Exit();
                    break;
            }

            if (_active == phase)
            {
                _active = FlowPhaseId.None;
            }
        }
    }
}
