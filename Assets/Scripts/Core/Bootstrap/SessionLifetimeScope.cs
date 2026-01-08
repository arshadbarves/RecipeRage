using Modules.Audio;
using Gameplay.Characters;
using Core.Currency;
using Gameplay.GameModes;
using Modules.Input;
using Modules.Networking;
using Modules.Networking.Services;
using Modules.Skins;
using Modules.Core.Banking;
using Modules.Core.Banking.Backends;
using Modules.Core.Banking.Interfaces;
using VContainer;
using VContainer.Unity;

namespace Core.Bootstrap
{
    /// <summary>
    /// LifetimeScope for the authenticated session.
    /// Created when a user logs in, disposed on logout.
    /// </summary>
    public class SessionLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // Banking & Currency
            // builder.Register<LocalDiskBankBackend>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<EOSBankBackend>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<BankService>(Lifetime.Singleton).AsImplementedInterfaces();
            
            // Audio System
            builder.Register<AudioPoolManager>(Lifetime.Singleton).WithParameter(transform);
            builder.Register<AudioVolumeController>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<MusicPlayer>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<SFXPlayer>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<AudioService>(Lifetime.Singleton).AsImplementedInterfaces();

            // Input System
            builder.RegisterInstance(InputProviderFactory.CreateForPlatform());
            builder.Register<InputService>(Lifetime.Singleton).AsImplementedInterfaces();

            // Game Systems
            builder.Register<GameModeService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<MapLoader>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<CharacterService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<SkinsService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<NetworkingServiceContainer>(Lifetime.Singleton).AsImplementedInterfaces();

            // Networking
            builder.Register<PlayerNetworkManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<NetworkObjectPool>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<NetworkGameManager>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}