using UnityEngine;
using UnityEditor;
using KitchenClash.Domain;
using KitchenClash.Application.Models;

namespace KitchenClash.Editor
{
    public static class DatabaseCreator
    {
        [MenuItem("KitchenClash/Create Databases")]
        public static void CreateDatabases()
        {
            CreateChefDatabase();
            CreateMapDatabase();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[DatabaseCreator] Databases created successfully");
        }

        private static void CreateChefDatabase()
        {
            var database = ScriptableObject.CreateInstance<ChefDatabaseSO>();

            database.Chefs.Add(CreateChef(ChefId.Rosa, "Rosa", "Italian grandmother with decades of kitchen wisdom. A balanced all-rounder.",
                1.1f, 1.0f, 0.0f, 1, 1.0f, UnlockType.Starter, 0, null, 0,
                AbilityType.QuickHands, AbilityType.SprintDash, AbilityType.KitchenRush, AbilityType.StickyMat));

            database.Chefs.Add(CreateChef(ChefId.Marco, "Marco", "Young culinary student who compensates with raw speed and hustle.",
                1.3f, 1.0f, 0.0f, 1, 1.0f, UnlockType.Wins, 5, null, 0,
                AbilityType.LongToss, AbilityType.FlavorBurst, AbilityType.GrandService, AbilityType.RecipeShortcut));

            database.Chefs.Add(CreateChef(ChefId.Yuki, "Yuki", "Japanese sushi master. Precision over speed — nothing burns on her watch.",
                1.0f, 1.0f, 0.3f, 1, 1.1f, UnlockType.Trophies, 200, null, 0,
                AbilityType.ZenFocus, AbilityType.CalmStep, AbilityType.PerfectPlating, AbilityType.FireproofGloves));

            database.Chefs.Add(CreateChef(ChefId.Grandpa, "Grandpa", "Retired head chef. Tough as nails, knows every trick in the book.",
                0.9f, 1.0f, 0.7f, 1, 1.0f, UnlockType.Matches, 20, null, 0,
                AbilityType.SecretRecipe, AbilityType.StumbleCharge, AbilityType.FamilyFeast, AbilityType.VintageSpice));

            database.Chefs.Add(CreateChef(ChefId.Bella, "Bella", "Competitive pastry chef who orchestrates her team to perfection.",
                1.0f, 1.0f, 0.0f, 1, 1.2f, UnlockType.BattlePass, 30, "S1", 0,
                AbilityType.Conductor, AbilityType.PrepRelay, AbilityType.Symphony, AbilityType.MiseEnPlace));

            database.Chefs.Add(CreateChef(ChefId.Raj, "Raj", "Street food vendor who juggles multiple orders with blazing cook speed.",
                1.0f, 1.2f, 0.0f, 2, 1.0f, UnlockType.Trophies, 500, null, 0,
                AbilityType.HotHands, AbilityType.SpiceBlast, AbilityType.CurryOverdrive, AbilityType.PressureCooker));

            string path = "Assets/_KitchenClash/ScriptableObjects/ChefDatabase.asset";
            AssetDatabase.CreateAsset(database, path);
        }

        private static ChefDefinitionSO CreateChef(
            ChefId id, string displayName, string description,
            float moveSpeed, float cookSpeed, float burnResist, int carryCap, float scoreMult,
            UnlockType unlockType, int unlockValue, string unlockSeason, int unlockBP,
            AbilityType passive, AbilityType active, AbilityType super, AbilityType gadget)
        {
            var chef = ScriptableObject.CreateInstance<ChefDefinitionSO>();
            chef.name = displayName;
            chef.Id = id;
            chef.DisplayName = displayName;
            chef.Description = description;
            chef.MoveSpeed = moveSpeed;
            chef.CookSpeedMult = cookSpeed;
            chef.BurnResistance = burnResist;
            chef.CarryCapacity = carryCap;
            chef.ScoreMultiplier = scoreMult;
            chef.UnlockType = unlockType;
            chef.UnlockValue = unlockValue;
            chef.UnlockSeason = unlockSeason;
            chef.UnlockBattlePassTier = unlockBP;
            chef.PassiveAbility = passive;
            chef.ActiveAbility = active;
            chef.SuperAbility = super;
            chef.GadgetAbility = gadget;
            return chef;
        }

