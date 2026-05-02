using System.Collections.Generic;
using System.Linq;
using KitchenClash.Domain;
using KitchenClash.Application.Models.RemoteConfigs;

namespace KitchenClash.Application.Services
{
    /// <summary>
    /// Registry of all GDD map definitions. Provides lookup by MapId and kitchen theme.
    /// </summary>
    public sealed class MapRegistry
    {
        private readonly Dictionary<string, MapDefinition> _maps = new();

        public MapRegistry()
        {
            BuildMaps();
        }

        private void BuildMaps()
        {
            // ── Sushi Shuffle — Japanese kitchen, conveyor belt, fish market ──
            Add(new MapDefinition
            {
                MapId = "sushi_shuffle",
                DisplayName = "Sushi Shuffle",
                Description = "Japanese kitchen with a conveyor belt and fresh fish market ingredients.",
                SceneName = "sushi_shuffle",
                KitchenTheme = "japanese",
                AvailableRecipeIds = new[] { "sushi_roll", "ramen" },
                StationCount = 4,
                Stations = new[]
                {
                    new StationLayout { StationId = "ss_ingredient", Type = StationType.Ingredient, GridX = 0, GridY = 0 },
                    new StationLayout { StationId = "ss_prep", Type = StationType.Prep, GridX = 1, GridY = 0 },
                    new StationLayout { StationId = "ss_cooking", Type = StationType.Cooking, GridX = 2, GridY = 0 },
                    new StationLayout { StationId = "ss_serving", Type = StationType.Serving, GridX = 3, GridY = 0 },
                },
                Hazards = new MapHazardConfig
                {
                    FireChanceMultiplier = 1.0f,
                    HasSpecialHazards = false,
                    SpecialHazardType = null
                }
            });

            // ── Pirate Pot — Ship kitchen, swaying deck, seafood recipes ──
            Add(new MapDefinition
            {
                MapId = "pirate_pot",
                DisplayName = "Pirate Pot",
                Description = "Pirate ship kitchen with a swaying deck and seafood recipes.",
                SceneName = "pirate_pot",
                KitchenTheme = "seafood",
                AvailableRecipeIds = new[] { "sushi_roll" },
                StationCount = 3,
                Stations = new[]
                {
                    new StationLayout { StationId = "pp_ingredient", Type = StationType.Ingredient, GridX = 0, GridY = 0 },
                    new StationLayout { StationId = "pp_cooking", Type = StationType.Cooking, GridX = 1, GridY = 0 },
                    new StationLayout { StationId = "pp_serving", Type = StationType.Serving, GridX = 2, GridY = 0 },
                },
                Hazards = new MapHazardConfig
                {
                    FireChanceMultiplier = 1.2f,
                    HasSpecialHazards = false,
                    SpecialHazardType = null
                }
            });

            // ── Burger Boulevard — American diner, drive-through counter ──
            Add(new MapDefinition
            {
                MapId = "burger_boulevard",
                DisplayName = "Burger Boulevard",
                Description = "American diner with a drive-through counter. Burgers, toast, and egg recipes.",
                SceneName = "burger_boulevard",
                KitchenTheme = "american",
                AvailableRecipeIds = new[] { "burger", "toast", "fried_egg" },
                StationCount = 4,
                Stations = new[]
                {
                    new StationLayout { StationId = "bb_ingredient", Type = StationType.Ingredient, GridX = 0, GridY = 0 },
                    new StationLayout { StationId = "bb_prep", Type = StationType.Prep, GridX = 1, GridY = 0 },
                    new StationLayout { StationId = "bb_cooking", Type = StationType.Cooking, GridX = 2, GridY = 0 },
                    new StationLayout { StationId = "bb_serving", Type = StationType.Serving, GridX = 3, GridY = 0 },
                },
                Hazards = new MapHazardConfig
                {
                    FireChanceMultiplier = 0.8f,
                    HasSpecialHazards = false,
                    SpecialHazardType = null
                }
            });

            // ── Haunted Kitchen — Spooky kitchen, surprise hazards, mixed recipes ──
            Add(new MapDefinition
            {
                MapId = "haunted_kitchen",
                DisplayName = "Haunted Kitchen",
                Description = "Spooky kitchen with surprise hazards and mixed recipes from all themes.",
                SceneName = "haunted_kitchen",
                KitchenTheme = "mixed",
                AvailableRecipeIds = new[] { "basic_salad", "toast", "fried_egg", "sushi_roll", "burger", "pasta_dish", "ramen" },
                StationCount = 5,
                Stations = new[]
                {
                    new StationLayout { StationId = "hk_ingredient", Type = StationType.Ingredient, GridX = 0, GridY = 0 },
                    new StationLayout { StationId = "hk_prep", Type = StationType.Prep, GridX = 1, GridY = 0 },
                    new StationLayout { StationId = "hk_cooking", Type = StationType.Cooking, GridX = 2, GridY = 0 },
                    new StationLayout { StationId = "hk_serving", Type = StationType.Serving, GridX = 3, GridY = 0 },
                    new StationLayout { StationId = "hk_sink", Type = StationType.Sink, GridX = 1, GridY = 1 },
                },
                Hazards = new MapHazardConfig
                {
                    FireChanceMultiplier = 1.5f,
                    HasSpecialHazards = true,
                    SpecialHazardType = "ghost_ingredient"
                }
            });

            // ── Rookie Kitchen — Training map, no hazards, 2v2 ──
            Add(new MapDefinition
            {
                MapId = "rookie_kitchen",
                DisplayName = "Rookie Kitchen",
                Description = "Training kitchen with no hazards. The simplest map for new players.",
                SceneName = "rookie_kitchen",
                KitchenTheme = "training",
                AvailableRecipeIds = new[] { "toast", "fried_egg", "basic_salad" },
                StationCount = 3,
                Stations = new[]
                {
                    new StationLayout { StationId = "rk_ingredient", Type = StationType.Ingredient, GridX = 0, GridY = 0 },
                    new StationLayout { StationId = "rk_cooking", Type = StationType.Cooking, GridX = 1, GridY = 0 },
                    new StationLayout { StationId = "rk_serving", Type = StationType.Serving, GridX = 2, GridY = 0 },
                },
                Hazards = new MapHazardConfig
                {
                    FireChanceMultiplier = 0.0f,
                    HasSpecialHazards = false,
                    SpecialHazardType = null
                }
            });

            // ── Taco Truck — Dual trucks, medium difficulty, 3v3 ──
            Add(new MapDefinition
            {
                MapId = "taco_truck",
                DisplayName = "Taco Truck",
                Description = "Dual food trucks with Mexican-themed recipes. Medium difficulty.",
                SceneName = "taco_truck",
                KitchenTheme = "mexican",
                AvailableRecipeIds = new[] { "burger", "pasta_dish", "toast" },
                StationCount = 5,
                Stations = new[]
                {
                    new StationLayout { StationId = "tt_ingredient1", Type = StationType.Ingredient, GridX = 0, GridY = 0 },
                    new StationLayout { StationId = "tt_ingredient2", Type = StationType.Ingredient, GridX = 0, GridY = 1 },
                    new StationLayout { StationId = "tt_prep", Type = StationType.Prep, GridX = 1, GridY = 0 },
                    new StationLayout { StationId = "tt_cooking", Type = StationType.Cooking, GridX = 2, GridY = 0 },
                    new StationLayout { StationId = "tt_serving", Type = StationType.Serving, GridX = 3, GridY = 0 },
                },
                Hazards = new MapHazardConfig
                {
                    FireChanceMultiplier = 1.0f,
                    HasSpecialHazards = false,
                    SpecialHazardType = null
                }
            });

            // ── Space Station — Zero-G throws, hard difficulty, 3v3 ──
            Add(new MapDefinition
            {
                MapId = "space_station",
                DisplayName = "Space Station",
                Description = "Sci-fi kitchen in zero gravity. Ingredients float and throws arc differently.",
                SceneName = "space_station",
                KitchenTheme = "scifi",
                AvailableRecipeIds = new[] { "sushi_roll", "ramen", "pizza" },
                StationCount = 5,
                Stations = new[]
                {
                    new StationLayout { StationId = "ss2_ingredient", Type = StationType.Ingredient, GridX = 0, GridY = 0 },
                    new StationLayout { StationId = "ss2_prep1", Type = StationType.Prep, GridX = 1, GridY = 0 },
                    new StationLayout { StationId = "ss2_prep2", Type = StationType.Prep, GridX = 1, GridY = 1 },
                    new StationLayout { StationId = "ss2_cooking", Type = StationType.Cooking, GridX = 2, GridY = 0 },
                    new StationLayout { StationId = "ss2_serving", Type = StationType.Serving, GridX = 3, GridY = 0 },
                },
                Hazards = new MapHazardConfig
                {
                    FireChanceMultiplier = 1.3f,
                    HasSpecialHazards = true,
                    SpecialHazardType = "zero_gravity"
                }
            });

            // ── Volcano Kitchen — Lava vent timers, hard difficulty, 3v3 ──
            Add(new MapDefinition
            {
                MapId = "volcano_kitchen",
                DisplayName = "Volcano Kitchen",
                Description = "Volcanic kitchen with lava vent timers. Extreme fire hazards.",
                SceneName = "volcano_kitchen",
                KitchenTheme = "volcanic",
                AvailableRecipeIds = new[] { "pizza", "ramen", "wedding_cake" },
                StationCount = 5,
                Stations = new[]
                {
                    new StationLayout { StationId = "vk_ingredient", Type = StationType.Ingredient, GridX = 0, GridY = 0 },
                    new StationLayout { StationId = "vk_prep", Type = StationType.Prep, GridX = 1, GridY = 0 },
                    new StationLayout { StationId = "vk_cooking1", Type = StationType.Cooking, GridX = 2, GridY = 0 },
                    new StationLayout { StationId = "vk_cooking2", Type = StationType.Cooking, GridX = 2, GridY = 1 },
                    new StationLayout { StationId = "vk_serving", Type = StationType.Serving, GridX = 3, GridY = 0 },
                },
                Hazards = new MapHazardConfig
                {
                    FireChanceMultiplier = 2.0f,
                    HasSpecialHazards = true,
                    SpecialHazardType = "lava_vent"
                }
            });

            // ── Clash Kitchen — Shared kitchen, hard difficulty, 3v3 ──
            Add(new MapDefinition
            {
                MapId = "clash_kitchen",
                DisplayName = "Clash Kitchen",
                Description = "Shared kitchen where both teams compete side by side. All recipes available.",
                SceneName = "clash_kitchen",
                KitchenTheme = "competition",
                AvailableRecipeIds = new[] { "toast", "fried_egg", "basic_salad", "burger", "pasta_dish", "sushi_roll", "ramen", "pizza", "wedding_cake" },
                StationCount = 6,
                Stations = new[]
                {
                    new StationLayout { StationId = "ck_ingredient1", Type = StationType.Ingredient, GridX = 0, GridY = 0 },
                    new StationLayout { StationId = "ck_ingredient2", Type = StationType.Ingredient, GridX = 0, GridY = 1 },
                    new StationLayout { StationId = "ck_prep", Type = StationType.Prep, GridX = 1, GridY = 0 },
                    new StationLayout { StationId = "ck_cooking1", Type = StationType.Cooking, GridX = 2, GridY = 0 },
                    new StationLayout { StationId = "ck_cooking2", Type = StationType.Cooking, GridX = 2, GridY = 1 },
                    new StationLayout { StationId = "ck_serving", Type = StationType.Serving, GridX = 3, GridY = 0 },
                },
                Hazards = new MapHazardConfig
                {
                    FireChanceMultiplier = 1.5f,
                    HasSpecialHazards = true,
                    SpecialHazardType = "shared_kitchen"
                }
            });
        }

