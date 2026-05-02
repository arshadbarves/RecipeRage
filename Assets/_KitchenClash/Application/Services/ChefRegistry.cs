using System.Collections.Generic;
using System.Linq;
using KitchenClash.Domain;
using KitchenClash.Application.Models.RemoteConfigs;

namespace KitchenClash.Application.Services
{
    /// <summary>
    /// Hardcoded GDD v3 chef definitions (6 chefs). Singleton, no Unity dependency.
    /// </summary>
    public class ChefRegistry
    {
        private readonly Dictionary<ChefId, ChefDefinition> _chefs;

        public ChefRegistry()
        {
            _chefs = BuildAll().ToDictionary(c => c.Id);
        }

        public ChefDefinition Get(ChefId id) => _chefs.TryGetValue(id, out var c) ? c : null;

        public IReadOnlyList<ChefDefinition> GetAll() => _chefs.Values.ToList();

        /// <summary>
        /// Returns chefs whose unlock requirement is satisfied by the given player level and economy.
        /// </summary>
        public IReadOnlyList<ChefDefinition> GetUnlockedChefs(int playerLevel, IEconomyService economy)
        {
            var result = new List<ChefDefinition>();
            foreach (var chef in _chefs.Values)
            {
                if (IsUnlocked(chef, playerLevel, economy))
                    result.Add(chef);
            }
            return result;
        }

        public bool IsUnlocked(ChefDefinition chef, int playerLevel, IEconomyService economy)
        {
            switch (chef.Unlock.Type)
            {
                case UnlockType.Starter:
                    return true;
                case UnlockType.Level:
                    return playerLevel >= chef.Unlock.Value;
                case UnlockType.Shop:
                    // Shop chefs are unlocked once purchased; economy tracks ownership.
                    return economy != null && economy.HasItem($"chef_{chef.Id}");
                default:
                    return false;
            }
        }

        /// <summary>
        /// Applies remote config overrides to chef stat blocks.
        /// Call after remote config is fetched.
        /// </summary>
        public void ApplyRemoteConfig(CharacterConfig config)
        {
            if (config?.Overrides == null) return;

            foreach (var ov in config.Overrides)
            {
                if (string.IsNullOrEmpty(ov.ChefId)) continue;
                if (!System.Enum.TryParse<ChefId>(ov.ChefId, true, out var chefId)) continue;
                if (!_chefs.TryGetValue(chefId, out var chef)) continue;

                var stats = chef.Stats;
                var newStats = new ChefStatBlock(
                    ov.SpeedMultiplier >= 0 ? ov.SpeedMultiplier : stats.MoveSpeed,
                    ov.CookingSpeedMultiplier >= 0 ? ov.CookingSpeedMultiplier : stats.CookSpeedMult,
                    ov.FireResistance >= 0 ? ov.FireResistance : stats.BurnResistance,
                    ov.CarryCapacity >= 0 ? ov.CarryCapacity : stats.CarryCapacity,
                    ov.InteractRange >= 0 ? ov.InteractRange : stats.InteractRange
                );

                _chefs[chefId] = new ChefDefinition(
                    chef.Id, chef.DisplayName, chef.Description, newStats,
                    chef.Unlock, chef.PassiveAbility, chef.ActiveAbility,
                    chef.SuperAbility, chef.GadgetAbility);

                GameLogger.Log($"[ChefRegistry] Applied remote overrides for {chefId}");
            }
        }

        private static IEnumerable<ChefDefinition> BuildAll()
        {
            // 1. Rosa — balanced, Italian grandmother. Starter.
            yield return new ChefDefinition(
                ChefId.Rosa,
                "Rosa",
                "Italian grandmother with decades of kitchen wisdom. A balanced all-rounder.",
                new ChefStatBlock(1.1f, 1.0f, 0.0f, 1, 1.0f),
                UnlockRequirement.Starter(),
                AbilityType.QuickHands,
                AbilityType.SprintDash,
                AbilityType.KitchenRush,
                AbilityType.StickyMat);

            // 2. Marco — speed-focused, young culinary student. Level 5 (GDD: 5 wins).
            yield return new ChefDefinition(
                ChefId.Marco,
                "Marco",
                "Young culinary student who compensates with raw speed and hustle.",
                new ChefStatBlock(1.3f, 1.0f, 0.0f, 1, 1.0f),
                UnlockRequirement.AtLevel(5),
                AbilityType.LongToss,
                AbilityType.FlavorBurst,
                AbilityType.GrandService,
                AbilityType.RecipeShortcut);

            // 3. Yuki — precision chef, Japanese sushi master. Level 10 (GDD: 200 trophies).
            yield return new ChefDefinition(
                ChefId.Yuki,
                "Yuki",
                "Japanese sushi master. Precision over speed — nothing burns on her watch.",
                new ChefStatBlock(1.0f, 1.0f, 0.3f, 1, 1.1f),
                UnlockRequirement.AtLevel(10),
                AbilityType.ZenFocus,
                AbilityType.CalmStep,
                AbilityType.PerfectPlating,
                AbilityType.FireproofGloves);

            // 4. Grandpa — tank/support, retired head chef. Shop 500 coins (GDD: 20 matches).
            yield return new ChefDefinition(
                ChefId.Grandpa,
                "Grandpa",
                "Retired head chef. Tough as nails, knows every trick in the book.",
                new ChefStatBlock(0.9f, 1.0f, 0.7f, 1, 1.0f),
                UnlockRequirement.ForCoins(500),
                AbilityType.SecretRecipe,
                AbilityType.StumbleCharge,
                AbilityType.FamilyFeast,
                AbilityType.VintageSpice);

            // 5. Bella — conductor/support, competitive pastry chef. Level 15 (GDD: S1 BP T30).
            yield return new ChefDefinition(
                ChefId.Bella,
                "Bella",
                "Competitive pastry chef who orchestrates the kitchen like a symphony.",
                new ChefStatBlock(1.0f, 1.0f, 0.0f, 1, 1.2f),
                UnlockRequirement.AtLevel(15),
                AbilityType.Conductor,
                AbilityType.PrepRelay,
                AbilityType.Symphony,
                AbilityType.MiseEnPlace);

            // 6. Raj — multi-tasker, street food vendor. Shop 1000 coins (GDD: 500 trophies S1).
            yield return new ChefDefinition(
                ChefId.Raj,
                "Raj",
                "Street food vendor who juggles multiple orders with ease.",
                new ChefStatBlock(1.0f, 1.0f, 0.0f, 2, 1.0f),
                UnlockRequirement.ForCoins(1000),
                AbilityType.HotHands,
                AbilityType.SpiceBlast,
                AbilityType.CurryOverdrive,
                AbilityType.PressureCooker);
        }
    }
}
