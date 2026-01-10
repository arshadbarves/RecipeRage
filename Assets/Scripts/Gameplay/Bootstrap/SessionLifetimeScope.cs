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
            // Team Manager
            builder.Register<TeamManager>(Lifetime.Singleton).As<ITeamManager>();

            // EOS Lobby Manager (Get from EOSManager singleton)
            builder.Register(resolver => EOSManager.Instance.GetOrCreateManager<EOSLobbyManager>(), Lifetime.Scoped);

            // Lobby Service
            builder.Register<LobbyService>(Lifetime.Singleton).As<ILobbyManager>();

            // Player Manager
            builder.Register<PlayerManager>(Lifetime.Singleton).As<IPlayerManager>();

            // Matchmaking Service
            builder.Register<MatchmakingService>(Lifetime.Singleton).As<IMatchmakingService>();

            // UI Stack Manager
            builder.Register<UIScreenStackManager>(Lifetime.Singleton).As<IUIScreenStackManager>();

            // 1. Networking Service Container (The Main Controller)
            builder.Register<Gameplay.App.Networking.NetworkingServiceContainer>(Lifetime.Singleton)
                .As<Core.Networking.INetworkingServices>()
                .As<System.IDisposable>();

            // 2. Banking
            builder.Register<Core.Banking.Backends.EOSBankBackend>(Lifetime.Singleton).As<Core.Banking.Interfaces.IBankBackend>();
            builder.Register<Core.Banking.BankService>(Lifetime.Singleton).As<Core.Banking.Interfaces.IBankService>();
        }
    }
}
