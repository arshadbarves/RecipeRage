using KitchenClash.Application;
using System;
using KitchenClash.Application.Services;
using KitchenClash.Application.State;
using Cysharp.Threading.Tasks;
using KitchenClash.Infrastructure.Logging;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.EOS;
using KitchenClash.Infrastructure.Services;

namespace KitchenClash.Infrastructure.States
{
    public class BootstrapState : BaseState
    {
        private const float SplashDuration = 3.5f;
        private readonly IUIService _uiService;
        private readonly INTPTimeService _ntpTimeService;
        private readonly IRemoteConfigService _remoteConfigService;
        private readonly IAuthService _authService;
        private readonly Domain.IEncryptionService _encryptionService;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IGameStateManager _stateManager;
        private readonly Domain.IEventBus _eventBus;

        public BootstrapState(
            IUIService uiService,
            INTPTimeService ntpTimeService,
            IRemoteConfigService remoteConfigService,
            IAuthService authService,
            Domain.IEncryptionService encryptionService,
            IMaintenanceService maintenanceService,
            IGameStateManager stateManager,
            Domain.IEventBus eventBus)
        {
            _uiService = uiService;
            _ntpTimeService = ntpTimeService;
            _remoteConfigService = remoteConfigService;
            _authService = authService;
            _encryptionService = encryptionService;
            _maintenanceService = maintenanceService;
            _stateManager = stateManager;
            _eventBus = eventBus;
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
                await UniTask.Delay(TimeSpan.FromSeconds(SplashDuration), cancellationToken: StateCancellationToken);
                if (!IsStateActive) return;

                await InitializeGameSequence();
            }
            catch (OperationCanceledException)
            {
                GameLogger.Log("[BootstrapState] Enter cancelled");
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
                _stateManager.ChangeState<LoginState>();
            }
        }

        private async UniTask InitializeGameSequence()
        {
            try
            {
                using var cts = new System.Threading.CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(5.0f));
                await _ntpTimeService.SyncTime().AttachExternalCancellation(cts.Token).SuppressCancellationThrow();
            }
            catch { }
            if (!IsStateActive) return;

            await _remoteConfigService.Initialize();
            if (!IsStateActive) return;

            await _remoteConfigService.RefreshConfig();
            if (!IsStateActive) return;

            var forceUpdateChecker = new ForceUpdateChecker(_remoteConfigService, _eventBus);
            bool isUpdateRequired = await forceUpdateChecker.CheckForUpdateAsync();
            if (!IsStateActive) return;

            if (isUpdateRequired)
            {
                GameLogger.LogInfo("[Bootstrap] Force update required. Halting boot sequence.");
                return;
            }

            if (_maintenanceService != null)
            {
                await _maintenanceService.CheckMaintenanceStatusAsync();
            }
            if (!IsStateActive) return;

            bool isAuthenticated = _authService.IsSignedIn;

            if (!isAuthenticated)
            {
                _stateManager.ChangeState<LoginState>();
                return;
            }

            GameLogger.Log("Initialization complete. Transitioning to SessionLoadingState.");
            _stateManager.ChangeState<SessionLoadingState>();
        }
    }
}
