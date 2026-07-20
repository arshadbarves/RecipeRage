using System;
using System.Linq;
using System.Reflection;
using KitchenClash.Application;
using KitchenClash.Application.Models;
using KitchenClash.Application.Services;
using KitchenClash.Composition;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Configuration;
using KitchenClash.Infrastructure.Ads;
using KitchenClash.Infrastructure.Analytics;
using KitchenClash.Infrastructure.Audio;
using KitchenClash.Infrastructure.IAP;
using KitchenClash.Infrastructure.DI;
using KitchenClash.Infrastructure.EOS;
using KitchenClash.Infrastructure.Flow;
using KitchenClash.Infrastructure.Flow.Handlers;
using KitchenClash.Infrastructure.Localization;
using KitchenClash.Infrastructure.Logging;
using KitchenClash.Infrastructure.Network;
using KitchenClash.Infrastructure.Persistence;
using KitchenClash.Infrastructure.Services;
using KitchenClash.Infrastructure.Settings;
using KitchenClash.Infrastructure.Input;
using KitchenClash.Presentation.Common;
using KitchenClash.Presentation.ViewModels;
using Playcenter.UI.Toolkit;
using Playcenter.GameFlow;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;
using Playcenter.Shell;
using Playcenter.Services;
using Playcenter.EOS;
using Playcenter.UI;
using Playcenter.Animation;

public class RootLifetimeScope : LifetimeScope
{
    [SerializeField] private UGSConfig _ugsConfig;
    [SerializeField] private KitchenClash.Infrastructure.Audio.AudioSettings _audioSettings;
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private ChefDatabaseSO _chefDatabase;
    [SerializeField] private MapDatabaseSO _mapDatabase;

    protected override void Configure(IContainerBuilder builder)
    {
        RegisterCoreServices(builder);
        RegisterAudio(builder);
        RegisterUI(builder);
        RegisterInfrastructure(builder);
        RegisterAppFlow(builder);
        RegisterViewModels(builder);
        RegisterScreens(builder);
        RegisterEntryPoints(builder);
    }

    private void RegisterCoreServices(IContainerBuilder builder)
    {
        builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
        builder.Register<UnityLoggingService>(Lifetime.Singleton).As<ILoggingService>();
        // Wire GameLogger as soon as the container builds — before any IInitializable/IStartable.
        // Configure() itself must not call GameLogger (runs pre-build); use Debug for DI misconfig.
        builder.RegisterBuildCallback(container =>
        {
            GameLogger.SetService(container.Resolve<ILoggingService>());
        });
        builder.RegisterEntryPoint<LoggingBootstrap>();
        builder.Register<EncryptionService>(Lifetime.Singleton).As<IEncryptionService>().WithParameter("passphrase", "KitchenClash_2026");
        // AsSelf so NetSessionConnectivityBridge can cast/resolve concrete forfeit events.
        builder.Register<NetworkConnectivityService>(Lifetime.Singleton)
            .AsSelf()
            .As<IConnectivityService>()
            .As<ITickable>();
        builder.Register<NTPTimeService>(Lifetime.Singleton).As<INTPTimeService>().As<IInitializable>();
        builder.RegisterInstance(_chefDatabase);
        builder.RegisterInstance(_mapDatabase);
        builder.Register(c => new ChefRegistry(c.Resolve<ChefDatabaseSO>(), c.Resolve<IEventBus>()), Lifetime.Singleton);
        builder.Register(c => new MapRegistry(c.Resolve<MapDatabaseSO>(), c.Resolve<IEventBus>()), Lifetime.Singleton);
    }

    private void RegisterAudio(IContainerBuilder builder)
    {
        if (_audioSettings != null)
        {
            builder.RegisterInstance(_audioSettings);
        }
        else
        {
            // Pre-container-build: GameLogger is not wired yet.
            UnityEngine.Debug.LogError("[RootLifetimeScope] AudioSettings not assigned");
            return;
        }

        builder.Register<AudioVolumeController>(Lifetime.Singleton).As<IAudioVolumeController>().As<IInitializable>();
        builder.Register<AudioPoolManager>(Lifetime.Singleton).WithParameter<Transform>(transform);
        builder.Register<MusicPlayer>(Lifetime.Singleton).As<IMusicPlayer>();
        builder.Register<SFXPlayer>(Lifetime.Singleton).As<ISFXPlayer>();
        builder.Register<AudioService>(Lifetime.Singleton).As<IAudioService>();
        builder.Register<AudioEventListener>(Lifetime.Singleton).As<IInitializable>();
    }

