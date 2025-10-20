using System;
using Core.Animation;
using Core.Audio;
using Core.Authentication;
using Core.Characters;
using Core.Currency;
using Core.Events;
using Core.GameModes;
using Core.Input;
using Core.Interfaces;
using Core.Logging;
using Core.Maintenance;
using Core.Networking;
using Core.SaveSystem;
using Core.State;
using UI.UISystem;
using UnityEngine;

namespace Core.Bootstrap
{
    /// <summary>
    /// Central service container with lazy loading support
    /// Follows Pyramid Architecture:
    /// - Foundation services (eager): EventBus, Animation, UI
    /// - Core services (eager): Save, Auth
    /// - Application services (lazy): Currency, Audio, Input, Logging
    /// - Game systems (lazy): GameMode, Character, Network, State
    /// </summary>
    public class ServiceContainer : IDisposable
    {
        // ============================================
        // FOUNDATION SERVICES (Eager - Always Loaded)
        // ============================================
        
        public IEventBus EventBus { get; private set; }
        public IAnimationService AnimationService { get; private set; }
        public IUIService UIService { get; private set; }

        // ============================================
        // CORE SERVICES (Eager - Pre-Auth)
        // ============================================

        public ISaveService SaveService { get; private set; }
        public IAuthenticationService AuthenticationService { get; private set; }
        public IMaintenanceService MaintenanceService { get; private set; }

        // ============================================
        // APPLICATION SERVICES (Lazy - Post-Auth)
        // ============================================
        
        private ICurrencyService _currencyService;
        public ICurrencyService CurrencyService => 
            _currencyService ??= CreateCurrencyService();

        private IAudioService _audioService;
        public IAudioService AudioService => 
            _audioService ??= CreateAudioService();

        private IInputService _inputService;
        public IInputService InputService => 
            _inputService ??= CreateInputService();

        private ILoggingService _loggingService;
        public ILoggingService LoggingService => 
            _loggingService ??= CreateLoggingService();

        // ============================================
        // GAME SYSTEMS (Lazy - On-Demand)
        // ============================================
        
        private IGameModeService _gameModeService;
        public IGameModeService GameModeService => 
            _gameModeService ??= CreateGameModeService();

        private ICharacterService _characterService;
        public ICharacterService CharacterService => 
            _characterService ??= CreateCharacterService();

        private INetworkingServices _networkingServices;
        public INetworkingServices NetworkingServices => 
            _networkingServices ??= CreateNetworkingServices();

        private IGameStateManager _stateManager;
        public IGameStateManager StateManager => 
            _stateManager ??= CreateStateManager();

        // ============================================
        // REGISTRATION METHODS (Eager Services)
        // ============================================
        
        public void RegisterEventBus(IEventBus eventBus) => EventBus = eventBus;
        public void RegisterAnimationService(IAnimationService service) => AnimationService = service;
        public void RegisterUIService(IUIService service) => UIService = service;
        public void RegisterSaveService(ISaveService service) => SaveService = service;
        public void RegisterAuthenticationService(IAuthenticationService service) => AuthenticationService = service;
        public void RegisterMaintenanceService(IMaintenanceService service) => MaintenanceService = service;

        // ============================================
        // LAZY SERVICE FACTORIES
        // ============================================
        
        private ICurrencyService CreateCurrencyService()
        {
            Debug.Log("[ServiceContainer] Lazy-loading CurrencyService");
            return new CurrencyService(SaveService, EventBus);
        }

        private IAudioService CreateAudioService()
        {
            Debug.Log("[ServiceContainer] Lazy-loading AudioService");
            var poolManager = new AudioPoolManager(GameBootstrap.Instance.transform);
            var volumeController = new AudioVolumeController(SaveService);
            var musicPlayer = new MusicPlayer(volumeController);
            var sfxPlayer = new SFXPlayer(poolManager, volumeController);
            return new AudioService(musicPlayer, sfxPlayer, volumeController);
        }

        private IInputService CreateInputService()
        {
            Debug.Log("[ServiceContainer] Lazy-loading InputService");
            var provider = InputProviderFactory.CreateForPlatform();
            return new InputService(provider);
        }

        private ILoggingService CreateLoggingService()
        {
            Debug.Log("[ServiceContainer] Lazy-loading LoggingService");
            return new LoggingService(maxLogEntries: 5000);
        }

        private IGameModeService CreateGameModeService()
        {
            Debug.Log("[ServiceContainer] Lazy-loading GameModeService");
            return new GameModeService();
        }

        private ICharacterService CreateCharacterService()
        {
            Debug.Log("[ServiceContainer] Lazy-loading CharacterService");
            return new CharacterService();
        }

        private INetworkingServices CreateNetworkingServices()
        {
            Debug.Log("[ServiceContainer] Lazy-loading NetworkingServices");
            return new NetworkingServiceContainer();
        }

        private IGameStateManager CreateStateManager()
        {
            Debug.Log("[ServiceContainer] Lazy-loading StateManager");
            return new GameStateManager();
        }

        // ============================================
        // LIFECYCLE MANAGEMENT
        // ============================================

        /// <summary>
        /// Reset user-specific services on logout
        /// Keeps foundation and core services alive
        /// </summary>
        public void ResetUserServices()
        {
            Debug.Log("[ServiceContainer] Resetting user services...");

            // Reset resettable services
            (_currencyService as IResettableService)?.Reset();

            // Dispose and nullify disposable services (will be recreated on next access)
            DisposeService(ref _gameModeService, "GameModeService");
            DisposeService(ref _characterService, "CharacterService");
            DisposeService(ref _networkingServices, "NetworkingServices");
            DisposeService(ref _stateManager, "StateManager");

            // Notify SaveService to switch to local storage
            SaveService?.OnUserLoggedOut();

            Debug.Log("[ServiceContainer] User services reset complete");
        }

        private void DisposeService<T>(ref T service, string serviceName) where T : class
        {
            if (service != null)
            {
                (service as IDisposable)?.Dispose();
                service = null;
                Debug.Log($"[ServiceContainer] Disposed {serviceName}");
            }
        }

        /// <summary>
        /// Update all services that need per-frame updates
        /// </summary>
        public void Update(float deltaTime)
        {
            // Only update if services are loaded
            _inputService?.Update(deltaTime);
            _stateManager?.Update(deltaTime);
            UIService?.Update(deltaTime);
        }

        /// <summary>
        /// Fixed update for physics-dependent services
        /// </summary>
        public void FixedUpdate(float fixedDeltaTime)
        {
            _stateManager?.FixedUpdate(fixedDeltaTime);
        }

        /// <summary>
        /// Clean up all services
        /// </summary>
        public void Dispose()
        {
            Debug.Log("[ServiceContainer] Disposing all services");

            // Dispose lazy services
            DisposeService(ref _currencyService, "CurrencyService");
            DisposeService(ref _audioService, "AudioService");
            DisposeService(ref _inputService, "InputService");
            DisposeService(ref _loggingService, "LoggingService");
            DisposeService(ref _gameModeService, "GameModeService");
            DisposeService(ref _characterService, "CharacterService");
            DisposeService(ref _networkingServices, "NetworkingServices");
            DisposeService(ref _stateManager, "StateManager");

            // Dispose eager services
            (SaveService as IDisposable)?.Dispose();
            (UIService as IDisposable)?.Dispose();
            (AnimationService as IDisposable)?.Dispose();

            // Clear event bus
            EventBus?.ClearAllSubscriptions();

            Debug.Log("[ServiceContainer] All services disposed");
        }
    }
}
