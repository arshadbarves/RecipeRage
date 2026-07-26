namespace RecipeRage
{
    public enum ChefAbilityType
    {
        MoveSpeed,          // Gordon: +1% per level (max +10%)
        PickupDropSpeed,    // Julia: +1.5% per level (max +15%)
        CarryCapacity,      // Marco: +1 at L5, +1 at L10 (max 4 items)
        Dash                // Gustavo: -2s cooldown per level (30s → 10s min)
    }

    public enum ChefRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }
}
