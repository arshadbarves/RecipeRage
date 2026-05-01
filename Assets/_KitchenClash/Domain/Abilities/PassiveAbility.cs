namespace KitchenClash.Domain
{
    /// <summary>
    /// Generic passive ability wrapper. Always active, no cooldown, always succeeds.
    /// The gameplay layer reads the definition's Value to apply the passive effect.
    /// </summary>
    public sealed class PassiveAbility : IAbility
    {
        public AbilityDefinition Definition { get; }
        public AbilitySlot Slot => AbilitySlot.Passive;
        public float Cooldown => 0f;

        public PassiveAbility(AbilityDefinition definition)
        {
            Definition = definition;
        }

        public bool CanActivate(AbilityContext ctx) => true;

        public AbilityResult Activate(AbilityContext ctx)
        {
            // Passives are always active; activation is a no-op that reports success.
            return new AbilityResult(AbilityEffectType.None, Definition.Value, 0f, true);
        }

        public void Deactivate() { /* Passives have no deactivation */ }
    }
}
