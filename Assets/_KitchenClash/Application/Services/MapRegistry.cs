using System.Collections.Generic;
using System.Linq;
using KitchenClash.Domain;

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
                KitchenTheme = "japanese",
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
                StationCount = 4,
                Stations = new[]
                {
                    new StationLayout { StationId = "hk_ingredient", Type = StationType.Ingredient, GridX = 0, GridY = 0 },
                    new StationLayout { StationId = "hk_prep", Type = StationType.Prep, GridX = 1, GridY = 0 },
                    new StationLayout { StationId = "hk_cooking", Type = StationType.Cooking, GridX = 2, GridY = 0 },
                    new StationLayout { StationId = "hk_sink", Type = StationType.Sink, GridX = 1, GridY = 1 },
                },
                Hazards = new MapHazardConfig
                {
                    FireChanceMultiplier = 1.5f,
                    HasSpecialHazards = true,
                    SpecialHazardType = "ghost_ingredient"
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
    }
}