        private static void CreateMapDatabase()
        {
            var database = ScriptableObject.CreateInstance<MapDatabaseSO>();

            database.Maps.Add(CreateMap("rookie_kitchen", "Rookie Kitchen", "A simple kitchen with no hazards. Perfect for beginners.",
                "rookie_kitchen", "basic", "2v2", MapDifficulty.Easy,
                new[] { "toast", "fried_egg", "basic_salad" }, 4,
                CreateStations("rk", new[] { StationType.Ingredient, StationType.Prep, StationType.Cooking, StationType.Serving }),
                0f, false, null));

            database.Maps.Add(CreateMap("sushi_shuffle", "Sushi Shuffle", "Japanese kitchen with a conveyor belt and fresh fish market ingredients.",
                "sushi_shuffle", "japanese", "2v2", MapDifficulty.Medium,
                new[] { "sushi_roll", "ramen" }, 4,
                CreateStations("ss", new[] { StationType.Ingredient, StationType.Prep, StationType.Cooking, StationType.Serving }),
                1.0f, true, "conveyor_belt"));

            database.Maps.Add(CreateMap("burger_boulevard", "Burger Boulevard", "American diner with a crosswalk hazard. Burgers, toast, and egg recipes.",
                "burger_boulevard", "american", "2v2", MapDifficulty.Medium,
                new[] { "burger", "toast", "fried_egg" }, 4,
                CreateStations("bb", new[] { StationType.Ingredient, StationType.Prep, StationType.Cooking, StationType.Serving }),
                0.8f, true, "crosswalk"));

            database.Maps.Add(CreateMap("pirate_pot", "Pirate Pot", "Pirate ship kitchen with sliding counters and seafood recipes.",
                "pirate_pot", "seafood", "2v2", MapDifficulty.Hard,
                new[] { "sushi_roll" }, 3,
                CreateStations("pp", new[] { StationType.Ingredient, StationType.Cooking, StationType.Serving }),
                1.2f, true, "sliding_counters"));

            database.Maps.Add(CreateMap("taco_truck", "Taco Truck", "Dual food trucks with split prep areas. Mexican street food recipes.",
                "taco_truck", "mexican", "3v3", MapDifficulty.Medium,
                new[] { "taco", "burrito", "nachos" }, 5,
                CreateDualTruckStations(),
                1.0f, true, "dual_trucks"));

            database.Maps.Add(CreateMap("space_station", "Space Station", "Orbital kitchen with zero-gravity ingredient throws and spread-out stations.",
                "space_station", "scifi", "3v3", MapDifficulty.Hard,
                new[] { "space_ration", "astro_burger", "nebula_noodles" }, 4,
                CreateSpreadStations("sp"),
                1.3f, true, "zero_g_throws"));

            database.Maps.Add(CreateMap("volcano_kitchen", "Volcano Kitchen", "Kitchen on a volcanic island with periodic lava vent eruptions.",
                "volcano_kitchen", "volcanic", "3v3", MapDifficulty.Hard,
                new[] { "lava_cake", "fire_steak", "magma_soup" }, 4,
                CreateStations("vk", new[] { StationType.Ingredient, StationType.Prep, StationType.Cooking, StationType.Serving }),
                1.8f, true, "lava_vent"));

            database.Maps.Add(CreateMap("clash_kitchen", "Clash Kitchen", "Both teams share one chaotic kitchen. Sabotage and strategy collide.",
                "clash_kitchen", "mixed", "3v3", MapDifficulty.Hard,
                new[] { "basic_salad", "toast", "fried_egg", "sushi_roll", "burger", "pasta_dish", "ramen" }, 6,
                CreateSharedStations(),
                1.5f, true, "shared_stations"));

            string path = "Assets/_KitchenClash/ScriptableObjects/MapDatabase.asset";
            AssetDatabase.CreateAsset(database, path);
        }

