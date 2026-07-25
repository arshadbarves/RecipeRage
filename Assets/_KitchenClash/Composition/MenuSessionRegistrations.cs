using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.EOS;
using KitchenClash.Infrastructure.Gameplay;
using KitchenClash.Infrastructure.Network;
using KitchenClash.Infrastructure.Services;
using KitchenClash.Presentation.ViewModels;
using Playcenter.Services;
using VContainer;
using VContainer.Unity;

namespace KitchenClash.Composition
{
    /// <summary>
    /// Sole menu/session DI install path for cold-boot
    /// <see cref="MenuSessionScopeInstaller"/> → <c>SessionManager.CreateSession</c> child.
    /// Do not call from scene <c>MenuLifetimeScope</c> — that scope must stay empty.
    /// Double-install causes orphan entry points (missing parent IEventBus) and double wallet credit.
    /// Character preview is Root <see cref="CharacterPreviewGateway"/> + <c>MenuSceneBinder</c>, not here.
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

            // MobileCore net glue: reconnect FSM (match-mode defaults; mc_reconnect_* RC keys)
            // + orchestrator as the sole start/stop path (wiki law).
            builder.Register<Playcenter.MobileCore.ReconnectStateMachine>(resolver =>
            {
                var config = resolver.Resolve<IConfigService>();
                var clock = Playcenter.MobileCore.PlaycenterBootstrap.Instance != null
                    ? Playcenter.MobileCore.PlaycenterBootstrap.Instance.Core.Clock
                    : new Playcenter.MobileCore.ManualClock();
                return new Playcenter.MobileCore.ReconnectStateMachine(
                    new Playcenter.MobileCore.ReconnectConfig(
                        maxAttempts: config.Get("mc_reconnect_match_attempts", 3),
                        attemptIntervalSeconds: config.Get("mc_reconnect_match_interval_ms", 5000) / 1000f,
                        backoffBaseSeconds: config.Get("mc_reconnect_backoff_base_ms", 1000) / 1000f),
                    clock,
                    seed: System.Environment.TickCount);
            }, Lifetime.Scoped);
            builder.Register<Playcenter.MobileCore.NetSessionOrchestrator>(Lifetime.Scoped);
            builder.Register<Playcenter.MobileCore.ConnectionQualityTracker>(resolver =>
            {
                var config = resolver.Resolve<IConfigService>();
                return new Playcenter.MobileCore.ConnectionQualityTracker(
                    degradedMs: config.Get("mc_reconnect_degraded_ms", 150f),
                    poorMs: config.Get("mc_reconnect_poor_ms", 400f));
            }, Lifetime.Scoped);

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

            // ViewModels
            builder.Register<HomeScreenViewModel>(Lifetime.Transient);
            builder.Register<DailyStreakViewModel>(Lifetime.Transient);
            builder.Register<MatchmakingViewModel>(Lifetime.Transient);
        }
    }
}
