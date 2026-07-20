using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using Playcenter.GameFlow;
using Playcenter.SDK;
using Playcenter.Services;
using Playcenter.Shell;

namespace KitchenClash.Infrastructure.Flow.Handlers
{
    /// <summary>
    /// Maintenance side phase: show SDK Maintenance shell and poll until clear → Login.
    /// </summary>
    public sealed class MaintenancePhase
    {
        private const float RetryIntervalSeconds = 30f;

        private readonly IMaintenanceService _maintenanceService;
        private readonly IRemoteConfigService _remoteConfigService;
        private readonly IAppFlow _appFlow;
        private readonly IShellUi _shellUi;

        private CancellationTokenSource _cts;
        private bool _active;

        public MaintenancePhase(
            IMaintenanceService maintenanceService,
            IRemoteConfigService remoteConfigService,
            IAppFlow appFlow,
            IShellUi shellUi)
        {
            _maintenanceService = maintenanceService;
            _remoteConfigService = remoteConfigService;
            _appFlow = appFlow;
            _shellUi = shellUi;
        }

        public void Enter()
        {
            Exit();
            _active = true;
            _cts = new CancellationTokenSource();
            GameLogger.LogInfo("[MaintenancePhase] Entered maintenance phase");
            _shellUi?.Show(ShellScreenId.Maintenance);
            RunAsync(_cts.Token).Forget();
        }

        public void Exit()
        {
            _active = false;
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }

            _shellUi?.HideAll();
            GameLogger.LogInfo("[MaintenancePhase] Exited maintenance phase");
        }

        private async UniTaskVoid RunAsync(CancellationToken ct)
        {
            try
            {
                while (_active && !ct.IsCancellationRequested)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(RetryIntervalSeconds), cancellationToken: ct);
                    if (!_active || ct.IsCancellationRequested)
                    {
                        return;
                    }

                    if (_remoteConfigService != null)
                    {
                        await _remoteConfigService.RefreshConfig();
                    }

                    bool stillInMaintenance = _maintenanceService != null
                        && await _maintenanceService.CheckMaintenanceStatusAsync();

                    if (!stillInMaintenance)
                    {
                        GameLogger.LogInfo("[MaintenancePhase] Maintenance ended → Login side phase");
                        _appFlow?.EnterSidePhase(FlowPhaseId.Login);
                        return;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
            }
        }
    }
}
