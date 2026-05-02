using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Application.State;
using KitchenClash.Composition;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Ads;
using KitchenClash.Infrastructure.Analytics;
using KitchenClash.Infrastructure.Audio;
using KitchenClash.Infrastructure.DI;
using KitchenClash.Infrastructure.EOS;
using KitchenClash.Infrastructure.Localization;
using KitchenClash.Infrastructure.Logging;
using KitchenClash.Infrastructure.Network;
using KitchenClash.Infrastructure.Persistence;
using KitchenClash.Infrastructure.Services;
using KitchenClash.Presentation.Common;
using KitchenClash.Presentation.ViewModels;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class RootLifetimeScope : LifetimeScope
{
    [SerializeField] private UGSConfig _ugsConfig;
    [SerializeField] private KitchenClash.Infrastructure.Audio.AudioSettings _audioSettings;

    protected override void Configure(IContainerBuilder builder)
    {
        // ── Core singletons ──
        builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
        builder.Register<FallbackConfigService>(Lifetime.Singleton).As<IConfigService>();
        builder.Register<UnityLoggingService>(Lifetime.Singleton).As<ILoggingService>();
        builder.Register<EncryptionService>(Lifetime.Singleton).As<IEncryptionService>();
        builder.Register<NetworkConnectivityService>(Lifetime.Singleton).As<IConnectivityService>().As<ITickable>();
        builder.Register<NTPTimeService>(Lifetime.Singleton).As<INTPTimeService>().As<IInitializable>();

        // ── Audio ──
        if (_audioSettings != null)
        {
            builder.RegisterInstance(_audioSettings);
        }
        else
        {
            builder.RegisterInstance(ScriptableObject.CreateInstance<KitchenClash.Infrastructure.Audio.AudioSettings>());
        }

        builder.Register<AudioVolumeController>(Lifetime.Singleton).As<IAudioVolumeController>().As<IInitializable>();
        builder.Register<AudioPoolManager>(Lifetime.Singleton).WithParameter<Transform>(transform);
        builder.Register<MusicPlayer>(Lifetime.Singleton).As<IMusicPlayer>();
        builder.Register<SFXPlayer>(Lifetime.Singleton).As<ISFXPlayer>();
        builder.Register<AudioService>(Lifetime.Singleton).As<IAudioService>();
        builder.Register<AudioEventListener>(Lifetime.Singleton);

        // ── Shared GDD registries ──
        builder.Register<ChefRegistry>(Lifetime.Singleton);
        builder.Register<MapRegistry>(Lifetime.Singleton);

        // ── UI ──
        builder.Register<UIService>(Lifetime.Singleton).As<IUIService>().As<IStartable>().As<ITickable>();

        // ── Localization ──
        builder.Register<LocalizationManager>(Lifetime.Singleton).As<ILocalizationManager>().As<IInitializable>();

        // ── State machine ──
        builder.Register<GameStateFactory>(Lifetime.Singleton).As<IStateFactory>();
        builder.Register<GameStateManager>(Lifetime.Singleton).As<IGameStateManager>().As<ITickable>();

        // ── Persistence ──
        builder.Register<PlayerDataService>(Lifetime.Singleton).As<IPlayerDataService>();
        builder.Register<LocalSaveService>(Lifetime.Singleton).As<ISaveService>();

        // ── Remote Config (composite: Firebase-first with fallback) ──
#if FIREBASE_REMOTE_CONFIG
        builder.Register<KitchenClash.Infrastructure.Firebase.FirebaseConfigProvider>(Lifetime.Singleton).As<IConfigProvider>();
        builder.Register<CompositeRemoteConfigService>(Lifetime.Singleton).As<IRemoteConfigService>();
#else
        builder.Register<CompositeRemoteConfigService>(Lifetime.Singleton).As<IRemoteConfigService>();
#endif

        // ── Maintenance ──
        builder.Register<MaintenanceService>(Lifetime.Singleton).As<IMaintenanceService>();

        // ── Analytics ──
        builder.Register<FirebaseAnalyticsService>(Lifetime.Singleton).As<IAnalyticsService>();

        // ── Ads ──
        builder.Register<StubAdsService>(Lifetime.Singleton).As<IAdsService>();

        // ── Auth ──
        // UGSConfig ScriptableObject – use serialized field if assigned, otherwise create default
        if (_ugsConfig != null)
        {
            builder.RegisterInstance(_ugsConfig);
        }
        else
        {
            builder.RegisterInstance(ScriptableObject.CreateInstance<UGSConfig>());
        }

        builder.Register<AuthenticationService>(Lifetime.Singleton).As<IAuthService>();

        // ── ViewModels (transient, injected into screens) ──
        builder.Register<LoginViewModel>(Lifetime.Transient);

        // ── Game states (transient, resolved by IStateFactory) ──
        builder.Register<KitchenClash.Infrastructure.States.BootstrapState>(Lifetime.Transient);
        builder.Register<KitchenClash.Infrastructure.States.LoginState>(Lifetime.Transient);
        builder.Register<KitchenClash.Infrastructure.States.MainMenuState>(Lifetime.Transient);
        builder.Register<KitchenClash.Infrastructure.States.SessionLoadingState>(Lifetime.Transient);
        builder.Register<KitchenClash.Infrastructure.States.MaintenanceState>(Lifetime.Transient);
        builder.Register<KitchenClash.Infrastructure.States.MatchmakingState>(Lifetime.Transient);
        builder.Register<KitchenClash.Infrastructure.States.GameplayState>(Lifetime.Transient);
        builder.Register<KitchenClash.Infrastructure.States.GameOverState>(Lifetime.Transient);

        // ── Entry point ──
        builder.RegisterEntryPoint<GameBootstrapper>();
    }
}
