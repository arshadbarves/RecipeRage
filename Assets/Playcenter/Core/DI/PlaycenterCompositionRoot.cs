using System;
using System.Collections;
using Playcenter.Services;
using UnityEngine;
using UnityEngine.Audio;

namespace Playcenter
{
    /// <summary>
    /// Boot composition root. Constructs + initializes every SDK service,
    /// registers them in ServiceLocator, then fires OnPlaycenterInitialized.
    /// GameplayCompositionRoot listens for that event before building game services.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class PlaycenterCompositionRoot : MonoBehaviour
    {
        public static event Action OnPlaycenterInitialized;

        [Header("Audio")]
        [SerializeField] private AudioMixer _mainMixer;
        [SerializeField] private AudioClipMap _clipMap;

        private void Awake()
        {

            // SDK services (FULL logic)
            var storageService = new EOSCloudStorageService(loggingService);
            DontDestroyOnLoad(gameObject);

            // Core primitives
            var eventBus = new EventBus();
            var loggingService = new UnityLoggingService();
            var timeService = new UnityTimeService();
            var saveService = new EOSCloudSaveService(storageService);
            var configService = new FirebaseConfigService(loggingService);
            var analyticsService = new FirebaseAnalyticsService(loggingService);
            var authService = new AuthService(saveService, loggingService, analyticsService);
            var eosTransport = new EOSPlayerDataTransport(authService, loggingService);
            storageService.SetTransport(eosTransport);
            var adsService = new AdMobService(loggingService, analyticsService);
            var iapService = new UnityIAPService(loggingService, analyticsService);
            var friendsService = new UnityGamingServicesFriends(saveService, loggingService);
            var audioService = new UnityAudioService(_mainMixer, transform);
            var walletService = new CoinWalletService(saveService, analyticsService);

            if (_clipMap != null)
            {
                _clipMap.RegisterAll(audioService);
            }

            var audioSystem = new AudioSystem(audioService);
            audioSystem.Initialize(eventBus);

            var uiService = new Playcenter.UI.UIService();

            // Register
            ServiceLocator.Register<IEventBus>(eventBus);
            ServiceLocator.Register<ILoggingService>(loggingService);
            ServiceLocator.Register<ITimeService>(timeService);
            ServiceLocator.Register<IStorageService>(storageService);
            ServiceLocator.Register<ISaveService>(saveService);
            ServiceLocator.Register<IConfigService>(configService);
            ServiceLocator.Register<IAnalyticsService>(analyticsService);
            ServiceLocator.Register<IAuthService>(authService);
            ServiceLocator.Register<IAdsService>(adsService);
            ServiceLocator.Register<IIAPService>(iapService);
            ServiceLocator.Register<IFriendsService>(friendsService);
            ServiceLocator.Register<IAudioService>(audioService);
            ServiceLocator.Register<IWalletService>(walletService);
            ServiceLocator.Register<Playcenter.UI.IUIService>(uiService);

            StartCoroutine(InitializeSDK());
        }

        private IEnumerator InitializeSDK()
        {
            yield return ServiceLocator.Get<IStorageService>().Initialize();
            yield return ServiceLocator.Get<IConfigService>().Initialize();
            yield return ServiceLocator.Get<IAuthService>().Initialize();

            if (ServiceLocator.Get<ISaveService>() is EOSCloudSaveService cloudSave)
            {
                yield return cloudSave.Preload(new[]
                {
                    "chef_progress", "trophies", "wallet_coins", "tutorial_completed", "friend_code"
                }).AsCoroutine();
            }

            yield return ServiceLocator.Get<IAnalyticsService>().Initialize();
            yield return ServiceLocator.Get<IAdsService>().Initialize();
            yield return ServiceLocator.Get<IIAPService>().Initialize();
            yield return ServiceLocator.Get<IFriendsService>().Initialize();

            ServiceLocator.Get<ILoggingService>().Log("[Playcenter] SDK initialized");
            OnPlaycenterInitialized?.Invoke();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && ServiceLocator.TryGet<ISaveService>(out var save))
            {
                _ = save.Flush();
            }
        }

        private void OnApplicationQuit()
        {
            if (ServiceLocator.TryGet<ISaveService>(out var save))
            {
                _ = save.Flush();
            }
        }
    }
}
