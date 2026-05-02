namespace KitchenClash.Domain
{
    /// <summary>
    /// Immutable GDD chef definition: identity, stats, unlock gate, 4 ability slots.
    /// </summary>
    public sealed class ChefDefinition
    {
        public ChefId Id { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public ChefStatBlock Stats { get; }
        public UnlockRequirement Unlock { get; }
        public AbilityType PassiveAbility { get; }
        public AbilityType ActiveAbility { get; }
        public AbilityType SuperAbility { get; }
        public AbilityType GadgetAbility { get; }

        public ChefDefinition(
            ChefId id,
            string displayName,
            string description,
            ChefStatBlock stats,
            UnlockRequirement unlock,
            AbilityType passiveAbility,
            AbilityType activeAbility,
            AbilityType superAbility = AbilityType.None,
            AbilityType gadgetAbility = AbilityType.None)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            Stats = stats;
            Unlock = unlock;
            PassiveAbility = passiveAbility;
            ActiveAbility = activeAbility;
            SuperAbility = superAbility;
            GadgetAbility = gadgetAbility;
        }
    }
}
