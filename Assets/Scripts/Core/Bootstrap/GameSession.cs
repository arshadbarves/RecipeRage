using System;
using Core.Audio;
using Core.Characters;
using Core.Currency;
using Core.Events;
using Core.GameModes;
using Core.Input;
using Core.Logging;
using Core.Networking;
using Core.Networking.Services;
using Core.SaveSystem;
using Core.Skins;
using Core.State;
using UnityEngine;

namespace Core.Bootstrap
{
    /// <summary>
    /// Represents a logged-in user session.
    /// Holds all services that strictly require an authenticated user environment.
    /// Accessing these services is only possible when a valid Session exists.
    /// </summary>
    public class GameSession : IDisposable
    {
        // ============================================
        // APPLICATION SERVICES (Eager within Session)
        // ============================================

        public ICurrencyService CurrencyService { get; private set; }
        public IAudioService AudioService { get; private set; }
        public IInputService InputService { get; private set; }

        // ============================================
        // GAME SYSTEMS (Eager within Session)
        // ============================================

        public IGameModeService GameModeService { get; private set; }
        public ICharacterService CharacterService { get; private set; }
        public ISkinsService SkinsService { get; private set; }
        public INetworkingServices NetworkingServices { get; private set; }

        // ============================================
        // NETWORK GAME SERVICES (Eager within Session)
        // ============================================

        public INetworkGameManager NetworkGameManager { get; private set; }
        public IPlayerNetworkManager PlayerNetworkManager { get; private set; }
        public INetworkObjectPool NetworkObjectPool { get; private set; }

        public GameSession(ISaveService saveService, IEventBus eventBus, ILoggingService loggingService)
        {
            GameLogger.Log("Initializing GameSession...");

            // Logic moved from ServiceContainer lazy factories
            CurrencyService = new CurrencyService(saveService, eventBus);

            var poolManager = new AudioPoolManager(GameBootstrap.Instance.transform);
            var volumeController = new AudioVolumeController(saveService);
            var musicPlayer = new MusicPlayer(volumeController);
            var sfxPlayer = new SFXPlayer(poolManager, volumeController);
            AudioService = new AudioService(musicPlayer, sfxPlayer, volumeController);
            
            var inputProvider = InputProviderFactory.CreateForPlatform();
            InputService = new InputService(inputProvider);

            GameModeService = new GameModeService();
            CharacterService = new CharacterService();
            SkinsService = new SkinsService();
            NetworkingServices = new NetworkingServiceContainer();

            PlayerNetworkManager = new PlayerNetworkManager(loggingService);
            NetworkObjectPool = new NetworkObjectPool(loggingService);
            NetworkGameManager = new NetworkGameManager(loggingService, PlayerNetworkManager);

            GameLogger.Log("GameSession Initialized");
        }

        public void Update(float deltaTime)
        {
            InputService?.Update(deltaTime);

            if (NetworkingServices is NetworkingServiceContainer networkingContainer)
            {
                networkingContainer.Update();
            }
        }

        public void FixedUpdate(float fixedDeltaTime)
        {
        }

        public void Dispose()
        {
            GameLogger.Log("Disposing GameSession...");
            
            // NetworkingServices implements IDisposable
            (NetworkingServices as IDisposable)?.Dispose();

            GameLogger.Log("GameSession Disposed");
        }
    }
}
