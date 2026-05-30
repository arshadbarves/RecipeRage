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
        private readonly IEventBus _eventBus;

        private static readonly Dictionary<AbilityType, AbilityDefinition> Definitions = new()
        {
            // ── Legacy Passives (kept for backward compat) ──
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

            // ── GDD v3 Passives ──
            [AbilityType.QuickHands] = new AbilityDefinition(
                AbilityType.QuickHands, AbilitySlot.Passive,
                "Quick Hands", "-1 chop tap required", value: 1f),

            [AbilityType.LongToss] = new AbilityDefinition(
                AbilityType.LongToss, AbilitySlot.Passive,
                "Long Toss", "+2 tile throw range", value: 2f),

            [AbilityType.ZenFocus] = new AbilityDefinition(
                AbilityType.ZenFocus, AbilitySlot.Passive,
                "Zen Focus", "+3s burn delay", value: 3f),

            [AbilityType.SecretRecipe] = new AbilityDefinition(
                AbilityType.SecretRecipe, AbilitySlot.Passive,
                "Secret Recipe", "5% chance to double points", value: 0.05f),

            [AbilityType.Conductor] = new AbilityDefinition(
                AbilityType.Conductor, AbilitySlot.Passive,
                "Conductor", "Adjacent teammates +10% speed", value: 0.10f),

            [AbilityType.HotHands] = new AbilityDefinition(
                AbilityType.HotHands, AbilitySlot.Passive,
                "Hot Hands", "Cook 20% faster", value: 0.20f),

            // ── GDD v3 Actives ──
            [AbilityType.SprintDash] = new AbilityDefinition(
                AbilityType.SprintDash, AbilitySlot.Active,
                "Sprint Dash", "Dash forward at high speed",
                cooldown: 8f, duration: 0f, value: 3f),

            [AbilityType.FlavorBurst] = new AbilityDefinition(
                AbilityType.FlavorBurst, AbilitySlot.Active,
                "Flavor Burst", "Nearby dishes gain +1 quality tier",
                cooldown: 10f, duration: 5f, value: 1f),

            [AbilityType.CalmStep] = new AbilityDefinition(
                AbilityType.CalmStep, AbilitySlot.Active,
                "Calm Step", "Immune to hazards for 4s",
                cooldown: 10f, duration: 4f, value: 1f),

            [AbilityType.StumbleCharge] = new AbilityDefinition(
                AbilityType.StumbleCharge, AbilitySlot.Active,
                "Stumble Charge", "Charge forward, bumping opponents",
                cooldown: 12f, duration: 0f, value: 3f),

            [AbilityType.PrepRelay] = new AbilityDefinition(
                AbilityType.PrepRelay, AbilitySlot.Active,
                "Prep Relay", "Pass ingredient to nearest teammate instantly",
                cooldown: 10f, duration: 0f, value: 1f),

            [AbilityType.SpiceBlast] = new AbilityDefinition(
                AbilityType.SpiceBlast, AbilitySlot.Active,
                "Spice Blast", "AoE stun nearby opponents for 2s",
                cooldown: 15f, duration: 2f, value: 1f),

            // ── GDD v3 Supers ──
            [AbilityType.KitchenRush] = new AbilityDefinition(
                AbilityType.KitchenRush, AbilitySlot.Super,
                "Kitchen Rush", "All actions instant for 6s",
                cooldown: 0f, duration: 6f, value: 1f),

            [AbilityType.GrandService] = new AbilityDefinition(
                AbilityType.GrandService, AbilitySlot.Super,
                "Grand Service", "Completes the longest active order instantly",
                cooldown: 0f, duration: 0f, value: 1f),

            [AbilityType.PerfectPlating] = new AbilityDefinition(
                AbilityType.PerfectPlating, AbilitySlot.Super,
                "Perfect Plating", "2x speed bonus on next 3 serves",
                cooldown: 0f, duration: 0f, value: 3f),

            [AbilityType.FamilyFeast] = new AbilityDefinition(
                AbilityType.FamilyFeast, AbilitySlot.Super,
                "Family Feast", "Auto-serves all Tier 1 orders",
                cooldown: 0f, duration: 0f, value: 1f),

            [AbilityType.Symphony] = new AbilityDefinition(
                AbilityType.Symphony, AbilitySlot.Super,
                "Symphony", "Team sees all orders + speed buff",
                cooldown: 0f, duration: 10f, value: 1f),

            [AbilityType.CurryOverdrive] = new AbilityDefinition(
                AbilityType.CurryOverdrive, AbilitySlot.Super,
                "Curry Overdrive", "All stations cook instantly for 8s",
                cooldown: 0f, duration: 8f, value: 1f),

            // ── GDD v3 Gadgets ──
            [AbilityType.StickyMat] = new AbilityDefinition(
                AbilityType.StickyMat, AbilitySlot.Gadget,
                "Sticky Mat", "Place a mat that slows opponents",
                cooldown: 20f, duration: 8f, value: 0.5f),

            [AbilityType.RecipeShortcut] = new AbilityDefinition(
                AbilityType.RecipeShortcut, AbilitySlot.Gadget,
                "Recipe Shortcut", "Skip one prep step on current dish",
                cooldown: 25f, duration: 0f, value: 1f),

            [AbilityType.FireproofGloves] = new AbilityDefinition(
                AbilityType.FireproofGloves, AbilitySlot.Gadget,
                "Fireproof Gloves", "Immune to fire for 10s",
                cooldown: 30f, duration: 10f, value: 1f),

            [AbilityType.VintageSpice] = new AbilityDefinition(
                AbilityType.VintageSpice, AbilitySlot.Gadget,
                "Vintage Spice", "Upgrade T1 dish to T2",
                cooldown: 25f, duration: 0f, value: 1f),

            [AbilityType.MiseEnPlace] = new AbilityDefinition(
                AbilityType.MiseEnPlace, AbilitySlot.Gadget,
                "Mise en Place", "Start round with 3 pre-prepped items",
                cooldown: 0f, duration: 0f, value: 3f),

            [AbilityType.PressureCooker] = new AbilityDefinition(
                AbilityType.PressureCooker, AbilitySlot.Gadget,
                "Pressure Cooker", "3x cook speed for 15s",
                cooldown: 45f, duration: 15f, value: 3f),

            // ── Legacy Actives (kept for backward compat) ──
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

        public AbilityFactory(IEventBus eventBus = null)
        {
            _eventBus = eventBus;
        }

        public AbilityDefinition GetDefinition(AbilityType type)
        {
            return Definitions.TryGetValue(type, out AbilityDefinition def) ? def : null;
        }

        /// <summary>
        /// Creates a concrete IAbility for the given type. Returns null for unknown types.
        /// Passives return a PassiveAbility wrapper; actives return their specific implementation.
        /// </summary>
        public IAbility CreateAbility(AbilityType type)
        {
            AbilityDefinition def = GetDefinition(type);
            if (def == null)
            {
                return null;
            }

            if (def.Slot == AbilitySlot.Passive)
            {
                return new PassiveAbility(def);
            }

            return type switch
            {
                _ => new GenericActiveAbility(def, _eventBus),
            };
        }
    }
}
