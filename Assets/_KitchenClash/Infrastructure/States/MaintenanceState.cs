using KitchenClash.Application.State;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using Cysharp.Threading.Tasks;
using System;
using Playcenter.GameFlow;

namespace KitchenClash.Infrastructure.States
{
    /// <summary>
    /// State shown when the game is in maintenance mode.
    /// Displays MaintenanceScreen and periodically re-checks status.
    /// </summary>
    public class MaintenanceState : BaseState
    {
        private const float RetryIntervalSeconds = 30f;

        private readonly IUIService _uiService;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IRemoteConfigService _remoteConfigService;
        private readonly IGameStateManager _stateManager;
        private readonly IEventBus _eventBus;
        private readonly IAppFlow _appFlow;

        public MaintenanceState(
            IUIService uiService,
            IMaintenanceService maintenanceService,
            IRemoteConfigService remoteConfigService,
            IGameStateManager stateManager,
            IEventBus eventBus,
            IAppFlow appFlow = null)
        {
            _uiService = uiService;
            _maintenanceService = maintenanceService;
            _remoteConfigService = remoteConfigService;
            _stateManager = stateManager;
            _eventBus = eventBus;
            _appFlow = appFlow;
        }

        public override void Enter()
        {
            base.Enter();
            GameLogger.LogInfo("[MaintenanceState] Entered maintenance state");
            ShowMaintenanceScreenAsync().Forget();
        }

        public override void Exit()
        {
            base.Exit();
            GameLogger.LogInfo("[MaintenanceState] Exited maintenance state");
        }

        private async UniTaskVoid ShowMaintenanceScreenAsync()
        {
            try
            {
                // Publish maintenance event so MaintenanceScreen shows
                _eventBus.Publish(new MaintenanceModeEvent
                {
                    IsMaintenanceMode = true,
                    Message = _maintenanceService.MaintenanceMessage,
                    EstimatedEndTime = _maintenanceService.EstimatedEndTime?.ToString("o") ?? "",
                    AllowRetry = true
                });

                // Periodically re-check maintenance status
                while (IsStateActive)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(RetryIntervalSeconds), cancellationToken: StateCancellationToken);
                    if (!IsStateActive)
                    {
                        return;
                    }

                    // Refresh config and re-check
                    await _remoteConfigService.RefreshConfig();
                    bool stillInMaintenance = await _maintenanceService.CheckMaintenanceStatusAsync();

                    if (!stillInMaintenance)
                    {
                        GameLogger.LogInfo("[MaintenanceState] Maintenance ended, transitioning to LoginState");
                        
                        // If entered as side phase, notify completion
                        _appFlow?.CompleteSidePhase();
                        
                        // Continue worker chain to login
                        _stateManager.ChangeState<LoginState>();
                        return;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // State exited
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
            }
        }
    }
}
