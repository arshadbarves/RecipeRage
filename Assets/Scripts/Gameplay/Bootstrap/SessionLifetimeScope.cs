using VContainer;
using VContainer.Unity;
using Core.Networking.Services;
using Core.Networking.Interfaces;
using Core.UI;
using Core.UI.Interfaces;
using Gameplay.Economy;
using Gameplay.Persistence;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;

namespace Gameplay.Bootstrap
{
    public class SessionLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // 1. Networking Service Container (The Main Controller)
            // Register this FIRST as other services depend on it
            builder.Register<Gameplay.App.Networking.NetworkingServiceContainer>(Lifetime.Singleton)
                .As<Core.Networking.INetworkingServices>()
                .As<System.IDisposable>();

            // 2. Delegate Interface Registrations to Container
            // This ensures we use the EXACT SAME instances that NetworkingServiceContainer created
            builder.Register(r => r.Resolve<Core.Networking.INetworkingServices>().LobbyManager, Lifetime.Singleton).As<ILobbyManager>();
            builder.Register(r => r.Resolve<Core.Networking.INetworkingServices>().PlayerManager, Lifetime.Singleton).As<IPlayerManager>();
            builder.Register(r => r.Resolve<Core.Networking.INetworkingServices>().MatchmakingService, Lifetime.Singleton).As<IMatchmakingService>();
            builder.Register(r => r.Resolve<Core.Networking.INetworkingServices>().TeamManager, Lifetime.Singleton).As<ITeamManager>();

            // EOS Lobby Manager (Wrapper for EOS SDK)
            builder.Register(resolver => EOSManager.Instance.GetOrCreateManager<EOSLobbyManager>(), Lifetime.Scoped);

            // UI Stack Manager
            builder.Register<UIScreenStackManager>(Lifetime.Singleton).As<IUIScreenStackManager>();

            // 2. Economy & Player Data (Game Layer Services)
            builder.Register<EconomyService>(Lifetime.Singleton);
            builder.Register<PlayerDataService>(Lifetime.Singleton);
        }
    }
}
