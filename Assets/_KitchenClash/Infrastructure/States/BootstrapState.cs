using KitchenClash.Application;
using System;
using KitchenClash.Application.Services;
using KitchenClash.Application.State;
using Cysharp.Threading.Tasks;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Services;
using Playcenter.GameFlow;

namespace KitchenClash.Infrastructure.States
{
    public class BootstrapState : BaseState
    {
        private readonly IUIService _uiService;
        private readonly INTPTimeService _ntpTimeService;
        private readonly IRemoteConfigService _remoteConfigService;
        private readonly IAuthService _authService;
        private readonly Domain.IEncryptionService _encryptionService;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IGameStateManager _stateManager;
        private readonly Domain.IEventBus _eventBus;
        private readonly ForceUpdateChecker _forceUpdateChecker;
        private readonly IAppFlow _appFlow;

        public BootstrapState(
            IUIService uiService,
            INTPTimeService ntpTimeService,
            IRemoteConfigService remoteConfigService,
            IAuthService authService,
            Domain.IEncryptionService encryptionService,
            IMaintenanceService maintenanceService,
            IGameStateManager stateManager,
            Domain.IEventBus eventBus,
            IAppFlow appFlow = null)
        {
            _uiService = uiService;
            _ntpTimeService = ntpTimeService;
            _remoteConfigService = remoteConfigService;
            _authService = authService;
            _encryptionService = encryptionService;
            _maintenanceService = maintenanceService;
            _stateManager = stateManager;
            _eventBus = eventBus;
            _forceUpdateChecker = new ForceUpdateChecker(remoteConfigService, eventBus);
            _appFlow = appFlow;
        }

        public override void Enter()
        {
            base.Enter();
            EnterAsync().Forget();
        }

        private async UniTask EnterAsync()
        {
            GameLogger.Log("Entering game initialization setup");

            try
            {
                // Splash dwell removed — SplashFlowPort owns it now.
                await InitializeGameSequence();
            }
            catch (OperationCanceledException)
            {
                GameLogger.Log("[BootstrapState] Enter cancelled");
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
                if (_appFlow != null)
                {
                    _appFlow.EnterSidePhase(FlowPhaseId.Login);
                }
                else
                {
                    _stateManager.ChangeState<LoginState>();
                }
            }
        }

        private async UniTask InitializeGameSequence()
        {
            // 1. NTP time sync (best-effort, timeout 5s)
            try
            {
                using var cts = new System.Threading.CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(5.0f));
                await _ntpTimeService.SyncTime().AttachExternalCancellation(cts.Token).SuppressCancellationThrow();
            }
            catch { }
            if (!IsStateActive)
            {
                return;
            }

            // 2. Initialize remote config
            GameLogger.Log("[BootstrapState] Initializing remote config...");
            await _remoteConfigService.Initialize();
            if (!IsStateActive)
            {
                return;
            }

            // 3. Fetch latest config values
            await _remoteConfigService.RefreshConfig();
            if (!IsStateActive)
            {
                return;
            }

            // 4. Force update check
            bool isUpdateRequired = await _forceUpdateChecker.CheckForUpdateAsync();
            if (!IsStateActive)
            {
                return;
            }

            if (isUpdateRequired)
            {
                GameLogger.LogInfo("[BootstrapState] Force update required. Halting boot sequence.");
                // ForceUpdateChecker already published ForceUpdateEvent
                if (_appFlow != null)
                {
                    _appFlow.EnterSidePhase(FlowPhaseId.ForceUpdate);
                }
                return;
            }

            // 5. Maintenance check
            if (_maintenanceService != null)
            {
                bool isInMaintenance = await _maintenanceService.CheckMaintenanceStatusAsync();
                if (!IsStateActive)
                {
                    return;
                }

                if (isInMaintenance)
                {
                    GameLogger.LogInfo("[BootstrapState] Maintenance active. Transitioning to MaintenanceState.");
                    if (_appFlow != null)
                    {
                        _appFlow.EnterSidePhase(FlowPhaseId.Maintenance);
                    }
                    else
                    {
                        _stateManager.ChangeState<MaintenanceState>();
                    }
                    return;
                }
            }
            if (!IsStateActive)
            {
                return;
            }

            // 6. Auth check
            bool isAuthenticated = !string.IsNullOrEmpty(_authService.ProductUserId);

            if (!isAuthenticated)
            {
                if (_appFlow != null)
                {
                    _appFlow.EnterSidePhase(FlowPhaseId.Login);
                }
                else
                {
                    _stateManager.ChangeState<LoginState>();
                }
                return;
            }

            GameLogger.Log("Initialization complete. Transitioning to SessionLoadingState.");
            _stateManager.ChangeState<SessionLoadingState>();
        }
    }
}
