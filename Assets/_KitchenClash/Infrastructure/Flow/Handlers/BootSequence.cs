using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using Playcenter.GameFlow;
using KitchenClash.Application;
using Playcenter.Shell;
using Playcenter.Services;

namespace KitchenClash.Infrastructure.Flow.Handlers
{
    /// <summary>
    /// Cold-boot pipeline: NTP → remote config → force update → maintenance → auth → session.
    /// Authenticated success calls NotifyBootComplete when arriving from Boot phase, or
    /// CompleteSidePhase when retrying from a side phase (e.g. NoConnection).
    /// </summary>
    public sealed class BootSequence
    {
        private readonly IConnectivityService _connectivity;
        private readonly INTPTimeService _ntpTimeService;
        private readonly IRemoteConfigService _remoteConfigService;
        private readonly IAuthService _authService;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IEventBus _eventBus;
        private readonly IAppFlow _appFlow;
        private readonly SessionLoader _sessionLoader;
        private readonly ForceUpdateChecker _forceUpdateChecker;

        private CancellationTokenSource _cts;

        public BootSequence(
            IConnectivityService connectivity,
            INTPTimeService ntpTimeService,
            IRemoteConfigService remoteConfigService,
            IAuthService authService,
            IMaintenanceService maintenanceService,
            IEventBus eventBus,
            IAppFlow appFlow,
            SessionLoader sessionLoader)
        {
            _connectivity = connectivity;
            _ntpTimeService = ntpTimeService;
            _remoteConfigService = remoteConfigService;
            _authService = authService;
            _maintenanceService = maintenanceService;
            _eventBus = eventBus;
            _appFlow = appFlow;
            _sessionLoader = sessionLoader;
            _forceUpdateChecker = new ForceUpdateChecker(remoteConfigService, eventBus);
        }

        public void Start()
        {
            Cancel();
            _cts = new CancellationTokenSource();
            RunAsync(_cts.Token).Forget();
        }

        public void Cancel()
        {
            if (_cts == null)
            {
                return;
            }

            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        public async UniTask RunAsync(CancellationToken ct)
        {
            GameLogger.Log("[BootSequence] Entering game initialization setup");

            try
            {
                // 0. Connectivity gate — must be online before any network service call.
                if (_connectivity == null || !_connectivity.IsOnline)
                {
                    GameLogger.LogInfo("[BootSequence] Offline — entering NoConnection.");
                    _appFlow?.EnterSidePhase(FlowPhaseId.NoConnection);
                    return;
                }

                // 1. NTP time sync (best-effort, timeout 5s)
                try
                {
                    using var ntpCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    ntpCts.CancelAfter(TimeSpan.FromSeconds(5.0f));
                    Task<bool> syncTask = _ntpTimeService.SyncTime();
                    Task completed = await Task.WhenAny(syncTask, Task.Delay(Timeout.Infinite, ntpCts.Token));
                    if (completed == syncTask)
                    {
                        await syncTask;
                    }
                }
                catch (OperationCanceledException)
                {
                    // best-effort timeout
                }
                catch
                {
                    // best-effort
                }

                ct.ThrowIfCancellationRequested();

                // 2. Initialize remote config
                GameLogger.Log("[BootSequence] Initializing remote config...");
                await _remoteConfigService.Initialize();
                ct.ThrowIfCancellationRequested();

                // 3. Fetch latest config values
                await _remoteConfigService.RefreshConfig();
                ct.ThrowIfCancellationRequested();

                // 4. Force update check
                bool isUpdateRequired = await _forceUpdateChecker.CheckForUpdateAsync();
                ct.ThrowIfCancellationRequested();

                if (isUpdateRequired)
                {
                    GameLogger.LogInfo("[BootSequence] Force update required. Halting boot sequence.");
                    _appFlow?.EnterSidePhase(FlowPhaseId.ForceUpdate);
                    return;
                }

                // 5. Maintenance check
                if (_maintenanceService != null)
                {
                    bool isInMaintenance = await _maintenanceService.CheckMaintenanceStatusAsync();
                    ct.ThrowIfCancellationRequested();

                    if (isInMaintenance)
                    {
                        GameLogger.LogInfo("[BootSequence] Maintenance active.");
                        _appFlow?.EnterSidePhase(FlowPhaseId.Maintenance);
                        return;
                    }
                }

                ct.ThrowIfCancellationRequested();

                // 6. Auth check
                bool isAuthenticated = !string.IsNullOrEmpty(_authService.ProductUserId);
                if (!isAuthenticated)
                {
                    _appFlow?.EnterSidePhase(FlowPhaseId.Login);
                    return;
                }

                // 7. Session load then complete boot.
                // From Boot phase → NotifyBootComplete transitions to Home.
                // From a side-phase retry (e.g. NoConnection) → CompleteSidePhase; the
                // Boot return-target mapping in AppFlowController will route to Home.
                GameLogger.Log("[BootSequence] Auth OK — loading session.");
                await _sessionLoader.LoadAsync(ct);
                ct.ThrowIfCancellationRequested();

                if (_appFlow != null)
                {
                    if (_appFlow.Current == FlowPhaseId.Boot)
                        _appFlow.NotifyBootComplete();
                    else
                        _appFlow.CompleteSidePhase();
                }
            }
            catch (OperationCanceledException)
            {
                GameLogger.Log("[BootSequence] Cancelled");
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
                _appFlow?.EnterSidePhase(FlowPhaseId.Login);
            }
        }
    }
}
