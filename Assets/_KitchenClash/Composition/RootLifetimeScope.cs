using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Application.State;
using KitchenClash.Composition;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.DI;
using KitchenClash.Infrastructure.Logging;
using KitchenClash.Infrastructure.Network;
using KitchenClash.Infrastructure.Persistence;
using KitchenClash.Presentation.Common;
using VContainer;
using VContainer.Unity;

public class RootLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // ── Core singletons ──
        builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
        builder.Register<FallbackConfigService>(Lifetime.Singleton).As<IConfigService>();
        builder.Register<UnityLoggingService>(Lifetime.Singleton).As<ILoggingService>();
        builder.Register<EncryptionService>(Lifetime.Singleton).As<IEncryptionService>();
        builder.Register<NetworkConnectivityService>(Lifetime.Singleton).As<IConnectivityService>().As<ITickable>();
        builder.Register<NTPTimeService>(Lifetime.Singleton).As<INTPTimeService>().As<IInitializable>();

        // ── UI ──
        builder.Register<UIService>(Lifetime.Singleton).As<IUIService>().As<IStartable>().As<ITickable>();

        // ── State machine ──
        builder.Register<GameStateFactory>(Lifetime.Singleton).As<IStateFactory>();
        builder.Register<GameStateManager>(Lifetime.Singleton).As<IGameStateManager>().As<ITickable>();

        // ── Persistence ──
        builder.Register<PlayerDataService>(Lifetime.Singleton).As<IPlayerDataService>();
        // SaveService requires StorageProviderFactory – register when persistence layer is ready
        // builder.Register<SaveService>(Lifetime.Singleton).As<ISaveService>();

        // ── Remote Config (Phase 3 – requires IConfigProvider / Firebase) ──
        // builder.Register<RemoteConfigService>(Lifetime.Singleton).As<IRemoteConfigService>().As<IStartable>();

        // ── Maintenance (Phase 3 – requires IRemoteConfigService) ──
        // builder.Register<MaintenanceService>(Lifetime.Singleton).As<IMaintenanceService>();

        // ── Auth (Phase 3 – requires EOS/UGS) ──
        // builder.Register<AuthenticationService>(Lifetime.Singleton).As<IAuthService>();

        // ── Game states (transient, resolved by IStateFactory) ──
        builder.Register<KitchenClash.Infrastructure.States.BootstrapState>(Lifetime.Transient);
        builder.Register<KitchenClash.Infrastructure.States.LoginState>(Lifetime.Transient);
        builder.Register<KitchenClash.Infrastructure.States.MainMenuState>(Lifetime.Transient);
        builder.Register<KitchenClash.Infrastructure.States.SessionLoadingState>(Lifetime.Transient);
        builder.Register<KitchenClash.Infrastructure.States.MatchmakingState>(Lifetime.Transient);
        builder.Register<KitchenClash.Infrastructure.States.GameplayState>(Lifetime.Transient);
        builder.Register<KitchenClash.Infrastructure.States.GameOverState>(Lifetime.Transient);

        // ── Entry point ──
        builder.RegisterEntryPoint<GameBootstrapper>();
    }
}
