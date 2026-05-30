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
            // ── Rookie Kitchen — beginner 2v2, no hazards ──
            Add(new MapDefinition
            {
                MapId = "rookie_kitchen",
                DisplayName = "Rookie Kitchen",
                Description = "A simple kitchen with no hazards. Perfect for beginners.",
                SceneName = "rookie_kitchen",
                KitchenTheme = "basic",
                GameMode = "2v2",
                Difficulty = MapDifficulty.Easy,
                AvailableRecipeIds = new[] { "toast", "fried_egg", "basic_salad" },
                StationCount = 4,
                Stations = new[]
                {
                    new StationLayout { StationId = "rk_ingredient", Type = StationType.Ingredient, GridX = 0, GridY = 0 },
                    new StationLayout { StationId = "rk_prep", Type = StationType.Prep, GridX = 1, GridY = 0 },
                    new StationLayout { StationId = "rk_cooking", Type = StationType.Cooking, GridX = 2, GridY = 0 },
                    new StationLayout { StationId = "rk_serving", Type = StationType.Serving, GridX = 3, GridY = 0 },
                },
                Hazards = new MapHazardConfig
                {
                    FireChanceMultiplier = 0f,
                    HasSpecialHazards = false,
                    SpecialHazardType = null
                }
            });

            // ── Sushi Shuffle — Japanese kitchen, conveyor belt, 2v2 medium ──
            Add(new MapDefinition
            {
                MapId = "sushi_shuffle",
                DisplayName = "Sushi Shuffle",
                Description = "Japanese kitchen with a conveyor belt and fresh fish market ingredients.",
                SceneName = "sushi_shuffle",
                KitchenTheme = "japanese",
                GameMode = "2v2",
                Difficulty = MapDifficulty.Medium,
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
                    HasSpecialHazards = true,
                    SpecialHazardType = "conveyor_belt"
                }
            });

            // ── Burger Boulevard — American diner, crosswalk, 2v2 medium ──
            Add(new MapDefinition
            {
                MapId = "burger_boulevard",
                DisplayName = "Burger Boulevard",
                Description = "American diner with a crosswalk hazard. Burgers, toast, and egg recipes.",
                SceneName = "burger_boulevard",
                KitchenTheme = "american",
                GameMode = "2v2",
                Difficulty = MapDifficulty.Medium,
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
                    HasSpecialHazards = true,
                    SpecialHazardType = "crosswalk"
                }
            });

            // ── Pirate Pot — Ship kitchen, sliding counters, 2v2 hard ──
            Add(new MapDefinition
            {
                MapId = "pirate_pot",
                DisplayName = "Pirate Pot",
                Description = "Pirate ship kitchen with sliding counters and seafood recipes.",
                SceneName = "pirate_pot",
                KitchenTheme = "seafood",
                GameMode = "2v2",
                Difficulty = MapDifficulty.Hard,
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
                    HasSpecialHazards = true,
                    SpecialHazardType = "sliding_counters"
                }
            });

            // ── Taco Truck — Dual trucks, 3v3 medium ──
            Add(new MapDefinition
            {
                MapId = "taco_truck",
                DisplayName = "Taco Truck",
                Description = "Dual food trucks with split prep areas. Mexican street food recipes.",
                SceneName = "taco_truck",
                KitchenTheme = "mexican",
                GameMode = "3v3",
                Difficulty = MapDifficulty.Medium,
                AvailableRecipeIds = new[] { "taco", "burrito", "nachos" },
                StationCount = 5,
                Stations = new[]
                {
                    new StationLayout { StationId = "tt_ingredient_a", Type = StationType.Ingredient, GridX = 0, GridY = 0 },
                    new StationLayout { StationId = "tt_prep_a", Type = StationType.Prep, GridX = 1, GridY = 0 },
                    new StationLayout { StationId = "tt_ingredient_b", Type = StationType.Ingredient, GridX = 0, GridY = 1 },
                    new StationLayout { StationId = "tt_prep_b", Type = StationType.Prep, GridX = 1, GridY = 1 },
                    new StationLayout { StationId = "tt_serving", Type = StationType.Serving, GridX = 2, GridY = 0 },
                },
                Hazards = new MapHazardConfig
                {
                    FireChanceMultiplier = 1.0f,
                    HasSpecialHazards = true,
                    SpecialHazardType = "dual_trucks"
                }
            });

            // ── Space Station — Zero-G throws, 3v3 hard ──
            Add(new MapDefinition
            {
                MapId = "space_station",
                DisplayName = "Space Station",
                Description = "Orbital kitchen with zero-gravity ingredient throws and spread-out stations.",
                SceneName = "space_station",
                KitchenTheme = "scifi",
                GameMode = "3v3",
                Difficulty = MapDifficulty.Hard,
                AvailableRecipeIds = new[] { "space_ration", "astro_burger", "nebula_noodles" },
                StationCount = 4,
                Stations = new[]
                {
                    new StationLayout { StationId = "sp_ingredient", Type = StationType.Ingredient, GridX = 0, GridY = 0 },
                    new StationLayout { StationId = "sp_prep", Type = StationType.Prep, GridX = 3, GridY = 0 },
                    new StationLayout { StationId = "sp_cooking", Type = StationType.Cooking, GridX = 0, GridY = 3 },
                    new StationLayout { StationId = "sp_serving", Type = StationType.Serving, GridX = 3, GridY = 3 },
                },
                Hazards = new MapHazardConfig
                {
                    FireChanceMultiplier = 1.3f,
                    HasSpecialHazards = true,
                    SpecialHazardType = "zero_g_throws"
                }
            });

            // ── Volcano Kitchen — Lava vent timers, 3v3 hard ──
            Add(new MapDefinition
            {
                MapId = "volcano_kitchen",
                DisplayName = "Volcano Kitchen",
                Description = "Kitchen on a volcanic island with periodic lava vent eruptions.",
                SceneName = "volcano_kitchen",
                KitchenTheme = "volcanic",
                GameMode = "3v3",
                Difficulty = MapDifficulty.Hard,
                AvailableRecipeIds = new[] { "lava_cake", "fire_steak", "magma_soup" },
                StationCount = 4,
                Stations = new[]
                {
                    new StationLayout { StationId = "vk_ingredient", Type = StationType.Ingredient, GridX = 0, GridY = 0 },
                    new StationLayout { StationId = "vk_prep", Type = StationType.Prep, GridX = 1, GridY = 0 },
                    new StationLayout { StationId = "vk_cooking", Type = StationType.Cooking, GridX = 2, GridY = 0 },
                    new StationLayout { StationId = "vk_serving", Type = StationType.Serving, GridX = 3, GridY = 0 },
                },
                Hazards = new MapHazardConfig
                {
                    FireChanceMultiplier = 1.8f,
                    HasSpecialHazards = true,
                    SpecialHazardType = "lava_vent"
                }
            });

            // ── Clash Kitchen — Shared kitchen, 3v3 hard ──
            Add(new MapDefinition
            {
                MapId = "clash_kitchen",
                DisplayName = "Clash Kitchen",
                Description = "Both teams share one chaotic kitchen. Sabotage and strategy collide.",
                SceneName = "clash_kitchen",
                KitchenTheme = "mixed",
                GameMode = "3v3",
                Difficulty = MapDifficulty.Hard,
                AvailableRecipeIds = new[] { "basic_salad", "toast", "fried_egg", "sushi_roll", "burger", "pasta_dish", "ramen" },
                StationCount = 6,
                Stations = new[]
                {
                    new StationLayout { StationId = "ck_ingredient_a", Type = StationType.Ingredient, GridX = 0, GridY = 0 },
                    new StationLayout { StationId = "ck_prep_a", Type = StationType.Prep, GridX = 1, GridY = 0 },
                    new StationLayout { StationId = "ck_cooking_a", Type = StationType.Cooking, GridX = 2, GridY = 0 },
                    new StationLayout { StationId = "ck_ingredient_b", Type = StationType.Ingredient, GridX = 0, GridY = 1 },
                    new StationLayout { StationId = "ck_prep_b", Type = StationType.Prep, GridX = 1, GridY = 1 },
                    new StationLayout { StationId = "ck_serving", Type = StationType.Serving, GridX = 3, GridY = 0 },
                },
                Hazards = new MapHazardConfig
                {
                    FireChanceMultiplier = 1.5f,
                    HasSpecialHazards = true,
                    SpecialHazardType = "shared_stations"
                }
            });
        }

        private void Add(MapDefinition def) => _maps[def.MapId] = def;

        public MapDefinition Get(string mapId)
        {
            return _maps.TryGetValue(mapId, out MapDefinition def) ? def : null;
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
            if (config?.Overrides == null)
            {
                return;
            }

            foreach (MapOverride ov in config.Overrides)
            {
                if (string.IsNullOrEmpty(ov.MapId))
                {
                    continue;
                }

                if (!_maps.TryGetValue(ov.MapId, out MapDefinition map))
                {
                    continue;
                }

                if (ov.FireChanceMultiplier >= 0)
                {
                    map.Hazards.FireChanceMultiplier = ov.FireChanceMultiplier;
                }

                if (ov.HasSpecialHazards.HasValue)
                {
                    map.Hazards.HasSpecialHazards = ov.HasSpecialHazards.Value;
                }

                GameLogger.Log($"[MapRegistry] Applied remote overrides for {ov.MapId}");
            }
        }
    }
}
