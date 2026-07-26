namespace RecipeRage
{
    /// <summary>
    /// Resolved ability values for a chef at a level. Personal utility ONLY —
    /// affects the owning player, never shared stations or cook/chop speed.
    /// </summary>
    public readonly struct ChefAbilityModifier
    {
        public float MoveSpeedMultiplier { get; }
        public float PickupDropSpeedMultiplier { get; }
        public int CarryCapacityBonus { get; }
        public bool HasDash { get; }
        public float DashCooldownSec { get; }

        public ChefAbilityModifier(
            float moveSpeedMultiplier,
            float pickupDropSpeedMultiplier,
            int carryCapacityBonus,
            bool hasDash,
            float dashCooldownSec)
        {
            MoveSpeedMultiplier = moveSpeedMultiplier;
            PickupDropSpeedMultiplier = pickupDropSpeedMultiplier;
            CarryCapacityBonus = carryCapacityBonus;
            HasDash = hasDash;
            DashCooldownSec = dashCooldownSec;
        }

        public static ChefAbilityModifier None => new ChefAbilityModifier(1f, 1f, 0, false, 0f);
    }
}
