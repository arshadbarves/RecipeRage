using KitchenClash.Application;
using System;
using KitchenClash.Application.Models.RemoteConfigs;
using KitchenClash.Application.Services;
using KitchenClash.Application.State;
using Cysharp.Threading.Tasks;
using KitchenClash.Domain;
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
        private readonly ForceUpdateChecker _forceUpdateChecker;
        private readonly ChefRegistry _chefRegistry;
        private readonly MapRegistry _mapRegistry;

        public BootstrapState(
            IUIService uiService,
            INTPTimeService ntpTimeService,
            IRemoteConfigService remoteConfigService,
            IAuthService authService,
            Domain.IEncryptionService encryptionService,
            IMaintenanceService maintenanceService,
            IGameStateManager stateManager,
            Domain.IEventBus eventBus,
            ChefRegistry chefRegistry,
            MapRegistry mapRegistry)
        {
            _uiService = uiService;
            _ntpTimeService = ntpTimeService;
            _remoteConfigService = remoteConfigService;
            _authService = authService;
            _encryptionService = encryptionService;
            _maintenanceService = maintenanceService;
            _stateManager = stateManager;
            _eventBus = eventBus;
            _chefRegistry = chefRegistry;
            _mapRegistry = mapRegistry;
            _forceUpdateChecker = new ForceUpdateChecker(remoteConfigService, eventBus);
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
                if (!IsStateActive)
                {
                    return;
                }

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
            ApplyRegistryOverrides();
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
                    _stateManager.ChangeState<MaintenanceState>();
                    return;
                }
            }
            if (!IsStateActive)
            {
                return;
            }

            // 6. Auth check
            bool isAuthenticated = _authService.IsSignedIn;

            if (!isAuthenticated)
            {
                _stateManager.ChangeState<LoginState>();
                return;
            }

            GameLogger.Log("Initialization complete. Transitioning to SessionLoadingState.");
            _stateManager.ChangeState<SessionLoadingState>();
        }

        private void ApplyRegistryOverrides()
        {
            try
            {
                CharacterConfig characterConfig = _remoteConfigService.GetConfig<CharacterConfig>();
                if (characterConfig != null)
                {
                    _chefRegistry.ApplyRemoteConfig(characterConfig);
                    GameLogger.Log("[BootstrapState] Applied character config overrides");
                }

                MapConfig mapConfig = _remoteConfigService.GetConfig<MapConfig>();
                if (mapConfig != null)
                {
                    _mapRegistry.ApplyRemoteConfig(mapConfig);
                    GameLogger.Log("[BootstrapState] Applied map config overrides");
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"[BootstrapState] Failed to apply registry overrides: {ex.Message}");
            }
        }
    }
}
