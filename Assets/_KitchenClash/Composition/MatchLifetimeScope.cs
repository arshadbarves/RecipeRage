using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Network;
using VContainer;
using VContainer.Unity;

public class MatchLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Match services
        builder.Register<ScoreService>(Lifetime.Scoped).As<IScoreService>();
        builder.Register<OrderService>(Lifetime.Scoped).As<IOrderService>();
        builder.Register<AbilityService>(Lifetime.Scoped).As<IAbilityService>();
        builder.Register<HazardService>(Lifetime.Scoped).As<IHazardService>();

        // Match context
        builder.Register<MatchContext>(Lifetime.Scoped).As<IMatchContext>();

        // Bot services
        builder.Register<BotManager>(Lifetime.Scoped);
        builder.Register<BotClaimRegistry>(Lifetime.Scoped);
        builder.Register<BotTaskPlanner>(Lifetime.Scoped);
    }
}
