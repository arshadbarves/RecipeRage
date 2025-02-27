using UnityEngine;
using System;
using System.Collections.Generic;

namespace RecipeRage.Gameplay.Cooking
{
    /// <summary>
    /// Validates ingredients and manages cooking states
    /// </summary>
    public class CookingValidator
    {
        private readonly Dictionary<string, IngredientData> _ingredientLookup = new Dictionary<string, IngredientData>();

        public CookingValidator(IEnumerable<IngredientData> ingredientDatabase)
        {
            InitializeLookup(ingredientDatabase);
        }

        private void InitializeLookup(IEnumerable<IngredientData> ingredients)
        {
            _ingredientLookup.Clear();
            foreach (var ingredient in ingredients)
            {
                _ingredientLookup[ingredient.ingredientId] = ingredient;
            }
        }

        /// <summary>
        /// Validates if an ingredient can be cooked using a specific method
        /// </summary>
        public bool ValidateIngredient(string ingredientId, CookingMethod method)
        {
            if (!_ingredientLookup.TryGetValue(ingredientId, out IngredientData data))
                return false;

            return data.validCookingMethods.Contains(method);
        }

        /// <summary>
        /// Gets the cooking state based on current cooking time
        /// </summary>
        public CookingState GetCookingState(string ingredientId, float currentCookingTime)
        {
            if (!_ingredientLookup.TryGetValue(ingredientId, out IngredientData data))
                return CookingState.Raw;

            if (currentCookingTime < data.cookingTime * 0.5f)
                return CookingState.Raw;
            
            if (currentCookingTime > data.cookingTime + data.burningThreshold)
                return CookingState.Burnt;
            
            return CookingState.Cooked;
        }

        /// <summary>
        /// Gets the cooking time for an ingredient
        /// </summary>
        public float GetCookingTime(string ingredientId)
        {
            return _ingredientLookup.TryGetValue(ingredientId, out IngredientData data) 
                ? data.cookingTime 
                : 0f;
        }

        /// <summary>
        /// Gets the burning threshold for an ingredient
        /// </summary>
        public float GetBurningThreshold(string ingredientId)
        {
            return _ingredientLookup.TryGetValue(ingredientId, out IngredientData data) 
                ? data.burningThreshold 
                : 0f;
        }
    }

    /// <summary>
    /// Possible states of food during cooking
    /// </summary>
    public enum CookingState
    {
        Raw,
        Cooked,
        Burnt
    }

    /// <summary>
    /// Available cooking methods
    /// </summary>
    public enum CookingMethod
    {
        None,
        Fry,
        Boil,
        Bake,
        Grill,
        Steam,
        Chop
    }
} 