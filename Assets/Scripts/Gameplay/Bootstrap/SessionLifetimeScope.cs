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

            // EOS Lobby Manager (Find in scene as it's a plugin MonoBehaviour)
            // Note: In a production setup, this would be part of a persistent prefab
            var eosLobbyManager = FindObjectOfType<EOSLobbyManager>();
            if (eosLobbyManager != null)
            {
                builder.RegisterInstance(eosLobbyManager);
            }

            // Lobby Service
            builder.Register<LobbyService>(Lifetime.Singleton).As<ILobbyManager>();

            // Player Manager
            builder.Register<PlayerManager>(Lifetime.Singleton).As<IPlayerManager>();

            // UI Stack Manager
            builder.Register<UIScreenStackManager>(Lifetime.Singleton).As<IUIScreenStackManager>();
        }
    }
}
