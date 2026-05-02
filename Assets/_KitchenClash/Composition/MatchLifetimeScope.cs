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
        builder.RegisterBuildCallback(resolver => {
            var factory = resolver.Resolve<AbilityFactory>();
            // Legacy actives
            factory.RegisterOverride(AbilityType.Dash, def => new DashAbility(def));
            factory.RegisterOverride(AbilityType.FlavorBoost, def => new FlavorBoostAbility(def));
            factory.RegisterOverride(AbilityType.SpiceRush, def => new SpiceRushAbility(def));
            factory.RegisterOverride(AbilityType.PerfectSlice, def => new PerfectSliceAbility(def));
            factory.RegisterOverride(AbilityType.KitchenWisdom, def => new KitchenWisdomAbility(def));
            factory.RegisterOverride(AbilityType.IngredientSwap, def => new IngredientSwapAbility(def));
            // GDD v3 Actives
            factory.RegisterOverride(AbilityType.SprintDash, def => new SprintDashAbility(def));
            factory.RegisterOverride(AbilityType.FlavorBurst, def => new FlavorBurstAbility(def));
            factory.RegisterOverride(AbilityType.CalmStep, def => new CalmStepAbility(def));
            factory.RegisterOverride(AbilityType.StumbleCharge, def => new StumbleChargeAbility(def));
            factory.RegisterOverride(AbilityType.PrepRelay, def => new PrepRelayAbility(def));
            factory.RegisterOverride(AbilityType.SpiceBlast, def => new SpiceBlastAbility(def));
            // GDD v3 Supers
            factory.RegisterOverride(AbilityType.KitchenRush, def => new KitchenRushAbility(def));
            factory.RegisterOverride(AbilityType.GrandService, def => new GrandServiceAbility(def));
            factory.RegisterOverride(AbilityType.PerfectPlating, def => new PerfectPlatingAbility(def));
            factory.RegisterOverride(AbilityType.FamilyFeast, def => new FamilyFeastAbility(def));
            factory.RegisterOverride(AbilityType.Symphony, def => new SymphonyAbility(def));
            factory.RegisterOverride(AbilityType.CurryOverdrive, def => new CurryOverdriveAbility(def));
            // GDD v3 Gadgets
            factory.RegisterOverride(AbilityType.StickyMat, def => new StickyMatAbility(def));
            factory.RegisterOverride(AbilityType.RecipeShortcut, def => new RecipeShortcutAbility(def));
            factory.RegisterOverride(AbilityType.FireproofGloves, def => new FireproofGlovesAbility(def));
            factory.RegisterOverride(AbilityType.VintageSpice, def => new VintageSpiceAbility(def));
            factory.RegisterOverride(AbilityType.MiseEnPlace, def => new MiseEnPlaceAbility(def));
            factory.RegisterOverride(AbilityType.PressureCooker, def => new PressureCookerAbility(def));
        });

        // Match services
        builder.Register<ScoreService>(Lifetime.Scoped).As<IScoreService>();
        builder.Register<OrderService>(Lifetime.Scoped).As<IOrderService>();
        builder.Register<AbilityService>(Lifetime.Scoped).As<IAbilityService>();
        builder.Register<HazardService>(Lifetime.Scoped).As<IHazardService>();
        builder.Register<ChopService>(Lifetime.Scoped);

        // Match context
        builder.Register<MatchContext>(Lifetime.Scoped).As<IMatchContext>();

        // Bot services
        builder.Register<BotManager>(Lifetime.Scoped);
        builder.Register<BotClaimRegistry>(Lifetime.Scoped);
        builder.Register<BotTaskPlanner>(Lifetime.Scoped);
    }
}
