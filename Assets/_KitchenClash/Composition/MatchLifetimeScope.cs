using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Gameplay.Abilities;
using KitchenClash.Infrastructure.Network;
using VContainer;
using VContainer.Unity;

public class MatchLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Recipe catalog
        builder.Register<RecipeCatalog>(Lifetime.Scoped);

        // Ability factory
        builder.Register<AbilityFactory>(Lifetime.Scoped);

        // Ability effect handler (dispatches domain events to concrete Infrastructure abilities)
        builder.Register<AbilityEffectHandler>(Lifetime.Scoped).As<System.IDisposable>();

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

        // Connectivity: notify match lifecycle for state machine
        builder.Register<MatchConnectivityBridge>(Lifetime.Scoped)
            .As<IStartable>()
            .As<System.IDisposable>();
    }
}

/// <summary>
/// Bridges match lifecycle to connectivity state machine.
/// Calls NotifyMatchStarted on start, NotifyMatchEnded on dispose.
/// </summary>
internal sealed class MatchConnectivityBridge : IStartable, System.IDisposable
{
    private readonly IConnectivityService _connectivity;

    public MatchConnectivityBridge(IConnectivityService connectivity)
    {
        _connectivity = connectivity;
    }

    void IStartable.Start() => _connectivity.NotifyMatchStarted();
    void System.IDisposable.Dispose() => _connectivity.NotifyMatchEnded();
}
