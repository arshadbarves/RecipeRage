using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.DI;
using KitchenClash.Infrastructure.Gameplay;
using VContainer;
using VContainer.Unity;

public class MenuLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Session management
        builder.Register<SessionManager>(Lifetime.Scoped).As<IInitializable>();
        builder.Register<SessionContext>(Lifetime.Scoped).As<ISessionContext>();

        // Menu services
        builder.Register<MatchService>(Lifetime.Scoped).As<IMatchService>();
        builder.Register<EconomyService>(Lifetime.Scoped).As<IEconomyService>();
        builder.Register<DailyStreakService>(Lifetime.Scoped).As<IDailyStreakService>();
        builder.Register<MapRotationCalculator>(Lifetime.Scoped);

        // Character service (uses ChefRegistry singleton from root)
        builder.Register<CharacterService>(Lifetime.Scoped).As<ICharacterService>();
    }
}
