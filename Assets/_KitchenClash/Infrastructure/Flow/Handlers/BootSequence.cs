using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using Playcenter.GameFlow;
using KitchenClash.Application;

namespace KitchenClash.Infrastructure.Flow.Handlers
{
    /// <summary>
    /// Cold-boot pipeline: NTP → remote config → force update → maintenance → auth → session.
    /// Authenticated success ends with NotifyBootComplete only (never dual CompleteSidePhase).
    /// </summary>
    public sealed class BootSequence
    {
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
            INTPTimeService ntpTimeService,
            IRemoteConfigService remoteConfigService,
            IAuthService authService,
            IMaintenanceService maintenanceService,
            IEventBus eventBus,
            IAppFlow appFlow,
            SessionLoader sessionLoader)
        {
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
                // 1. NTP time sync (best-effort, timeout 5s)
                try
                {
                    using var ntpCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    ntpCts.CancelAfter(TimeSpan.FromSeconds(5.0f));
                    await _ntpTimeService.SyncTime().AttachExternalCancellation(ntpCts.Token).SuppressCancellationThrow();
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

                // 7. Session load then complete boot (authenticated cold boot only)
                GameLogger.Log("[BootSequence] Auth OK — loading session.");
                await _sessionLoader.LoadAsync(ct);
                ct.ThrowIfCancellationRequested();

                _appFlow?.NotifyBootComplete();
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
