using System.Threading;
using Cysharp.Threading.Tasks;
using KitchenClash.Application;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Boot;
using KitchenClash.Infrastructure.Flow;
using Playcenter.GameFlow;
using Playcenter.SDK;
using Playcenter.SDK.Unity;
using Playcenter.Services;
using Playcenter.Shell;
using VContainer.Unity;

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

        private PlaycenterClient _client;
        private CancellationTokenSource _cts;

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
            BootRetryRef bootRetryRef)
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
        }

        /// <summary>VContainer entry point: fires SDK boot asynchronously.</summary>
        public void Start()
        {
            // Bind after construction so AppFlow factory can resolve IPlaycenterBootRetry
            // without a circular dependency on this entry point.
            _bootRetryRef.Bind(this);
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
                o.UseShell(new ToolkitShellUi());
                o.UseTheme(new ShellTheme("UI/Themes/DesignSystem"));
                o.SetGameEntry(entry);

                // Pass live VContainer-resolved instances so modules and game share the same objects.
                o.Services.AddSingleton<IConnectivityService>(_connectivity);
                o.Services.AddSingleton<INTPTimeService>(_ntpTimeService);
                o.Services.AddSingleton<IRemoteConfigService>(_remoteConfigService);
                o.Services.AddSingleton<IMaintenanceService>(_maintenanceService);
                o.Services.AddSingleton<IAnalyticsService>(_analytics);
                o.Services.AddSingleton<IForceUpdatePolicy>(
                    new KitchenClashForceUpdatePolicy(_remoteConfigService, _eventBus));
                o.Services.AddSingleton<IAppVersion>(new AppVersionAdapter());
            });

            await _client.RunAsync(ct).AsUniTask();
        }
    }
}
