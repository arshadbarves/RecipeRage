namespace Gameplay.Character.Stats
{
    public enum StatType
    {
        // Base Stats
        Health,
        MaxHealth,
        Defense,
        MovementSpeed,

        // Combat Stats
        AttackPower,
        CriticalChance,
        CriticalMultiplier,

        // Cooking Stats
        CookingSpeed,
        InteractionRange,
        CarryCapacity,

        // Resource Stats
        Resource,
        MaxResource,
        ResourceRegenRate,

        // Utility Stats
        CooldownReduction,
        BuffDuration,
        DebuffResistance
    }
}
