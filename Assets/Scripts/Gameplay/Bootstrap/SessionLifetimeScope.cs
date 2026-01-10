using VContainer;
using VContainer.Unity;
using Core.Networking.Services;
using Core.Networking.Interfaces;
using Core.UI;
using Core.UI.Interfaces;
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
            // Use factory registration to defer access until resolution time
            builder.Register(resolver => EOSManager.Instance.GetOrCreateManager<EOSLobbyManager>(), Lifetime.Scoped);

            // Lobby Service
            builder.Register<LobbyService>(Lifetime.Singleton).As<ILobbyManager>();

            // Player Manager
            builder.Register<PlayerManager>(Lifetime.Singleton).As<IPlayerManager>();

            // UI Stack Manager
            builder.Register<UIScreenStackManager>(Lifetime.Singleton).As<IUIScreenStackManager>();
        }
    }
}
