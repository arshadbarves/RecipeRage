using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Network.Cooking;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay
{
    public class StandardDishValidator : IDishValidator
    {
        private const int PERFECT_SCORE_MULTIPLIER = 2;
        private const int GOOD_SCORE_MULTIPLIER = 1;
        private const float ACCEPTABLE_SCORE_MULTIPLIER = 0.5f;
        private const int TIME_BONUS_PER_SECOND = 2;

        public bool ValidateDish(IList ingredients, object recipe)
        {
            if (ingredients == null || recipe == null || ingredients.Count == 0)
                return false;

            var required = ExtractRequiredIds(recipe);
            if (required == null || required.Count == 0)
                return false;

            var provided = ExtractProvidedIds(ingredients);

            // Every required ingredient must be present
            return required.All(id => provided.Contains(id));
        }

        public DishQuality GetDishQuality(IList ingredients, object recipe)
        {
            if (!ValidateDish(ingredients, recipe)) return DishQuality.Wrong;

            var required = ExtractRequiredIds(recipe);
            var provided = ExtractProvidedIds(ingredients);

            if (provided.Count == required.Count)
                return DishQuality.Perfect;

            // Extra ingredients: still good but not perfect
            if (provided.Count <= required.Count + 1)
                return DishQuality.Good;

            return DishQuality.Acceptable;
        }

        public int CalculateScore(IList ingredients, object recipe, float timeRemaining)
        {
            var quality = GetDishQuality(ingredients, recipe);
            if (quality == DishQuality.Wrong) return 0;

            int score = 100;
            switch (quality)
            {
                case DishQuality.Perfect: score *= PERFECT_SCORE_MULTIPLIER; break;
                case DishQuality.Good: score *= GOOD_SCORE_MULTIPLIER; break;
                case DishQuality.Acceptable: score = Mathf.RoundToInt(score * ACCEPTABLE_SCORE_MULTIPLIER); break;
            }

            score += Mathf.RoundToInt(timeRemaining * TIME_BONUS_PER_SECOND);
            return Mathf.Max(score, 0);
        }

        /// <summary>
        /// Extract required ingredient IDs from a recipe object.
        /// Supports both Recipe (ScriptableObject) and RecipeDefinition (domain model).
        /// </summary>
        private static List<int> ExtractRequiredIds(object recipe)
        {
            if (recipe is RecipeDefinition def)
            {
                return def.RequiredIngredients.Select(i => (int)i).ToList();
            }

            if (recipe is Recipe soRecipe)
            {
                return soRecipe.Ingredients
                    .Where(ri => ri.Ingredient != null)
                    .Select(ri => ri.Ingredient.Id)
                    .ToList();
            }

            return null;
        }

        /// <summary>
        /// Extract provided ingredient IDs from a list of ingredients on the plate.
        /// </summary>
        private static HashSet<int> ExtractProvidedIds(IList ingredients)
        {
            var ids = new HashSet<int>();
            foreach (var item in ingredients)
            {
                if (item is IngredientItem ingredientItem && ingredientItem.Ingredient != null)
                {
                    ids.Add(ingredientItem.Ingredient.Id);
                }
            }
            return ids;
        }
    }
}