        private static MapDefinitionSO CreateMap(
            string mapId, string displayName, string description,
            string sceneName, string kitchenTheme, string gameMode, MapDifficulty difficulty,
            string[] recipes, int stationCount, StationLayout[] stations,
            float fireChance, bool hasSpecial, string specialType)
        {
            var map = ScriptableObject.CreateInstance<MapDefinitionSO>();
            map.name = displayName;
            map.MapId = mapId;
            map.DisplayName = displayName;
            map.Description = description;
            map.SceneName = sceneName;
            map.KitchenTheme = kitchenTheme;
            map.GameMode = gameMode;
            map.Difficulty = difficulty;
            map.AvailableRecipeIds = recipes;
            map.StationCount = stationCount;
            map.Stations = stations;
            map.FireChanceMultiplier = fireChance;
            map.HasSpecialHazards = hasSpecial;
            map.SpecialHazardType = specialType;
            return map;
        }

        private static StationLayout[] CreateStations(string prefix, StationType[] types)
        {
            var stations = new StationLayout[types.Length];
            for (int i = 0; i < types.Length; i++)
            {
                stations[i] = new StationLayout
                {
                    StationId = $"{prefix}_{types[i].ToString().ToLowerInvariant()}",
                    Type = types[i],
                    GridX = i,
                    GridY = 0
                };
            }
            return stations;
        }

        private static StationLayout[] CreateDualTruckStations()
        {
            return new[]
            {
                new StationLayout { StationId = "tt_ingredient_a", Type = StationType.Ingredient, GridX = 0, GridY = 0 },
                new StationLayout { StationId = "tt_prep_a", Type = StationType.Prep, GridX = 1, GridY = 0 },
                new StationLayout { StationId = "tt_ingredient_b", Type = StationType.Ingredient, GridX = 0, GridY = 1 },
                new StationLayout { StationId = "tt_prep_b", Type = StationType.Prep, GridX = 1, GridY = 1 },
                new StationLayout { StationId = "tt_serving", Type = StationType.Serving, GridX = 2, GridY = 0 },
            };
        }

        private static StationLayout[] CreateSpreadStations(string prefix)
        {
            return new[]
            {
                new StationLayout { StationId = $"{prefix}_ingredient", Type = StationType.Ingredient, GridX = 0, GridY = 0 },
                new StationLayout { StationId = $"{prefix}_prep", Type = StationType.Prep, GridX = 3, GridY = 0 },
                new StationLayout { StationId = $"{prefix}_cooking", Type = StationType.Cooking, GridX = 0, GridY = 3 },
                new StationLayout { StationId = $"{prefix}_serving", Type = StationType.Serving, GridX = 3, GridY = 3 },
            };
        }

        private static StationLayout[] CreateSharedStations()
        {
            return new[]
            {
                new StationLayout { StationId = "ck_ingredient_a", Type = StationType.Ingredient, GridX = 0, GridY = 0 },
                new StationLayout { StationId = "ck_prep_a", Type = StationType.Prep, GridX = 1, GridY = 0 },
                new StationLayout { StationId = "ck_cooking_a", Type = StationType.Cooking, GridX = 2, GridY = 0 },
                new StationLayout { StationId = "ck_ingredient_b", Type = StationType.Ingredient, GridX = 0, GridY = 1 },
                new StationLayout { StationId = "ck_prep_b", Type = StationType.Prep, GridX = 1, GridY = 1 },
                new StationLayout { StationId = "ck_serving", Type = StationType.Serving, GridX = 3, GridY = 0 },
            };
        }
    }
}
