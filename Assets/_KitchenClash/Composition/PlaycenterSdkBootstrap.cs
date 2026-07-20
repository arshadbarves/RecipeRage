using System.Threading;
using Cysharp.Threading.Tasks;
using KitchenClash.Application;
using KitchenClash.Application.Models.RemoteConfigs;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Boot;
using KitchenClash.Infrastructure.Flow;
using Playcenter.GameFlow;
using Playcenter.SDK;
using Playcenter.SDK.Unity;
using Playcenter.Services;
using Playcenter.Shell;
using UnityEngine;
using VContainer.Unity;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KitchenClash.Composition
{
    /// <summary>
    /// VContainer IStartable that launches the Playcenter SDK boot pipeline instead of the
    /// legacy GameBootstrapper / BootSequence cold-boot path.
    /// <para>
    /// Also implements <see cref="IPlaycenterBootRetry"/> so NoConnectionPhase can trigger a full
    /// SDK retry without depending on this concrete type directly.
    /// </para>
    /// <para>
    /// Service wiring: game-side VContainer singletons are passed as live references into the
    /// SDK service registry so the same instances are shared post-ready — no second construction
    /// of EOS auth, RC, or connectivity services.
    /// </para>
    /// </summary>
    public sealed class PlaycenterSdkBootstrap : IStartable, IPlaycenterBootRetry
    {
        private readonly IAuthService _authService;
        private readonly ISessionLifecycle _sessionLifecycle;
        private readonly ISessionContext _sessionContext;
        private readonly IAppFlow _appFlow;
        private readonly IConnectivityService _connectivity;
        private readonly INTPTimeService _ntpTimeService;
        private readonly IRemoteConfigService _remoteConfigService;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IAnalyticsService _analytics;
        private readonly IEventBus _eventBus;
        private readonly BootRetryRef _bootRetryRef;
        private readonly ISettingsService _settingsService;
        private readonly ShellRef _shellRef;

        private PlaycenterClient _client;
        private CancellationTokenSource _cts;
        private ToolkitShellUi _shellUi;

        public IShellUi Shell => _shellUi;

        public PlaycenterSdkBootstrap(
            IAuthService authService,
            ISessionLifecycle sessionLifecycle,
            ISessionContext sessionContext,
            IAppFlow appFlow,
            IConnectivityService connectivity,
            INTPTimeService ntpTimeService,
            IRemoteConfigService remoteConfigService,
            IMaintenanceService maintenanceService,
            IAnalyticsService analytics,
            IEventBus eventBus,
            BootRetryRef bootRetryRef,
            ISettingsService settingsService,
            ShellRef shellRef)
        {
            _authService = authService;
            _sessionLifecycle = sessionLifecycle;
            _sessionContext = sessionContext;
            _appFlow = appFlow;
            _connectivity = connectivity;
            _ntpTimeService = ntpTimeService;
            _remoteConfigService = remoteConfigService;
            _maintenanceService = maintenanceService;
            _analytics = analytics;
            _eventBus = eventBus;
            _bootRetryRef = bootRetryRef;
            _settingsService = settingsService;
            _shellRef = shellRef;
            // Eager shell so it exists before Start() binds it into ShellRef.
            _shellUi = new ToolkitShellUi();
        }

        /// <summary>VContainer entry point: fires SDK boot asynchronously.</summary>
        public void Start()
        {
            // Bind after construction so consumers (ShellRef / IPlaycenterBootRetry) reach the
            // live shell and retry without a circular dependency on this entry point.
            _bootRetryRef.Bind(this);
            _shellRef.Bind(_shellUi);
            _cts = new CancellationTokenSource();
            Run(_cts.Token).Forget();
        }

        /// <summary>
        /// Called by <see cref="IPlaycenterBootRetry"/> consumers (e.g. NoConnectionPhase) to
        /// re-run the full module pipeline after connectivity is restored.
        /// </summary>
        public void Retry()
        {
            if (_client == null) return;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _client.RetryBootAsync(_cts.Token).AsUniTask().Forget();
        }

        private async UniTaskVoid Run(CancellationToken ct)
        {
            var entry = new RecipeRageGameEntry(
                _authService, _sessionLifecycle, _sessionContext, _appFlow, _analytics);

            _client = PlaycenterClient.Create(o =>
            {
                o.UseDefaultModules();
                o.UseShell(_shellUi);
                o.UseTheme(new ShellTheme("UI/Themes/DesignSystem"));
                o.SetGameEntry(entry);

                // Pass live VContainer-resolved instances so modules and game share the same objects.
                o.Services.AddSingleton<IConnectivityService>(_connectivity);
                o.Services.AddSingleton<INTPTimeService>(_ntpTimeService);
                o.Services.AddSingleton<IRemoteConfigService>(_remoteConfigService);
                o.Services.AddSingleton<IMaintenanceService>(_maintenanceService);
                o.Services.AddSingleton<IAnalyticsService>(_analytics);
                o.Services.AddSingleton<ISettingsService>(_settingsService);
                o.Services.AddSingleton<IForceUpdatePolicy>(
                    new KitchenClashForceUpdatePolicy(_remoteConfigService, _eventBus));
                o.Services.AddSingleton<IAppVersion>(new AppVersionAdapter());
            });

            // Wire shell callbacks after client created so we have access to it
            _shellUi.SetServices(_client.Services);
            _shellUi.OnRetryRequested = Retry;
            _shellUi.OnQuitRequested = OnQuit;
            _shellUi.OnUpdateRequested = OnUpdate;

            await _client.RunAsync(ct).AsUniTask();
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }

        private void OnUpdate()
        {
            // Get store URL from typed RC config; fallback to platform default
            string storeUrl = "https://reciperage.game/download";
            if (_remoteConfigService != null
                && _remoteConfigService.TryGetConfig<ForceUpdateConfig>(out var cfg)
                && !string.IsNullOrEmpty(cfg.UpdateUrl))
            {
                storeUrl = cfg.UpdateUrl;
            }

            UnityEngine.Application.OpenURL(storeUrl);
        }
    }
}
