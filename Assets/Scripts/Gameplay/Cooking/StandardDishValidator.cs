using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.Cooking
{
    /// <summary>
    /// Standard dish validation implementation.
    /// Follows Strategy Pattern - standard validation logic.
    /// </summary>
    public class StandardDishValidator : IDishValidator
    {
        private const int PERFECT_SCORE_MULTIPLIER = 2;
        private const int GOOD_SCORE_MULTIPLIER = 1;
        private const float ACCEPTABLE_SCORE_MULTIPLIER = 0.5f;
        private const int TIME_BONUS_PER_SECOND = 2;
        
        /// <summary>
        /// Validate if the ingredients match the recipe requirements.
        /// </summary>
        public bool ValidateDish(List<IngredientItem> ingredients, Recipe recipe)
        {
            if (ingredients == null || recipe == null)
            {
                return false;
            }
            
            // Check if we have the right number of ingredients
            if (ingredients.Count != recipe.Ingredients.Count)
            {
                return false;
            }
            
            // Check if all required ingredients are present
            foreach (RecipeIngredient requiredIngredient in recipe.Ingredients)
            {
                bool found = false;
                
                foreach (IngredientItem ingredient in ingredients)
                {
                    if (ingredient.Ingredient.Id == requiredIngredient.Ingredient.Id)
                    {
                        // Check if preparation matches requirements
                        if (requiredIngredient.RequireCut && !ingredient.IsCut)
                        {
                            continue;
                        }
                        
                        if (requiredIngredient.RequireCooked && !ingredient.IsCooked)
                        {
                            continue;
                        }
                        
                        // Check if ingredient is burned (always invalid)
                        if (ingredient.IsBurned)
                        {
                            continue;
                        }
                        
                        found = true;
                        break;
                    }
                }
                
                if (!found)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Get the quality of the assembled dish.
        /// </summary>
        public DishQuality GetDishQuality(List<IngredientItem> ingredients, Recipe recipe)
        {
            if (!ValidateDish(ingredients, recipe))
            {
                return DishQuality.Wrong;
            }
            
            // Check for perfect preparation
            bool isPerfect = true;
            bool isGood = true;
            
            foreach (RecipeIngredient requiredIngredient in recipe.Ingredients)
            {
                IngredientItem ingredient = ingredients.FirstOrDefault(i => 
                    i.Ingredient.Id == requiredIngredient.Ingredient.Id);
                
                if (ingredient == null)
                {
                    continue;
                }
                
                // Check preparation quality
                IngredientState state = ingredient.GetState();
                
                // Perfect: exactly as required
                if (requiredIngredient.RequireCut && state.CuttingProgress < 1.0f)
                {
                    isPerfect = false;
                }
                
                if (requiredIngredient.RequireCooked)
                {
                    // Perfect cooking: 0.9-1.0
                    if (state.CookingProgress < 0.9f || state.CookingProgress > 1.0f)
                    {
                        isPerfect = false;
                    }
                    
                    // Good cooking: 0.8-1.1
                    if (state.CookingProgress < 0.8f || state.CookingProgress > 1.1f)
                    {
                        isGood = false;
                    }
                }
            }
            
            if (isPerfect)
            {
                return DishQuality.Perfect;
            }
            else if (isGood)
            {
                return DishQuality.Good;
            }
            else
            {
                return DishQuality.Acceptable;
            }
        }
        
        /// <summary>
        /// Calculate the score for the dish.
        /// </summary>
        public int CalculateScore(List<IngredientItem> ingredients, Recipe recipe, float timeRemaining)
        {
            DishQuality quality = GetDishQuality(ingredients, recipe);
            
            if (quality == DishQuality.Wrong)
            {
                return 0;
            }
            
            // Base score from recipe
            int score = recipe.PointValue;
            
            // Apply quality multiplier
            switch (quality)
            {
                case DishQuality.Perfect:
                    score *= PERFECT_SCORE_MULTIPLIER;
                    break;
                case DishQuality.Good:
                    score *= GOOD_SCORE_MULTIPLIER;
                    break;
                case DishQuality.Acceptable:
                    score = Mathf.RoundToInt(score * ACCEPTABLE_SCORE_MULTIPLIER);
                    break;
            }
            
            // Add time bonus
            int timeBonus = Mathf.RoundToInt(timeRemaining * TIME_BONUS_PER_SECOND);
            score += timeBonus;
            
            return Mathf.Max(score, 0);
        }
    }
}
