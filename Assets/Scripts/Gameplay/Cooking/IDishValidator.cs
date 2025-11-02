using System.Collections.Generic;

namespace Gameplay.Cooking
{
    /// <summary>
    /// Interface for dish validation strategies.
    /// Follows Strategy Pattern - different validation strategies per game mode.
    /// </summary>
    public interface IDishValidator
    {
        /// <summary>
        /// Validate if the ingredients match the recipe requirements.
        /// </summary>
        /// <param name="ingredients">The list of ingredients</param>
        /// <param name="recipe">The target recipe</param>
        /// <returns>True if the dish is valid</returns>
        bool ValidateDish(List<IngredientItem> ingredients, Recipe recipe);
        
        /// <summary>
        /// Get the quality of the assembled dish.
        /// </summary>
        /// <param name="ingredients">The list of ingredients</param>
        /// <param name="recipe">The target recipe</param>
        /// <returns>The dish quality</returns>
        DishQuality GetDishQuality(List<IngredientItem> ingredients, Recipe recipe);
        
        /// <summary>
        /// Calculate the score for the dish.
        /// </summary>
        /// <param name="ingredients">The list of ingredients</param>
        /// <param name="recipe">The target recipe</param>
        /// <param name="timeRemaining">The time remaining when dish was completed</param>
        /// <returns>The calculated score</returns>
        int CalculateScore(List<IngredientItem> ingredients, Recipe recipe, float timeRemaining);
    }
    
    /// <summary>
    /// Dish quality levels.
    /// </summary>
    public enum DishQuality
    {
        Perfect,    // All ingredients correct, perfect preparation
        Good,       // All ingredients correct, good preparation
        Acceptable, // All ingredients correct, acceptable preparation
        Wrong       // Missing or incorrect ingredients
    }
}
