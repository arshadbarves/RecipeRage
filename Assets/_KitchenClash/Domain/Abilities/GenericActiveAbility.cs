namespace KitchenClash.Domain
{
    /// <summary>
    /// Default active ability implementation for the Application layer.
    /// Infrastructure can override with concrete subclasses (e.g., DashAbility) at runtime via DI.
    /// </summary>
    public class GenericActiveAbility : IAbility
    {
        public AbilityDefinition Definition { get; }
        public AbilitySlot Slot => AbilitySlot.Active;
        public float Cooldown => Definition.Cooldown;

        public GenericActiveAbility(AbilityDefinition definition)
        {
            Definition = definition;
        }

        public virtual bool CanActivate(AbilityContext ctx) => true;

        public virtual AbilityResult Activate(AbilityContext ctx)
        {
            if (!CanActivate(ctx))
                return AbilityResult.Failed;

            return new AbilityResult(AbilityEffectType.None, Definition.Value, Definition.Duration, true);
        }

        public virtual void Deactivate() { }
    }
}
