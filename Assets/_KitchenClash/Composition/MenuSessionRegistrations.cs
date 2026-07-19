using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.EOS;
using KitchenClash.Infrastructure.Gameplay;
using KitchenClash.Infrastructure.Network;
using KitchenClash.Infrastructure.Services;
using KitchenClash.Presentation.ViewModels;
using Playcenter.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace KitchenClash.Composition
{
    /// <summary>
    /// Shared menu/session DI registrations used by both:
    /// - <see cref="MenuLifetimeScope"/> (scene-owned scope on MainMenu)
    /// - <see cref="MenuSessionScopeInstaller"/> (cold-boot SessionManager.CreateSession child)
    /// </summary>
    public static class MenuSessionRegistrations
    {
        public static void Install(IContainerBuilder builder)
        {
            // Menu services (SessionManager/SessionContext live on Root for cold boot)
            builder.Register<MatchService>(Lifetime.Scoped).As<IMatchService>();
            builder.Register<EconomyService>(Lifetime.Scoped)
                .AsSelf()
                .As<IEconomyService>()
                .As<IWallet>()
                .As<IWalletLedger>();
            builder.RegisterEntryPoint<MatchRewardHandler>();
            builder.Register<DailyStreakService>(Lifetime.Scoped).As<IDailyStreakService>();
            builder.Register<TrophyService>(Lifetime.Scoped).As<ITrophyService>();
            builder.Register<MapRotationCalculator>(Lifetime.Scoped);
            builder.Register<ShopCatalog>(Lifetime.Scoped);

            // Character service (uses ChefRegistry singleton from root)
            builder.Register<CharacterService>(Lifetime.Scoped).As<ICharacterService>();
            builder.Register<SkinsService>(Lifetime.Scoped).As<ISkinsService>();
            builder.Register<GameModeService>(Lifetime.Scoped).As<IGameModeService>();

            // Tutorial
            builder.Register<TutorialService>(Lifetime.Scoped).As<ITutorialService>();

            // Dual-track EOS lobby stack (party ≠ match). Explicit LobbyInterface ids —
            // never sample EOSLobbyManager.CurrentLobby.
            builder.Register<EOSTeamManager>(Lifetime.Scoped).As<ITeamManager>();
            builder.Register<EOSLobbyService>(Lifetime.Scoped).As<ILobbyManager>();
            builder.Register<EOSPlayerManager>(Lifetime.Scoped).As<IPlayerManager>();
            builder.Register<EOSMatchmakingService>(Lifetime.Scoped).As<IMatchmakingService>();

            // Playcenter net session (NGO+EOS). IMatchContext is match-scoped; adapter
            // falls back to NetworkManager.Singleton until SetMatchContext is called
            // (e.g. from NetworkingServiceContainer when match context is available).
            builder.Register<NgoEosNetSession>(Lifetime.Scoped).AsSelf().As<INetSession>();
            builder.RegisterEntryPoint<NetSessionConnectivityBridge>();

            // Session networking facade (lobby/player/mm + GameStarter). IMatchContext
            // is match-scoped only — force null here; bridge sets it later.
            builder.Register<NetworkingServiceContainer>(Lifetime.Scoped)
                .AsSelf()
                .As<INetworkingServices>()
                .WithParameter(typeof(IMatchContext), (IMatchContext)null);
            builder.Register<IGameStarter>(
                c => c.Resolve<NetworkingServiceContainer>().GameStarter,
                Lifetime.Scoped);

            // Scene MonoBehaviour preview port for lobby / character details.
            // Falls back to root NullCharacterPreviewService when the component is not in the scene.
            CharacterPreviewManager preview = Object.FindFirstObjectByType<CharacterPreviewManager>();
            if (preview != null)
            {
                builder.RegisterComponent(preview).As<ICharacterPreviewService>();
            }

            // ViewModels
            builder.Register<HomeScreenViewModel>(Lifetime.Transient);
            builder.Register<DailyStreakViewModel>(Lifetime.Transient);
            builder.Register<MatchmakingViewModel>(Lifetime.Transient);
        }
    }
}
