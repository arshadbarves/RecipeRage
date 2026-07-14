using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Gameplay;
using KitchenClash.Presentation.ViewModels;
using VContainer;
using VContainer.Unity;

public class MenuLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Menu services (SessionManager/SessionContext live on Root for cold boot)
        builder.Register<MatchService>(Lifetime.Scoped).As<IMatchService>();
        builder.Register<EconomyService>(Lifetime.Scoped).As<IEconomyService>();
        builder.Register<DailyStreakService>(Lifetime.Scoped).As<IDailyStreakService>();
        builder.Register<TrophyService>(Lifetime.Scoped).As<ITrophyService>();
        builder.Register<MapRotationCalculator>(Lifetime.Scoped);
        builder.Register<ShopCatalog>(Lifetime.Scoped);

        // Character service (uses ChefRegistry singleton from root)
        builder.Register<CharacterService>(Lifetime.Scoped).As<ICharacterService>();

        // Tutorial
        builder.Register<TutorialService>(Lifetime.Scoped).As<ITutorialService>();

        // ViewModels
        builder.Register<HomeScreenViewModel>(Lifetime.Transient);
        builder.Register<DailyStreakViewModel>(Lifetime.Transient);
        builder.Register<MatchmakingViewModel>(Lifetime.Transient);
    }
}
