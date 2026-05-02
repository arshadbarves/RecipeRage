namespace KitchenClash.Domain
{
    /// <summary>
    /// Published when an active ability is activated. Infrastructure listeners apply the actual effect.
    /// </summary>
    public sealed class AbilityActivatedEvent
    {
        public AbilityType AbilityType { get; }
        public AbilityEffectType EffectType { get; }
        public float Value { get; }
        public float Duration { get; }
        public AbilityContext Context { get; }

        public AbilityActivatedEvent(AbilityType abilityType, AbilityEffectType effectType, float value, float duration, AbilityContext context)
        {
            AbilityType = abilityType;
            EffectType = effectType;
            Value = value;
            Duration = duration;
            Context = context;
        }
    }
}
