namespace KitchenClash.Domain
{
    /// <summary>
    /// Immutable data describing an ability's properties per GDD v3.
    /// </summary>
    public sealed class AbilityDefinition
    {
        public AbilityType Type { get; }
        public AbilitySlot Slot { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public float Cooldown { get; }
        public float Duration { get; }
        public float Value { get; }

        public AbilityDefinition(
            AbilityType type,
            AbilitySlot slot,
            string displayName,
            string description,
            float cooldown = 0f,
            float duration = 0f,
            float value = 0f)
        {
            Type = type;
            Slot = slot;
            DisplayName = displayName;
            Description = description;
            Cooldown = cooldown;
            Duration = duration;
            Value = value;
        }
    }
}