    private void RegisterUI(IContainerBuilder builder)
    {
        builder.Register<UIScreenStackManager>(Lifetime.Singleton).As<IUIScreenStackManager>();
        if (_uiDocument != null)
        {
            builder.RegisterInstance(_uiDocument);
        }
        else
        {
            // Pre-container-build: GameLogger is not wired yet.
            UnityEngine.Debug.LogError("[RootLifetimeScope] UIDocument not assigned");
            return;
        }

        builder.Register<VContainerScreenInstanceFactory>(Lifetime.Singleton)
            .As<IScreenInstanceFactory>()
            .As<IScopeAwareScreenFactory>();
        builder.Register<UIService>(Lifetime.Singleton).As<IUIService>().AsSelf();
        builder.RegisterEntryPoint<UIServiceEntryPoint>();

        // Presentation ports: null defaults; scene binders attach real adapters to gateways.
        builder.RegisterInstance(KitchenClash.Application.Services.NullMatchHudPort.Instance)
            .As<KitchenClash.Application.Services.IMatchHudPort>();
        // Root-owned gateway — MenuSceneBinder binds CharacterPreviewManager after MainMenu loads.
        // Session/UI always resolve this singleton; never re-register ICharacterPreviewService in children.
        builder.Register<KitchenClash.Application.CharacterPreviewGateway>(Lifetime.Singleton)
            .AsSelf()
            .As<KitchenClash.Application.ICharacterPreviewService>();
        builder.Register<LocalizationManager>(Lifetime.Singleton).As<ILocalizationManager>().As<IInitializable>();

        // Animation leaf assembly — DOTween-backed IAnimationService for Presentation screens
        builder.Register<DOTweenUIAnimator>(Lifetime.Singleton).As<IUIAnimator>();
        builder.Register<DOTweenTransformAnimator>(Lifetime.Singleton).As<ITransformAnimator>();
        builder.Register<AnimationService>(Lifetime.Singleton).As<IAnimationService>();
    }

    private void RegisterInfrastructure(IContainerBuilder builder)
    {
        // Session scope for SDK ready path (SessionLoader / RecipeRageGameEntry). Root owns SessionManager;
        // scene MenuLifetimeScope does not install session services (empty Configure).
        // MenuSessionScopeInstaller is the sole CreateSession install path for economy/lobby/MM.
        builder.Register<MenuSessionScopeInstaller>(Lifetime.Singleton).As<ISessionScopeInstaller>();
        builder.Register<SessionManager>(Lifetime.Singleton)
            .AsSelf()
            .As<ISessionLifecycle>()
            .As<IInitializable>();
        builder.Register<SessionContext>(Lifetime.Singleton).As<ISessionContext>();
        builder.Register<MatchmakingPhaseHost>(Lifetime.Singleton).AsSelf().As<ITickable>();

        builder.Register<PlayerDataService>(Lifetime.Singleton).As<IPlayerDataService>();
        builder.Register<Playcenter.EOS.EOSCloudStorageProvider>(Lifetime.Singleton).As<ICloudStorageProvider>();
        builder.Register<StorageProviderFactory>(Lifetime.Singleton);
        builder.Register<SaveService>(Lifetime.Singleton).As<ISaveService>();
        builder.Register<EOSFriendsServiceFactory>(Lifetime.Singleton).As<IFriendsServiceFactory>();
        builder.Register<EOSLocalNetworkIdentity>(Lifetime.Singleton).As<ILocalNetworkIdentity>();
        builder.Register<EOSClientTransportConfigurator>(Lifetime.Singleton)
            .As<IClientTransportConfigurator>()
            .As<INetTransportConfigurator>();

        // Local device settings + gameplay input ports (root-owned; not match-scoped).
        builder.Register<PlayerPrefsSettingsStore>(Lifetime.Singleton).As<ISettingsStore>();
        builder.Register<PlayerPrefsSettingsService>(Lifetime.Singleton).As<ISettingsService>();
        builder.Register<GameplayInputService>(Lifetime.Singleton)
            .AsSelf()
            .As<IGameplayInput>()
            .As<IGameplayInputPublisher>();

#if FIREBASE_REMOTE_CONFIG
        builder.Register<KitchenClash.Infrastructure.Firebase.FirebaseConfigProvider>(Lifetime.Singleton).As<IConfigProvider>();
        builder.Register(c => new CompositeRemoteConfigService(c.Resolve<IConfigProvider>(), c.Resolve<IEventBus>()), Lifetime.Singleton).As<IConfigService>().As<IRemoteConfigService>();
#else
        builder.Register(c => new CompositeRemoteConfigService(c.Resolve<IEventBus>()), Lifetime.Singleton).As<IConfigService>().As<IRemoteConfigService>();
#endif
        builder.Register<MaintenanceService>(Lifetime.Singleton).As<IMaintenanceService>();
        builder.Register<FirebaseAnalyticsService>(Lifetime.Singleton).As<IAnalyticsService>();
        builder.Register<StubAdsService>(Lifetime.Singleton).As<IAdsService>();
        builder.Register<StubIAPService>(Lifetime.Singleton).As<IIAPService>();

        if (_ugsConfig != null)
        {
            builder.RegisterInstance(_ugsConfig);
        }
        else
        {
            Debug.LogError("UGSConfig not assigned in RootLifetimeScope");
        }

        builder.Register<UgsEosConfigAdapter>(Lifetime.Singleton).As<IEOSConfig>();
        builder.Register<KitchenClashAuthLifecycleHooks>(Lifetime.Singleton).As<IAuthLifecycleHooks>();
        builder.Register<Playcenter.EOS.AuthenticationService>(Lifetime.Singleton).As<IAuthService>();
    }

