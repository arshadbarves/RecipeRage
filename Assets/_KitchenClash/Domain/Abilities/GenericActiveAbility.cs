namespace KitchenClash.Domain
{
    /// <summary>
    /// Default active ability implementation for the Application layer.
    /// Publishes an AbilityActivatedEvent via IEventBus so Infrastructure can apply concrete effects.
    /// </summary>
    public class GenericActiveAbility : IAbility
    {
        public AbilityDefinition Definition { get; }
        public AbilitySlot Slot => AbilitySlot.Active;
        public float Cooldown => Definition.Cooldown;

        private readonly IEventBus _eventBus;

        public GenericActiveAbility(AbilityDefinition definition, IEventBus eventBus = null)
        {
            Definition = definition;
            _eventBus = eventBus;
        }

        public virtual bool CanActivate(AbilityContext ctx) => true;

        public virtual AbilityResult Activate(AbilityContext ctx)
        {
            if (!CanActivate(ctx))
                return AbilityResult.Failed;

            var effectType = MapEffectType(Definition.Type);

            _eventBus?.Publish(new AbilityActivatedEvent(
                Definition.Type, effectType, Definition.Value, Definition.Duration, ctx));

            return new AbilityResult(effectType, Definition.Value, Definition.Duration, true);
        }

        public virtual void Deactivate() { }

        private static AbilityEffectType MapEffectType(AbilityType type) => type switch
        {
            AbilityType.FlavorBoost => AbilityEffectType.FlavorBoost,
            AbilityType.Dash => AbilityEffectType.Dash,
            AbilityType.PerfectSlice => AbilityEffectType.PerfectSlice,
            AbilityType.KitchenWisdom => AbilityEffectType.KitchenWisdom,
            AbilityType.IngredientSwap => AbilityEffectType.IngredientSwap,
            AbilityType.SpiceRush => AbilityEffectType.SpiceRush,
            AbilityType.SpeedBoost => AbilityEffectType.SpeedBoost,
            AbilityType.InstantCook => AbilityEffectType.InstantCook,
            AbilityType.DoubleIngredients => AbilityEffectType.DoubleIngredients,
            AbilityType.FreezeTime => AbilityEffectType.FreezeTime,
            _ => AbilityEffectType.None,
        };
    }
}
