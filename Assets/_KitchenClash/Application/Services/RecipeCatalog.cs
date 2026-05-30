using System.Collections.Generic;
using System.Linq;
using KitchenClash.Domain;

namespace KitchenClash.Application.Services
{
    /// <summary>
    /// Registry of all recipe definitions available in the game.
    /// Provides lookup by ID, tier, and kitchen theme.
    /// </summary>
    public sealed class RecipeCatalog
    {
        private readonly Dictionary<string, RecipeDefinition> _recipes = new();

        public RecipeCatalog(IConfigService config)
        {
            float baseTime = config?.Get("order_base_time", 30f) ?? 30f;
            BuildDefaultRecipes(baseTime);
        }

        /// <summary>
        /// Internal constructor for testing without IConfigService.
        /// </summary>
        internal RecipeCatalog(float baseTime)
        {
            BuildDefaultRecipes(baseTime);
        }

        private void BuildDefaultRecipes(float baseTime)
        {
            // ── Tier 1 — simple, 2 ingredients ──
            Add(new RecipeDefinition
            {
                RecipeId = "basic_salad",
                DisplayName = "Basic Salad",
                Tier = 1,
                RequiredIngredients = new[] { IngredientType.Lettuce, IngredientType.Tomato },
                BaseTimeLimitSec = baseTime + 10f,
                KitchenTheme = "bistro",
                BasePoints = 50
            });

            Add(new RecipeDefinition
            {
                RecipeId = "toast",
                DisplayName = "Toast",
                Tier = 1,
                RequiredIngredients = new[] { IngredientType.Bread, IngredientType.Butter },
                BaseTimeLimitSec = baseTime + 10f,
                KitchenTheme = "bistro",
                BasePoints = 50
            });

            Add(new RecipeDefinition
            {
                RecipeId = "fried_egg",
                DisplayName = "Fried Egg",
                Tier = 1,
                RequiredIngredients = new[] { IngredientType.Egg, IngredientType.Butter },
                BaseTimeLimitSec = baseTime + 10f,
                KitchenTheme = "bistro",
                BasePoints = 50
            });

            // ── Tier 2 — medium, 3-4 ingredients ──
            Add(new RecipeDefinition
            {
                RecipeId = "sushi_roll",
                DisplayName = "Sushi Roll",
                Tier = 2,
                RequiredIngredients = new[] { IngredientType.Rice, IngredientType.Fish, IngredientType.Seaweed },
                BaseTimeLimitSec = baseTime + 20f,
                KitchenTheme = "japanese",
                BasePoints = 100
            });

            Add(new RecipeDefinition
            {
                RecipeId = "burger",
                DisplayName = "Burger",
                Tier = 2,
                RequiredIngredients = new[] { IngredientType.Bread, IngredientType.Beef, IngredientType.Lettuce, IngredientType.Tomato },
                BaseTimeLimitSec = baseTime + 20f,
                KitchenTheme = "american",
                BasePoints = 100
            });

            Add(new RecipeDefinition
            {
                RecipeId = "pasta_dish",
                DisplayName = "Pasta",
                Tier = 2,
                RequiredIngredients = new[] { IngredientType.Pasta, IngredientType.Sauce, IngredientType.Cheese },
                BaseTimeLimitSec = baseTime + 20f,
                KitchenTheme = "italian",
                BasePoints = 100
            });

            // ── Tier 3 — complex, 4-5 ingredients ──
            Add(new RecipeDefinition
            {
                RecipeId = "pizza",
                DisplayName = "Pizza",
                Tier = 3,
                RequiredIngredients = new[] { IngredientType.Dough, IngredientType.Sauce, IngredientType.Cheese, IngredientType.Tomato, IngredientType.Vegetables },
                BaseTimeLimitSec = baseTime + 30f,
                KitchenTheme = "italian",
                BasePoints = 150
            });

            Add(new RecipeDefinition
            {
                RecipeId = "ramen",
                DisplayName = "Ramen",
                Tier = 3,
                RequiredIngredients = new[] { IngredientType.Noodles, IngredientType.Broth, IngredientType.Egg, IngredientType.Vegetables },
                BaseTimeLimitSec = baseTime + 30f,
                KitchenTheme = "japanese",
                BasePoints = 150
            });

            Add(new RecipeDefinition
            {
                RecipeId = "wedding_cake",
                DisplayName = "Wedding Cake",
                Tier = 3,
                RequiredIngredients = new[] { IngredientType.Dough, IngredientType.Egg, IngredientType.Cream, IngredientType.Frosting, IngredientType.Butter },
                BaseTimeLimitSec = baseTime + 30f,
                KitchenTheme = "bakery",
                BasePoints = 150
            });
        }

        private void Add(RecipeDefinition def) => _recipes[def.RecipeId] = def;

        public RecipeDefinition Get(string recipeId)
        {
            return _recipes.TryGetValue(recipeId, out RecipeDefinition def) ? def : null;
        }

        public IReadOnlyList<RecipeDefinition> GetByTier(int tier)
        {
            return _recipes.Values.Where(r => r.Tier == tier).ToList();
        }

        public IReadOnlyList<RecipeDefinition> GetByKitchen(string kitchenTheme)
        {
            return _recipes.Values.Where(r => r.KitchenTheme == kitchenTheme).ToList();
        }

        public IReadOnlyList<RecipeDefinition> GetAll()
        {
            return _recipes.Values.ToList();
        }

        public int Count => _recipes.Count;
    }
}