        private void Add(MapDefinition def) => _maps[def.MapId] = def;

        public MapDefinition Get(string mapId)
        {
            return _maps.TryGetValue(mapId, out var def) ? def : null;
        }

        public IReadOnlyList<MapDefinition> GetAll()
        {
            return _maps.Values.ToList();
        }

        public IReadOnlyList<MapDefinition> GetByKitchenTheme(string kitchenTheme)
        {
            return _maps.Values.Where(m => m.KitchenTheme == kitchenTheme).ToList();
        }

        public int Count => _maps.Count;

        /// <summary>
        /// Applies remote config overrides to map hazard settings.
        /// </summary>
        public void ApplyRemoteConfig(MapConfig config)
        {
            if (config?.Overrides == null) return;

            foreach (var ov in config.Overrides)
            {
                if (string.IsNullOrEmpty(ov.MapId)) continue;
                if (!_maps.TryGetValue(ov.MapId, out var map)) continue;

                if (ov.FireChanceMultiplier >= 0)
                    map.Hazards.FireChanceMultiplier = ov.FireChanceMultiplier;

                if (ov.HasSpecialHazards.HasValue)
                    map.Hazards.HasSpecialHazards = ov.HasSpecialHazards.Value;

                GameLogger.Log($"[MapRegistry] Applied remote overrides for {ov.MapId}");
            }
        }
    }
}
