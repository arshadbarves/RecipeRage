using System;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeRage.Gameplay.Cooking
{
    /// <summary>
    /// Represents a recipe that can be prepared in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "New Recipe", menuName = "RecipeRage/Cooking/Recipe")]
    public class Recipe : ScriptableObject
    {
        [Header("Basic Properties")]
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private int _pointValue = 100;
        [SerializeField] private float _baseTimeLimit = 60f;

        [Header("Ingredients")]
        [SerializeField] private List<RecipeIngredient> _ingredients = new List<RecipeIngredient>();

        [Header("Difficulty")]
        [SerializeField] private RecipeDifficulty _difficulty = RecipeDifficulty.Easy;

        /// <summary>
        /// The unique ID of this recipe.
        /// </summary>
        public int Id => _id;
        [SerializeField] private int _id;

        /// <summary>
        /// The display name of the recipe.
        /// </summary>
        public string DisplayName => _displayName;

        /// <summary>
        /// The icon representing the recipe.
        /// </summary>
        public Sprite Icon => _icon;

        /// <summary>
        /// The point value awarded for completing this recipe.
        /// </summary>
        public int PointValue => _pointValue;

        /// <summary>
        /// The base time limit for completing this recipe.
        /// </summary>
        public float BaseTimeLimit => _baseTimeLimit;

        /// <summary>
        /// The ingredients required for this recipe.
        /// </summary>
        public IReadOnlyList<RecipeIngredient> Ingredients => _ingredients;

        /// <summary>
        /// The difficulty level of this recipe.
        /// </summary>
        public RecipeDifficulty Difficulty => _difficulty;

        /// <summary>
        /// Validates the recipe data.
        /// </summary>
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_displayName))
            {
                _displayName = name;
            }

            if (_id == 0)
            {
                // Generate a simple hash code for the recipe name
                _id = name.GetHashCode();
            }
        }
    }

    /// <summary>
    /// Represents an ingredient in a recipe with specific preparation requirements.
    /// </summary>
    [Serializable]
    public class RecipeIngredient
    {
        [SerializeField] private Ingredient _ingredient;
        [SerializeField] private bool _requireCut = false;
        [SerializeField] private bool _requireCooked = false;

        /// <summary>
        /// The ingredient.
        /// </summary>
        public Ingredient Ingredient => _ingredient;

        /// <summary>
        /// Whether the ingredient needs to be cut.
        /// </summary>
        public bool RequireCut => _requireCut;

        /// <summary>
        /// Whether the ingredient needs to be cooked.
        /// </summary>
        public bool RequireCooked => _requireCooked;
    }

    /// <summary>
    /// Difficulty levels for recipes.
    /// </summary>
    public enum RecipeDifficulty
    {
        Easy,
        Medium,
        Hard,
        Expert
    }

    // RecipeOrderState has been moved to its own file
}
