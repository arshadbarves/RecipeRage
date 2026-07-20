namespace KitchenClash.Domain.Enums
{
    /// <summary>
    /// v2 chef archetype — determines HP, active ability shape, and cross-mode fit.
    /// See wiki/GameplayDesign.md § Roles for full archetype intent.
    /// </summary>
    public enum ChefArchetype
    {
        /// <summary>Speed + chase. Short-range shove. Best: Rush Service / Last Plate.</summary>
        Rusher,

        /// <summary>Production specialist. Prime-buff active. Best: Hell's Kitchen.</summary>
        Cook,

        /// <summary>Map control / sabotage. Force-burns enemy stations. Universal value.</summary>
        Controller,

        /// <summary>Loot specialist. Steal-from-hands active. Best: Hell's Kitchen / Rush Service.</summary>
        Disruptor,
    }
}
