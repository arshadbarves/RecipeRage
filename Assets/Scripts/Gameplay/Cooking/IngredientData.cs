using UnityEngine;
using System;
using System.Collections.Generic;

namespace RecipeRage.Gameplay.Cooking
{
    /// <summary>
    /// ScriptableObject containing ingredient data
    /// </summary>
    [CreateAssetMenu(fileName = "New Ingredient", menuName = "RecipeRage/Ingredient Data")]
    public class IngredientData : ScriptableObject
    {
        [Header("Basic Info")]
        public string ingredientId;
        public string displayName;
        
        [Header("Visual States")]
        public Sprite rawIcon;
        public Sprite cookedIcon;
        public Sprite burntIcon;
        public GameObject rawPrefab;
        public GameObject cookedPrefab;
        public GameObject burntPrefab;
        
        [Header("Cooking Properties")]
        public List<CookingMethod> validCookingMethods;
        public float baseValue;
        public float cookingTime;
        public float burningThreshold;
        
        [Header("Combinations")]
        public List<string> validCombinations;

        /// <summary>
        /// Gets the appropriate icon for the current cooking state
        /// </summary>
        public Sprite GetIconForState(CookingState state)
        {
            return state switch
            {
                CookingState.Raw => rawIcon,
                CookingState.Cooked => cookedIcon,
                CookingState.Burnt => burntIcon,
                _ => rawIcon
            };
        }

        /// <summary>
        /// Gets the appropriate prefab for the current cooking state
        /// </summary>
        public GameObject GetPrefabForState(CookingState state)
        {
            return state switch
            {
                CookingState.Raw => rawPrefab,
                CookingState.Cooked => cookedPrefab,
                CookingState.Burnt => burntPrefab,
                _ => rawPrefab
            };
        }
    }
} 