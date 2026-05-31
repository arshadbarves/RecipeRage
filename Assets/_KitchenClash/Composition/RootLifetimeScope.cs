using System;
using System.Linq;
using System.Reflection;
using KitchenClash.Application;
using KitchenClash.Application.Models;
using KitchenClash.Application.Services;
using KitchenClash.Application.State;
using KitchenClash.Composition;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Ads;
using KitchenClash.Infrastructure.Analytics;
using KitchenClash.Infrastructure.Audio;
using KitchenClash.Infrastructure.IAP;
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
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

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
        RegisterViewModels(builder);
        RegisterScreens(builder);
        RegisterGameStates(builder);
        RegisterEntryPoints(builder);
    }

    private void RegisterCoreServices(IContainerBuilder builder)
    {
        builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
        builder.Register<UnityLoggingService>(Lifetime.Singleton).As<ILoggingService>();
        builder.Register<EncryptionService>(Lifetime.Singleton).As<IEncryptionService>().WithParameter("passphrase", "KitchenClash_2026");
        builder.Register<NetworkConnectivityService>(Lifetime.Singleton).As<IConnectivityService>().As<ITickable>();
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
            GameLogger.LogError("AudioSettings not assigned in RootLifetimeScope");
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
            GameLogger.LogError("UIDocument not assigned in RootLifetimeScope");
            return;
        }

        builder.Register<UIService>(Lifetime.Singleton).As<IUIService>().As<IStartable>().As<ITickable>();
        builder.Register<LocalizationManager>(Lifetime.Singleton).As<ILocalizationManager>().As<IInitializable>();
    }

    private void RegisterInfrastructure(IContainerBuilder builder)
    {
        builder.Register<GameStateFactory>(Lifetime.Singleton).As<IStateFactory>();
        builder.Register<GameStateManager>(Lifetime.Singleton).As<IGameStateManager>().As<ITickable>();
        builder.Register<PlayerDataService>(Lifetime.Singleton).As<IPlayerDataService>();
        builder.Register<StorageProviderFactory>(Lifetime.Singleton);
        builder.Register<SaveService>(Lifetime.Singleton).As<ISaveService>();

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
            builder.RegisterInstance(ScriptableObject.CreateInstance<UGSConfig>());
        }

        builder.Register<AuthenticationService>(Lifetime.Singleton).As<IAuthService>();
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

    private void RegisterGameStates(IContainerBuilder builder)
    {
        var stateTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch { return System.Array.Empty<System.Type>(); }
            })
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IState).IsAssignableFrom(t))
            .Where(t => t.Namespace?.StartsWith("KitchenClash.Infrastructure.States") == true);

        foreach (System.Type stateType in stateTypes)
        {
            builder.Register(stateType, Lifetime.Transient);
        }
    }

    private void RegisterEntryPoints(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<KitchenClash.Presentation.Overlays.ConnectivityOverlayPresenter>();
        builder.RegisterEntryPoint<GameBootstrapper>();
    }
}