    private void RegisterAppFlow(IContainerBuilder builder)
    {
        builder.Register<IAppFlow>(resolver =>
        {
            AppFlowController flow = null;
            IAppFlow Proxy() => flow;

            var ui = resolver.Resolve<IUIService>();
            var analytics = resolver.Resolve<IAnalyticsService>();
            var eventBus = resolver.Resolve<IEventBus>();
            var remoteConfig = resolver.Resolve<IRemoteConfigService>();
            var maintenance = resolver.Resolve<IMaintenanceService>();
            var config = resolver.Resolve<IConfigService>();
            var sessionLifecycle = resolver.Resolve<ISessionLifecycle>();
            var sessionContext = resolver.Resolve<ISessionContext>();
            var matchmakingHost = resolver.Resolve<MatchmakingPhaseHost>();

            // Optional services may only exist in menu/session scopes.
            resolver.TryResolve(out IMatchHudPort matchHudPort);
            resolver.TryResolve(out ITutorialService tutorial);
            resolver.TryResolve(out IMatchmakingService matchmakingService);
            resolver.TryResolve(out IGameModeService gameModeService);

            var appFlowProxy = new AppFlowProxy(Proxy);

            var sessionLoader = new SessionLoader(sessionLifecycle, sessionContext);

            var homePhase = new HomePhase(eventBus);
            var matchmakingPhase = new MatchmakingPhase(
                ui, sessionContext, maintenance, config, eventBus, appFlowProxy, matchmakingService);
            matchmakingHost.Phase = matchmakingPhase;

            var matchRuntimePhase = new MatchRuntimePhase(eventBus, sessionContext, gameModeService);
            // Results never mints wallet — publishes MatchEndedEvent for SESSION MatchRewardHandler.
            var resultsPhase = new ResultsPhase(eventBus, matchHudPort);

            var loginPhase = new LoginPhase(ui, eventBus, appFlowProxy, sessionLoader, analytics);
            var maintenancePhase = new MaintenancePhase(maintenance, remoteConfig, eventBus, appFlowProxy);

            // BootRetryRef breaks the cycle: AppFlow factory → IPlaycenterBootRetry ← PlaycenterSdkBootstrap.
            var bootRetry = resolver.Resolve<IPlaycenterBootRetry>();
            var noConnectionPhase = new NoConnectionPhase(ui, eventBus, appFlowProxy, bootRetry);

            var tutorialPhase = new TutorialPhase(ui, eventBus, appFlowProxy, tutorial);
            var accountUpgradePhase = new AccountUpgradePhase(ui, eventBus, appFlowProxy);
            var sidePhases = new SidePhaseFlowPort(
                loginPhase, maintenancePhase, noConnectionPhase, tutorialPhase, accountUpgradePhase);

            // SDK owns Splash/Boot — pass null ports so AppFlowController skips those phases.
            flow = new AppFlowController(
                splash: null,
                boot: null,
                home: new HomeFlowPort(homePhase),
                matchmaking: new MatchmakingFlowPort(matchmakingPhase, ui),
                matchIntro: new MatchIntroFlowPort(ui, appFlowProxy),
                countdown: new CountdownFlowPort(ui, appFlowProxy),
                matchRuntime: new MatchRuntimeFlowPort(matchRuntimePhase),
                results: new ResultsFlowPort(resultsPhase, ui),
                popupPolicy: new SoftPopupPolicy(),
                analytics: new AnalyticsFlowPort(analytics),
                sidePhases: sidePhases);

            return flow;
        }, Lifetime.Singleton);
    }

    private void RegisterViewModels(IContainerBuilder builder)
    {
        builder.Register<LoginViewModel>(Lifetime.Transient);
    }

    private void RegisterScreens(IContainerBuilder builder)
    {
        var screenTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch { return System.Array.Empty<System.Type>(); }
            })
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseUIScreen)))
            .Where(t => t.GetCustomAttribute<UIScreenAttribute>() != null);

        foreach (System.Type screenType in screenTypes)
        {
            builder.Register(screenType, Lifetime.Transient);
        }
    }

    private void RegisterEntryPoints(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<KitchenClash.Presentation.Overlays.ConnectivityOverlayPresenter>();
        // Mutable holder resolved by AppFlow factory; bound by PlaycenterSdkBootstrap.Start().
        builder.Register<BootRetryRef>(Lifetime.Singleton).AsSelf().As<IPlaycenterBootRetry>();
        // PlaycenterSdkBootstrap replaces GameBootstrapper cold-boot path.
        builder.RegisterEntryPoint<PlaycenterSdkBootstrap>();
    }
}
