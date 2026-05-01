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
            builder.Register(resolver => EOSManager.Instance.GetOrCreateManager<EOSLobbyManager>(), Lifetime.Scoped);
            builder.Register<TeamManager>(Lifetime.Singleton).As<ITeamManager>();
            builder.Register<PlayerManager>(Lifetime.Singleton).As<IPlayerManager>();
            builder.Register(resolver =>
                {
                    var service = new LobbyService(
                        resolver.Resolve<EOSLobbyManager>(),
                        resolver.Resolve<ITeamManager>());
                    service.Initialize();
                    return service;
                }, Lifetime.Singleton)
                .As<ILobbyManager>()
                .As<System.IDisposable>();
            builder.Register(resolver =>
                {
                    var service = new MatchmakingService(
                        resolver.Resolve<ILobbyManager>(),
                        resolver.Resolve<EOSLobbyManager>());
                    service.Initialize();
                    return service;
                }, Lifetime.Singleton)
                .As<IMatchmakingService>();

            // 1. Networking Service Container (The Main Controller)
            // Register this FIRST as other services depend on it
            builder.Register<Gameplay.App.Networking.NetworkingServiceContainer>(Lifetime.Singleton)
                .As<INetworkingServices>()
                .As<System.IDisposable>();

            // 2. Delegate Interface Registrations to Container
            // Core session services are now created directly by DI; the container composes them.
            builder.Register(r => r.Resolve<INetworkingServices>().GameStarter, Lifetime.Singleton).As<IGameStarter>();
        }
    }
}
