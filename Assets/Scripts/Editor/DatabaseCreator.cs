using UnityEngine;
using UnityEditor;
using KitchenClash.Domain;
using KitchenClash.Application.Models;
using System.Linq;

namespace KitchenClash.Editor
{
    public static class DatabaseCreator
    {
        private const string CHEF_DB_PATH = "Assets/_KitchenClash/ScriptableObjects/ChefDatabase.asset";
        private const string MAP_DB_PATH = "Assets/_KitchenClash/ScriptableObjects/MapDatabase.asset";

        [MenuItem("KitchenClash/Create Databases")]
        public static void CreateDatabases()
        {
            try
            {
                AssetDatabase.StartAssetEditing();
                CreateChefDatabase();
                CreateMapDatabase();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[DatabaseCreator] Done! ChefDatabase + MapDatabase created.");
                Debug.Log("[DatabaseCreator] You can now delete this editor script via KitchenClash > Delete Database Creator.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DatabaseCreator] Failed: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        [MenuItem("KitchenClash/Create Databases", true)]
        public static bool ValidateCreateDatabases() => !EditorApplication.isPlaying;

        [MenuItem("KitchenClash/Delete Database Creator")]
        public static void DeleteSelf()
        {
            if (EditorUtility.DisplayDialog("Delete DatabaseCreator", "Delete the editor script?", "Yes", "Cancel"))
            {
                AssetDatabase.DeleteAsset("Assets/Scripts/Editor/DatabaseCreator.cs");
                AssetDatabase.Refresh();
                Debug.Log("[DatabaseCreator] Deleted.");
            }
        }

        private static void CreateChefDatabase()
        {
            var existing = AssetDatabase.LoadAssetAtPath<ChefDatabaseSO>(CHEF_DB_PATH);
            var database = existing != null ? existing : ScriptableObject.CreateInstance<ChefDatabaseSO>();

            database.Chefs.Clear();
            database.Chefs.Add(MakeChef(ChefId.Rosa, "Rosa", "Italian grandmother with decades of kitchen wisdom. A balanced all-rounder.",
                1.1f, 1.0f, 0.0f, 1, 1.0f, UnlockType.Starter, 0, null,
                AbilityType.QuickHands, AbilityType.SprintDash, AbilityType.KitchenRush, AbilityType.StickyMat));
            database.Chefs.Add(MakeChef(ChefId.Marco, "Marco", "Young culinary student who compensates with raw speed and hustle.",
                1.3f, 1.0f, 0.0f, 1, 1.0f, UnlockType.Wins, 5, null,
                AbilityType.LongToss, AbilityType.FlavorBurst, AbilityType.GrandService, AbilityType.RecipeShortcut));
            database.Chefs.Add(MakeChef(ChefId.Yuki, "Yuki", "Japanese sushi master. Precision over speed.",
                1.0f, 1.0f, 0.3f, 1, 1.1f, UnlockType.Trophies, 200, null,
                AbilityType.ZenFocus, AbilityType.CalmStep, AbilityType.PerfectPlating, AbilityType.FireproofGloves));
            database.Chefs.Add(MakeChef(ChefId.Grandpa, "Grandpa", "Retired head chef. Tough as nails, knows every trick in the book.",
                0.9f, 1.0f, 0.7f, 1, 1.0f, UnlockType.Matches, 20, null,
                AbilityType.SecretRecipe, AbilityType.StumbleCharge, AbilityType.FamilyFeast, AbilityType.VintageSpice));
            database.Chefs.Add(MakeChef(ChefId.Bella, "Bella", "Competitive pastry chef who orchestrates her team to perfection.",
                1.0f, 1.0f, 0.0f, 1, 1.2f, UnlockType.BattlePass, 30, "S1",
                AbilityType.Conductor, AbilityType.PrepRelay, AbilityType.Symphony, AbilityType.MiseEnPlace));
            database.Chefs.Add(MakeChef(ChefId.Raj, "Raj", "Street food vendor who juggles multiple orders with blazing cook speed.",
                1.0f, 1.2f, 0.0f, 2, 1.0f, UnlockType.Trophies, 500, null,
                AbilityType.HotHands, AbilityType.SpiceBlast, AbilityType.CurryOverdrive, AbilityType.PressureCooker));

            EnsureDirectory(CHEF_DB_PATH);
            if (existing == null) AssetDatabase.CreateAsset(database, CHEF_DB_PATH);
            else EditorUtility.SetDirty(database);
        }

        private static void CreateMapDatabase()
        {
            var existing = AssetDatabase.LoadAssetAtPath<MapDatabaseSO>(MAP_DB_PATH);
            var database = existing != null ? existing : ScriptableObject.CreateInstance<MapDatabaseSO>();

            database.Maps.Clear();
            database.Maps.Add(MakeMap("rookie_kitchen", "Rookie Kitchen", "A simple kitchen with no hazards. Perfect for beginners.",
                "rookie_kitchen", "basic", "2v2", MapDifficulty.Easy,
                new[] { "toast", "fried_egg", "basic_salad" }, 4,
                Stations("rk", StationType.Ingredient, StationType.Prep, StationType.Cooking, StationType.Serving),
                0f, false, null));
            database.Maps.Add(MakeMap("sushi_shuffle", "Sushi Shuffle", "Japanese kitchen with a conveyor belt and fresh fish market ingredients.",
                "sushi_shuffle", "japanese", "2v2", MapDifficulty.Medium,
                new[] { "sushi_roll", "ramen" }, 4,
                Stations("ss", StationType.Ingredient, StationType.Prep, StationType.Cooking, StationType.Serving),
                1.0f, true, "conveyor_belt"));
            database.Maps.Add(MakeMap("burger_boulevard", "Burger Boulevard", "American diner with a crosswalk hazard.",
                "burger_boulevard", "american", "2v2", MapDifficulty.Medium,
                new[] { "burger", "toast", "fried_egg" }, 4,
                Stations("bb", StationType.Ingredient, StationType.Prep, StationType.Cooking, StationType.Serving),
                0.8f, true, "crosswalk"));
            database.Maps.Add(MakeMap("pirate_pot", "Pirate Pot", "Pirate ship kitchen with sliding counters and seafood recipes.",
                "pirate_pot", "seafood", "2v2", MapDifficulty.Hard,
                new[] { "sushi_roll" }, 3,
                Stations("pp", StationType.Ingredient, StationType.Cooking, StationType.Serving),
                1.2f, true, "sliding_counters"));
            database.Maps.Add(MakeMap("taco_truck", "Taco Truck", "Dual food trucks with split prep areas. Mexican street food recipes.",
                "taco_truck", "mexican", "3v3", MapDifficulty.Medium,
                new[] { "taco", "burrito", "nachos" }, 5,
                Stations("tt", StationType.Ingredient, StationType.Prep, StationType.Ingredient, StationType.Prep, StationType.Serving),
                1.0f, true, "dual_trucks"));
            database.Maps.Add(MakeMap("space_station", "Space Station", "Orbital kitchen with zero-gravity ingredient throws.",
                "space_station", "scifi", "3v3", MapDifficulty.Hard,
                new[] { "space_ration", "astro_burger", "nebula_noodles" }, 4,
                Stations("sp", StationType.Ingredient, StationType.Prep, StationType.Cooking, StationType.Serving),
                1.3f, true, "zero_g_throws"));
            database.Maps.Add(MakeMap("volcano_kitchen", "Volcano Kitchen", "Kitchen on a volcanic island with periodic lava vent eruptions.",
                "volcano_kitchen", "volcanic", "3v3", MapDifficulty.Hard,
                new[] { "lava_cake", "fire_steak", "magma_soup" }, 4,
                Stations("vk", StationType.Ingredient, StationType.Prep, StationType.Cooking, StationType.Serving),
                1.8f, true, "lava_vent"));
            database.Maps.Add(MakeMap("clash_kitchen", "Clash Kitchen", "Both teams share one chaotic kitchen. Sabotage and strategy collide.",
                "clash_kitchen", "mixed", "3v3", MapDifficulty.Hard,
                new[] { "basic_salad", "toast", "fried_egg", "sushi_roll", "burger", "pasta_dish", "ramen" }, 6,
                Stations("ck", StationType.Ingredient, StationType.Prep, StationType.Cooking, StationType.Ingredient, StationType.Prep, StationType.Serving),
                1.5f, true, "shared_stations"));

            EnsureDirectory(MAP_DB_PATH);
            if (existing == null) AssetDatabase.CreateAsset(database, MAP_DB_PATH);
            else EditorUtility.SetDirty(database);
        }

        private static ChefDefinitionSO MakeChef(
            ChefId id, string name, string desc,
            float move, float cook, float burn, int carry, float score,
            UnlockType unlock, int val, string season,
            AbilityType passive, AbilityType active, AbilityType super, AbilityType gadget)
        {
            var chef = ScriptableObject.CreateInstance<ChefDefinitionSO>();
            chef.name = name;
            chef.Id = id;
            chef.DisplayName = name;
            chef.Description = desc;
            chef.MoveSpeed = move;
            chef.CookSpeedMult = cook;
            chef.BurnResistance = burn;
            chef.CarryCapacity = carry;
            chef.ScoreMultiplier = score;
            chef.UnlockType = unlock;
            chef.UnlockValue = val;
            chef.UnlockSeason = season;
            chef.PassiveAbility = passive;
            chef.ActiveAbility = active;
            chef.SuperAbility = super;
            chef.GadgetAbility = gadget;
            return chef;
        }

        private static MapDefinitionSO MakeMap(
            string id, string name, string desc,
            string scene, string theme, string mode, MapDifficulty diff,
            string[] recipes, int stations, StationLayout[] layout,
            float fire, bool special, string specialType)
        {
            var map = ScriptableObject.CreateInstance<MapDefinitionSO>();
            map.name = name;
            map.MapId = id;
            map.DisplayName = name;
            map.Description = desc;
            map.SceneName = scene;
            map.KitchenTheme = theme;
            map.GameMode = mode;
            map.Difficulty = diff;
            map.AvailableRecipeIds = recipes;
            map.StationCount = stations;
            map.Stations = layout;
            map.FireChanceMultiplier = fire;
            map.HasSpecialHazards = special;
            map.SpecialHazardType = specialType;
            return map;
        }

        private static StationLayout[] Stations(string prefix, params StationType[] types)
        {
            return types.Select((t, i) => new StationLayout
            {
                StationId = $"{prefix}_{t.ToString().ToLowerInvariant()}",
                Type = t,
                GridX = i % 4,
                GridY = i / 4
            }).ToArray();
        }

        private static void EnsureDirectory(string path)
        {
            string dir = System.IO.Path.GetDirectoryName(path);
            if (AssetDatabase.IsValidFolder(dir)) return;
            string[] parts = dir.Replace("\\", "/").Split('/');
            string cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = cur + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }
    }
}
