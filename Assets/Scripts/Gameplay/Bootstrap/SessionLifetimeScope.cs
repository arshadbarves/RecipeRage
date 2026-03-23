using Core.Networking;
using VContainer;
using VContainer.Unity;
using Core.Networking.Services;
using Core.Networking.Interfaces;
using Core.UI;
using Core.UI.Interfaces;
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
                .As<INetworkingServices>()
                .As<System.IDisposable>();

            // 2. Delegate Interface Registrations to Container
            // This ensures we use the EXACT SAME instances that NetworkingServiceContainer created
            builder.Register(r => r.Resolve<INetworkingServices>().LobbyManager, Lifetime.Singleton).As<ILobbyManager>();
            builder.Register(r => r.Resolve<INetworkingServices>().PlayerManager, Lifetime.Singleton).As<IPlayerManager>();
            builder.Register(r => r.Resolve<INetworkingServices>().MatchmakingService, Lifetime.Singleton).As<IMatchmakingService>();
            builder.Register(r => r.Resolve<INetworkingServices>().TeamManager, Lifetime.Singleton).As<ITeamManager>();
            builder.Register(r => r.Resolve<INetworkingServices>().GameStarter, Lifetime.Singleton).As<IGameStarter>();

            // EOS Lobby Manager (Wrapper for EOS SDK)
            builder.Register(resolver => EOSManager.Instance.GetOrCreateManager<EOSLobbyManager>(), Lifetime.Scoped); // todo: need to use this
        }
    }
}
