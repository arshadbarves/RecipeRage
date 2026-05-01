using System.Collections.Generic;
using KitchenClash.Domain;

namespace KitchenClash.Application
{
    /// <summary>
    /// Maps AbilityType → AbilityDefinition with GDD v3-accurate values.
    /// Creates concrete IAbility instances for active abilities.
    /// </summary>
    public sealed class AbilityFactory
    {
        private static readonly Dictionary<AbilityType, AbilityDefinition> Definitions = new()
        {
            // ── Passives (always active, no cooldown) ──
            [AbilityType.SlightSpeedBoost] = new AbilityDefinition(
                AbilityType.SlightSpeedBoost, AbilitySlot.Passive,
                "Slight Speed Boost", "+10% movement speed", value: 0.10f),

            [AbilityType.SpeedBoost] = new AbilityDefinition(
                AbilityType.SpeedBoost, AbilitySlot.Passive,
                "Speed Boost", "+30% movement speed", value: 0.30f),

            [AbilityType.SlowerBurnRate] = new AbilityDefinition(
                AbilityType.SlowerBurnRate, AbilitySlot.Passive,
                "Slower Burn Rate", "Burn timer 30% slower", value: 0.30f),

            [AbilityType.FireResistance] = new AbilityDefinition(
                AbilityType.FireResistance, AbilitySlot.Passive,
                "Fire Resistance", "70% fire damage reduction", value: 0.70f),

            [AbilityType.StealImmunity] = new AbilityDefinition(
                AbilityType.StealImmunity, AbilitySlot.Passive,
                "Steal Immunity", "Cannot have items stolen", value: 1f),

            [AbilityType.CarryTwo] = new AbilityDefinition(
                AbilityType.CarryTwo, AbilitySlot.Passive,
                "Carry Two", "Carry capacity = 2", value: 2f),

            // ── Actives (cooldown-based) ──
            [AbilityType.FlavorBoost] = new AbilityDefinition(
                AbilityType.FlavorBoost, AbilitySlot.Active,
                "Flavor Boost", "Nearby dishes gain +1 quality tier",
                cooldown: 15f, duration: 5f, value: 1f),

            [AbilityType.Dash] = new AbilityDefinition(
                AbilityType.Dash, AbilitySlot.Active,
                "Dash", "Dash forward 3 units",
                cooldown: 10f, duration: 0f, value: 3f),

            [AbilityType.PerfectSlice] = new AbilityDefinition(
                AbilityType.PerfectSlice, AbilitySlot.Active,
                "Perfect Slice", "Current prep completes instantly",
                cooldown: 20f, duration: 0f, value: 1f),

            [AbilityType.KitchenWisdom] = new AbilityDefinition(
                AbilityType.KitchenWisdom, AbilitySlot.Active,
                "Kitchen Wisdom", "Reveal all active orders to team",
                cooldown: 30f, duration: 8f, value: 1f),

            [AbilityType.IngredientSwap] = new AbilityDefinition(
                AbilityType.IngredientSwap, AbilitySlot.Active,
                "Ingredient Swap", "Swap target opponent's held item with random ingredient",
                cooldown: 25f, duration: 0f, value: 1f),

            [AbilityType.SpiceRush] = new AbilityDefinition(
                AbilityType.SpiceRush, AbilitySlot.Active,
                "Spice Rush", "Cook speed 2x",
                cooldown: 20f, duration: 5f, value: 2f),
        };

        public AbilityDefinition GetDefinition(AbilityType type)
        {
            return Definitions.TryGetValue(type, out var def) ? def : null;
        }

        /// <summary>
        /// Creates a concrete IAbility for the given type. Returns null for unknown types.
        /// Passives return a PassiveAbility wrapper; actives return their specific implementation.
        /// </summary>
        public IAbility CreateAbility(AbilityType type)
        {
            var def = GetDefinition(type);
            if (def == null) return null;

            if (def.Slot == AbilitySlot.Passive)
                return new PassiveAbility(def);

            return type switch
            {
                _ => new GenericActiveAbility(def),
            };
        }
    }
}
