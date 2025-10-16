using System;
using Core.Audio;
using Core.Authentication;
using Core.Characters;
using Core.GameModes;
using Core.Input;
using Core.Logging;
using Core.Networking;
using Core.SaveSystem;
using Core.State;

namespace Core.Bootstrap
{
    /// <summary>
    /// Central service container - single source of truth for all services
    /// Replaces all singletons with proper dependency injection
    /// </summary>
    public class ServiceContainer : IDisposable
    {
        // Core Services
        public ISaveService SaveService { get; private set; }
        public IAudioService AudioService { get; private set; }
        public IInputService InputService { get; private set; }
        public ILoggingService LoggingService { get; private set; }

        // Game Systems
        public IGameModeService GameModeService { get; private set; }
        public ICharacterService CharacterService { get; private set; }
        public IGameStateManager StateManager { get; private set; }

        // Authentication
        public IAuthenticationService AuthenticationService { get; private set; }

        // Networking
        public INetworkingServices NetworkingServices { get; private set; }

        // Registration methods
        public void RegisterSaveService(ISaveService service) => SaveService = service;
        public void RegisterAudioService(IAudioService service) => AudioService = service;
        public void RegisterInputService(IInputService service) => InputService = service;
        public void RegisterLoggingService(ILoggingService service) => LoggingService = service;
        public void RegisterGameModeService(IGameModeService service) => GameModeService = service;
        public void RegisterCharacterService(ICharacterService service) => CharacterService = service;
        public void RegisterStateManager(IGameStateManager manager) => StateManager = manager;
        public void RegisterAuthenticationService(IAuthenticationService service) => AuthenticationService = service;
        public void RegisterNetworkingServices(INetworkingServices services) => NetworkingServices = services;

        /// <summary>
        /// Update all services that need per-frame updates
        /// </summary>
        public void Update(float deltaTime)
        {
            InputService?.Update(deltaTime);
            StateManager?.Update(deltaTime);
        }

        /// <summary>
        /// Fixed update for physics-dependent services
        /// </summary>
        public void FixedUpdate(float fixedDeltaTime)
        {
            StateManager?.FixedUpdate(fixedDeltaTime);
        }

        /// <summary>
        /// Clean up all services
        /// </summary>
        public void Dispose()
        {
            LoggingService?.Dispose();
            (SaveService as IDisposable)?.Dispose();
            (AudioService as IDisposable)?.Dispose();
            (InputService as IDisposable)?.Dispose();
            (NetworkingServices as IDisposable)?.Dispose();
        }
    }
}
